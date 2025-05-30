using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using BCrypt.Net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using TyutyunnikovaAnna_Diplom.Context;
using TyutyunnikovaAnna_Diplom.Models;

namespace TyutyunnikovaAnna_Diplom;

public partial class AddBreeder : Window
{
    public ObservableCollection<TrainingType> selected_Types { get; set; } = new ObservableCollection<TrainingType>(); //сохраняем все выбранные тренировки берейтора
    public ObservableCollection<TrainingType> Trainingtypes_spisok { get; set; } = new ObservableCollection<TrainingType>();

    private string _photoPath;

    public AddBreeder()
    {
        InitializeComponent();
        LoadTraining_types();
        DataContext = this;
    }

    private async void AddPhoto_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Выберите фото лошади",
            Filters = { new FileDialogFilter { Name = "Images", Extensions = { "jpg", "jpeg", "png" } } },
            AllowMultiple = false
        };

        var result = await dialog.ShowAsync(this);
        if (result != null && result.Length > 0)
        {
            _photoPath = result[0];
            breedImage.Source = new Bitmap(_photoPath);
        }
    }


    private void LoadTraining_types()
    {
        using (var context = new DiplomHorseClubContext())
        {
            var types = context.TrainingTypes.ToList();

            Trainingtypes_spisok = new ObservableCollection<TrainingType>(types);
        }
    }

    //тут добавляем в коллекцию тренировок берейтора
    private void AddTrainingtypeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is TrainingType type)
        {
            selected_Types.Add(type);
            Trainingtypes_spisok.Remove(type);
        }
    }

    //тут удаляем из списка берейтора и возвращаем в коллекцию всех типов тренировок
    private void RemoveTrainingtypeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is TrainingType type)
        {
            Trainingtypes_spisok.Add(type);
            selected_Types.Remove(type);
        }
    }

    private async void AddBreeder_ButtonClick(object sender, RoutedEventArgs e)
    {
        using (var context = new DiplomHorseClubContext())
        {
            if (!string.IsNullOrEmpty(loginTextBox.Text) && !string.IsNullOrEmpty(emailTextBox.Text) &&
                !string.IsNullOrEmpty(nameTextBox.Text) && !string.IsNullOrEmpty(surnameTextBox.Text) &&
                !string.IsNullOrEmpty(passwordTextBox.Text) && !string.IsNullOrEmpty(repeatPasswordTextBox.Text))
            {
                //фотка пока будет по умолчанию
                var existsEmail = CheckEmailExists(); //если email существует нужно вводить другой

                if (existsEmail)
                {
                    var messageBox = new CustomMessageBox("Такой Email уже существует", false);
                    await messageBox.ShowDialog(this);
                    return;
                }

                if (repeatPasswordTextBox.Text != passwordTextBox.Text)
                {
                    var messageBox = new CustomMessageBox("Пароли не совпадают", false);
                    await messageBox.ShowDialog(this);
                    return;
                }

                try
                {
                    // Сохранение фото
                    string photoName;
                    if (!string.IsNullOrEmpty(_photoPath))
                    {
                        // Генерируем уникальное имя для файла
                        photoName = $"{Guid.NewGuid()}{Path.GetExtension(_photoPath)}";
                        var imagesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images");

                        // Создаем папку если ее нет
                        if (!Directory.Exists(imagesPath))
                            Directory.CreateDirectory(imagesPath);

                        // Копируем файл
                        File.Copy(_photoPath, Path.Combine(imagesPath, photoName), true);
                    }
                    else
                    {
                        // Используем фото по умолчанию
                        photoName = "tatu.png";
                    }

                    var newBreeder = new User
                    {
                        Id = context.Users.Any() ? context.Users.Max(a => a.Id) + 1 : 1,
                        Roleid = 2,
                        Login = loginTextBox.Text,
                        Email = emailTextBox.Text,
                        Name = nameTextBox.Text,
                        Surname = surnameTextBox.Text,
                        Biography = biographyTextBox.Text ?? "Данные о биографии отсутствуют",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordTextBox.Text),
                        UserPhoto = photoName,
                        Zasluga1 = zasluga1.Text ?? "Данных нет",
                        Zasluga2 = zasluga2.Text ?? "Данных нет",
                        Zasluga3 = zasluga3.Text ?? "Данных нет",
                        Zasluga4 = zasluga4.Text ?? "Данных нет"
                    };

                    context.Users.Add(newBreeder);

                    var breedWallet = new Wallet
                    {
                        Id = context.Wallets.Any() ? context.Wallets.Max(a => a.Id) + 1 : 1,
                        UserId = newBreeder.Id,
                        Summ = 0
                    };

                    context.Wallets.Add(breedWallet);

                    // Передаем context
                    AddBreederTrainings(context, newBreeder.Id);

                    context.SaveChanges();

                    var message = new CustomMessageBox("Берейтор успешно добавлен!", false);
                    await message.ShowDialog(this);

                    this.Close();
                }
                catch (Exception ex)
                {
                    var messageBox = new CustomMessageBox($"Ошибка: {ex.Message}", false);
                    await messageBox.ShowDialog(this);
                }
            }
            else
            {
                var messageBox = new CustomMessageBox("Не все поля заполнены!", false);
                await messageBox.ShowDialog(this);
            }
        } 
    }
    private void AddBreederTrainings(DiplomHorseClubContext context, int breederId)
    {
        foreach (var training in selected_Types)
        {
            var train = new BreederTrainingType
            {
                Id = context.BreederTrainingTypes.Any() ? context.BreederTrainingTypes.Max(a => a.Id) + 1 : 1,
                Breederid = breederId,
                Trainingtypeid = training.Id,
                Costoverride = training.Basecost
            };

            context.BreederTrainingTypes.Add(train);
            context.SaveChanges();
        }
        // context.SaveChanges(); // НЕ вызываем тут чтобы не вызвать ошибки отслеживания сущностей
    }

    //проверяем существует ли уже в БД вводимый email
    private bool CheckEmailExists()
    {
        using (var context = new DiplomHorseClubContext())
        {
            var exists = context.Users.FirstOrDefault(a => a.Email == emailTextBox.Text);
            if (exists != null)
            {
                return true;
            }

            return false;
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}