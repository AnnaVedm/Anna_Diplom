using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TyutyunnikovaAnna_Diplom.AccountData;
using TyutyunnikovaAnna_Diplom.Context;
using TyutyunnikovaAnna_Diplom.Models;
using BCrypt.Net;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System.Text.RegularExpressions;
using Avalonia.VisualTree;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Org.BouncyCastle.Utilities;
namespace TyutyunnikovaAnna_Diplom;

public partial class ClientProfile : UserControl, INotifyPropertyChanged
{
    private User _user; // Текущий пользователь 
    private Bitmap? _currentUserImage; // Аватарка пользователя
    private readonly string _imagesFolderPath; // Путь к папке с изображениями

    private string _currentPassword = "";
    public string CurrentPassword
    {
        get => _currentPassword;
        set
        {
            _currentPassword = value;
            OnPropertyChanged(nameof(CurrentPassword));
        }
    }

    private string _newPassword = "";
    public string NewPassword
    {
        get => _newPassword;
        set
        {
            _newPassword = value;
            OnPropertyChanged(nameof(NewPassword));
        }
    }

    private string _confirmPassword = "";
    public string ConfirmPassword
    {
        get => _confirmPassword;
        set
        {
            _confirmPassword = value;
            OnPropertyChanged(nameof(ConfirmPassword));
        }
    }

    public bool IsZayavkiExists { get; set; }

    //список для отображения истории заявок
    public ObservableCollection<BreederTraining> ZayavkiHistory_spisok { get; set; } = new ObservableCollection<BreederTraining>();

    //список заявок доступен только для клиентов
    public bool IsCurrentUserClient => UserForAuthorization.SelectedUser.Roleid == 3;

    public bool IsBreeder => UserForAuthorization.SelectedUser.Roleid == 2;

    public bool IsEditEnabled { get; set; } = false;

    public ClientProfile()
    {
        InitializeComponent();

        // Устанавливаем путь к папке с изображениями в директории приложения
        _imagesFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images");
        // Создаем папку, если она не существует
        Directory.CreateDirectory(_imagesFolderPath);
        // Загружаем данные пользователя
        LoadUserData();

        Load_ZayavkiHistory();

        updateZayavkiVisibility();
        DataContext = this;
    }
    public User user
    {
        get => _user;
        set
        {
            _user = value;
            // Уведомляем об изменении
            OnPropertyChanged(nameof(user));
            // Загружаем изображение пользователя
            LoadUserImage();
        }
    }

    public void updateZayavkiVisibility()
    {
        IsZayavkiExists = ZayavkiHistory_spisok.Any();
        OnPropertyChanged(nameof(IsZayavkiExists));
    }

    //загрузка истории заявок
    private void Load_ZayavkiHistory(string? filterStatus = null)
    {
        var sortedZayavki = LoadZayavki(filterStatus);

        ZayavkiHistory_spisok.Clear();
        foreach (var z in sortedZayavki)
        {
            ZayavkiHistory_spisok.Add(z);
        }
    }

    private void EnableEditCostButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is BreederTrainingType training)
        {
            training.IsEditTrainingEnabled = true;
        }
    }

    private async void ApplyEditCostButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is BreederTrainingType training)
        {
            using (var context = new DiplomHorseClubContext())
            {
                // Получаем сущность из базы по ключу (Id)
                var trainingUpdate = context.BreederTrainingTypes.Find(training.Id);

                if (trainingUpdate != null)
                {
                    // Проверяем, что Costoverride - число
                    if (decimal.TryParse(training.Costoverride.ToString(), out decimal cost))
                    {
                        trainingUpdate.Costoverride = cost;

                        //training.Costoverride = trainingUpdate.Costoverride; // Обновляем стоимость из объекта, который связан с UI

                        context.BreederTrainingTypes.Update(trainingUpdate); // Сохраняем изменения
                        //context.BreederTrainingTypes.Update(training); // Сохраняем изменения
                        context.SaveChanges();

                        //перезагружаем данные
                        LoadUserData();

                        var message = new CustomMessageBox($"Стоимость тренировки {training.Trainingtype.Name} успешно изменена!", false);;
                        await message.ShowDialog(this.GetVisualRoot() as TrainerMenu);

                    }
                    else
                    {
                        // Если не удалось преобразовать, сообщаем об ошибке
                        var message = new CustomMessageBox("Неверный формат стоимости. Введите число.", false);
                        await message.ShowDialog(this.GetVisualRoot() as TrainerMenu);
                    }
                }
            }

            // Закрываем режим редактирования
            training.IsEditTrainingEnabled = false;
        }
    }


    private List<BreederTraining> LoadZayavki(string? filterStatus = null)
    {
        using (var context = new DiplomHorseClubContext())
        {
            var zayavki = context.BreederTrainings
                .Include(b => b.Breedertrainingtype)
                    .ThenInclude(t => t.Trainingtype)
                .Include(u => u.User)
                .Include(h => h.Horse)
                .ToList();

            // Обновляем статусы
            foreach (var z in zayavki)
            {
                z.UpdateRealStatus();
            }

            // Приоритеты для сортировки
            var statusPriority = new Dictionary<string, int>
            {
                ["В процессе"] = 1,
                ["Принята"] = 2,
                ["Не принята"] = 3,
                ["Выполнена"] = 4,
                ["Отменена"] = 5
            };

            // Фильтруем, если указан фильтр и он не "Все заявки"
            if (!string.IsNullOrEmpty(filterStatus) && filterStatus != "Все заявки")
            {
                zayavki = zayavki.Where(z => z.RealTrainingStatus == filterStatus).ToList();
            }

            // Сортируем по приоритету
            var sortedZayavki = zayavki
                .OrderBy(z => statusPriority.TryGetValue(z.RealTrainingStatus, out var priority) ? priority : 6)
                .ToList();

            return sortedZayavki;
        }
    }


    private void FilterStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox combo && combo.SelectedItem is ComboBoxItem statusItem)
        {
            var selectedStatus = statusItem.Content?.ToString();

            // Загружаем с фильтром или без
            Load_ZayavkiHistory(selectedStatus);
        }
    }

    // Загрузка изображения пользователя
    private void LoadUserImage()
    {
        // Проверяем, есть ли у пользователя фото
        if (user != null && !string.IsNullOrEmpty(user.UserPhoto))
        {
            // Формируем полный путь к изображению
            var imagePath = Path.Combine(_imagesFolderPath, user.UserPhoto);
            // Если файл существует - загружаем, иначе null
            CurrentUserImage = File.Exists(imagePath) ? new Bitmap(imagePath) : null;
        }
    }

    // Свойство изображения пользователя с уведомлением об изменении
    public Bitmap? CurrentUserImage
    {
        get => _currentUserImage;
        set
        {
            _currentUserImage = value;
            OnPropertyChanged(nameof(CurrentUserImage));
        }
    }

    // Событие для уведомления об изменениях свойств
    public event PropertyChangedEventHandler? PropertyChanged;
    // Метод для уведомления об изменении свойств
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Инициализация компонентов XAML
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private bool _isGoogleAuthUser;
    public bool IsGoogleAuthUser
    {
        get => _isGoogleAuthUser;
        set
        {
            _isGoogleAuthUser = value;
            OnPropertyChanged(nameof(IsGoogleAuthUser));
        }
    }

    private void LoadUserData()
    {
        using (var context = new DiplomHorseClubContext())
        {
            user = context.Users
                .Include(a => a.Wallets)
                .Include(b => b.BreederTrainingTypes)
                    .ThenInclude(t => t.Trainingtype)
                .FirstOrDefault(a => a.Id == UserForAuthorization.SelectedUser.Id);

            // Проверяем, является ли пользователь Google-аутентифицированным
            IsGoogleAuthUser = !string.IsNullOrEmpty(user?.GoogleId);
        }
    }

    private void EditBreederDataButton_Click(object sender, RoutedEventArgs e)
    {
        IsEditEnabled = true;
        OnPropertyChanged(nameof(IsEditEnabled));
    }



    //ДОБАВИТЬ ИЗМЕНЕНИЕ COSTOVERRIDE НА УСЛУГИ ТРЕНЕРА

    private async void ApplyBreederDataButton_Click(object sender, RoutedEventArgs e)
    {
        using (var context = new DiplomHorseClubContext())
        {
            var currentUser = context.Users.FirstOrDefault(a => a.Id == user.Id); // user из DataContext

            if (currentUser != null)
            {
                // Обновляем поля из объекта user, который уже содержит изменения из UI
                currentUser.Biography = string.IsNullOrWhiteSpace(user.Biography) ? "Данные о биографии отсутствуют" : user.Biography;
                currentUser.Zasluga1 = string.IsNullOrWhiteSpace(user.Zasluga1) ? "Данных нет" : user.Zasluga1;
                currentUser.Zasluga2 = string.IsNullOrWhiteSpace(user.Zasluga2) ? "Данных нет" : user.Zasluga2;
                currentUser.Zasluga3 = string.IsNullOrWhiteSpace(user.Zasluga3) ? "Данных нет" : user.Zasluga3;
                currentUser.Zasluga4 = string.IsNullOrWhiteSpace(user.Zasluga4) ? "Данных нет" : user.Zasluga4;

                context.Users.Update(currentUser);
                context.SaveChanges();

                // Обновляем локальный объект user из базы, если нужно
                LoadUserData();

                var message = new CustomMessageBox("Данные успешно обновлены!", false);
                await message.ShowDialog(this.GetVisualRoot() as TrainerMenu);

                IsEditEnabled = false;
                OnPropertyChanged(nameof(IsEditEnabled));
            }
        }
    }

    private async void CancelZayavkaButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is BreederTraining zayavka)
        {
            var confirm = new CustomMessageBox("Вы уверены, что хотите отменить заявку?", true);
            var result = await confirm.ShowDialog<bool>(this.GetVisualRoot() as Menu);

            if (result == true)
            {
                //отменяем заявку
                using (var context = new DiplomHorseClubContext())
                {
                    //возвращаем пользователю списанную за тренировку сумму
                    var userWallet = context.Wallets
                        .FirstOrDefault(a => a.UserId == UserForAuthorization.SelectedUser.Id);

                    if (userWallet != null)
                    {
                        userWallet.Summ += (decimal)zayavka.Cost;
                        context.Wallets.Update(userWallet);
                    }

                    //далее нужно обновить вывод суммы кошелька в Menu:
                    var ownerMenu = this.GetVisualRoot() as Menu;
                    if (ownerMenu != null)
                    {
                        ownerMenu.LoadUserData();
                    }

                    context.BreederTrainings.Remove(zayavka);
                    context.SaveChanges();

                    //обновляем данные этого окна
                    Load_ZayavkiHistory();
                    updateZayavkiVisibility();

                    var message = new CustomMessageBox("Заявка успешно отменена!", false);
                    message.ShowDialog(this.GetVisualRoot() as Menu);
                }
            }
        }
    }

    // Обработчик смены фото пользователя
    private async void ChangePhotoButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Получаем TopLevel для работы с диалоговыми окнами
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            // Открываем диалог выбора файла
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "Выберите новое фото",
                    FileTypeFilter = new[] { new FilePickerFileType("Изображения")
                        { Patterns = new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp" } } },
                    AllowMultiple = false
                });

            // Если пользователь выбрал файл
            if (files.Count == 1)
            {
                var selectedFile = files[0];
                // Генерируем уникальное имя файла
                var newFileName = $"{Guid.NewGuid()}{Path.GetExtension(selectedFile.Name)}";
                var newFilePath = Path.Combine(_imagesFolderPath, newFileName);

                // Копируем выбранный файл в нашу папку
                using (var stream = await selectedFile.OpenReadAsync())
                using (var fileStream = File.Create(newFilePath))
                {
                    await stream.CopyToAsync(fileStream);
                }

                // Удаляем старое фото, если оно было
                if (!string.IsNullOrEmpty(user.UserPhoto))
                {
                    var oldFilePath = Path.Combine(_imagesFolderPath, user.UserPhoto);
                    if (File.Exists(oldFilePath)) File.Delete(oldFilePath);
                }

                // Обновляем информацию в базе данных
                using (var context = new DiplomHorseClubContext())
                {
                    var dbUser = context.Users.FirstOrDefault(u => u.Id == user.Id);
                    if (dbUser != null)
                    {
                        dbUser.UserPhoto = newFileName;
                        context.SaveChanges();
                        user.UserPhoto = newFileName;
                        // Загружаем новое изображение
                        LoadUserImage();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Логируем ошибки
            Console.WriteLine($"Ошибка при смене фото: {ex.Message}");
        }
    }

    // Обработчик смены пароля
    private async void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
    {
        var Content = this.GetVisualRoot() as Menu;
        // Валидация полей через привязки
        if (string.IsNullOrWhiteSpace(CurrentPassword))
        {
            CustomMessageBox message = new CustomMessageBox("Введите текущий пароль!");
            await message.ShowDialog(Content);
            return;
        }

        if (string.IsNullOrWhiteSpace(NewPassword))
        {
            CustomMessageBox message = new CustomMessageBox("Введите новый пароль!");
            await message.ShowDialog(Content);
            return;
        }

        if (NewPassword != ConfirmPassword)
        {
            CustomMessageBox message = new CustomMessageBox("Новые пароли не совпадают!");
            await message.ShowDialog(Content);
            return;
        }

        if (!IsValidPassword(NewPassword))
        {
            CustomMessageBox message = new CustomMessageBox("Пароль должен содержать минимум 8 символов, включая буквы и цифры!");
            await message.ShowDialog(Content);
            return;
        }

        try
        {
            using (var context = new DiplomHorseClubContext())
            {
                var dbUser = context.Users.FirstOrDefault(u => u.Id == user.Id);
                if (dbUser == null)
                {
                    CustomMessageBox message1 = new CustomMessageBox("Пользователь не найден!");
                    await message1.ShowDialog(Content);
                    return;
                }

                // Проверка текущего пароля
                if (!BCrypt.Net.BCrypt.Verify(CurrentPassword, dbUser.PasswordHash))
                {
                    CustomMessageBox message2 = new CustomMessageBox("Неверный текущий пароль!");
                    await message2.ShowDialog(Content);
                    return;
                }

                // Обновление пароля
                dbUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(NewPassword, workFactor: 12);
                context.SaveChanges();

                // Очистка полей через свойства
                CurrentPassword = "";
                NewPassword = "";
                ConfirmPassword = "";

                CustomMessageBox message = new CustomMessageBox("Пароль успешно изменен!");
                await message.ShowDialog(Content);
            }
        }
        catch (Exception ex)
        {
            CustomMessageBox message = new CustomMessageBox($"Ошибка при изменении пароля: {ex.Message}");
            await message.ShowDialog(Content);
        }
    }

    private bool IsValidPassword(string password)
    {
        var regex = new Regex(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$");
        return regex.IsMatch(password);
    }

    private async Task ShowMessage(string text, bool isError = true)
    {
        var messageBox = MessageBoxManager.GetMessageBoxStandard(
            "Уведомление",
            text,
            ButtonEnum.Ok,
            isError ? Icon.Error : Icon.Success);

        await messageBox.ShowAsync();
    }
}