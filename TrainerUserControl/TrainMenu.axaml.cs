using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.VisualTree;
using System.ComponentModel;
using System.IO;
using System;
using TyutyunnikovaAnna_Diplom.Models;
using TyutyunnikovaAnna_Diplom.Context;
using Avalonia.Platform.Storage;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TyutyunnikovaAnna_Diplom.AccountData;
using System.Collections.ObjectModel;

namespace TyutyunnikovaAnna_Diplom;

public partial class TrainMenu : UserControl
{
    public ObservableCollection<BreederTraining> Zayavki_spisok { get; set; } = new ObservableCollection<BreederTraining>();

    public TrainerMenu trainerMenu { get; set; }
    //public bool iszayvaexcist { get; set; }
    private User _user; // Текущий пользователь
    private Horse _horse;
    private Bitmap? _currentUserImage; // Аватарка пользователя
    private Bitmap? _currentHorseImage;
    private readonly string _imagesFolderPath; // Путь к папке с изображениями

    public TrainMenu()
    {
        InitializeComponent();
        // Устанавливаем путь к папке с изображениями в директории приложения
        _imagesFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images");
        // Создаем папку, если она не существует
        Directory.CreateDirectory(_imagesFolderPath);
        // Загружаем данные пользователя
        LoadUserData();
        Load_zayavki();
        DataContext = this;
    }

    private void Load_zayavki()
    {
        using (var context = new DiplomHorseClubContext())
        {
            var zayavki = context.BreederTrainings
                .Include(h => h.Horse)
                    .ThenInclude(h => h.HorseStables)
                        .ThenInclude(s => s.Stable)
                .Include(a => a.User)
                .Include(a => a.Breedertrainingtype)
                    .ThenInclude(t => t.Trainingtype)
                .Where(a => a.Status == "Не принята")
                .ToList();

            Zayavki_spisok.Clear();
            foreach (var item in zayavki)
            {
                Zayavki_spisok.Add(item);
            }

            // Обновляем флаг наличия заявок
            iszayvaexcist = Zayavki_spisok.Any();
            OnPropertyChanged(nameof(iszayvaexcist));
        }
    }


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

    // Свойство пользователя с уведомлением об изменении

    public Horse horse
    {
        get => _horse;
        set
        {
            _horse = value;
            // Уведомляем об изменении
            OnPropertyChanged(nameof(horse));
            // Загружаем изображение пользователя
            LoadHorseImage();
        }
    }

    private bool _iszayvaexcist;
    public bool iszayvaexcist
    {
        get => _iszayvaexcist;
        set
        {
            _iszayvaexcist = value;
            OnPropertyChanged(nameof(iszayvaexcist));
        }
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


    // Свойство изображения пользователя с уведомлением об изменении
    public Bitmap? CurrentHorseImage
    {
        get => _currentHorseImage;
        set
        {
            _currentHorseImage = value;
            OnPropertyChanged(nameof(CurrentHorseImage));
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

    private void LoadHorseImage()
    {
        // Проверяем, есть ли у пользователя фото
        if (user != null && !string.IsNullOrEmpty(horse.HorsePhoto))
        {
            // Формируем полный путь к изображению
            var imagePath = Path.Combine(_imagesFolderPath, horse.HorsePhoto);
            // Если файл существует - загружаем, иначе null
            CurrentHorseImage = File.Exists(imagePath) ? new Bitmap(imagePath) : null;
        }
    }

    // Событие для уведомления об изменениях свойств
    public event PropertyChangedEventHandler? PropertyChanged;

    // Загрузка данных пользователя из базы данных
    private void LoadUserData()
    {
        // Используем контекст базы данных
        using (var context = new DiplomHorseClubContext())
        {
            //Получаем пользователя с его кошельком
            user = context.Users
                .Include(a => a.Wallets)
                .FirstOrDefault(a => a.Id == UserForAuthorization.SelectedUser.Id);

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

    private void NewsBorder_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (this.GetVisualRoot() is TrainerMenu menuWindow)
        {
            // 1. Скрываем индикатор и сбрасываем кнопки
            menuWindow.HideNavigationPointer();

            // 2. Меняем контент
            menuWindow.Control.Content = new ClubNews();
        }
    }


    // Обработчик нажатия на плитку "Главная"
    private void HomeBorder_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (this.GetVisualRoot() is TrainerMenu menuWindow)
        {
            menuWindow.SetActiveNavigationButton((int)TrainerMenu.NavButtonIndex.Home);
        }
    }

    // Обработчик нажатия на плитку "Профиль"
    private void ProfileBorder_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (this.GetVisualRoot() is TrainerMenu menuWindow)
        {
            menuWindow.SetActiveNavigationButton((int)TrainerMenu.NavButtonIndex.ClientProfile);
        }
    }
    private async void RejectButton_Click(object? sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button?.DataContext is BreederTraining breed)
        {
            var message = new CustomMessageBox("Вы уверены, что хотите отклонить заявку?", true);
            var result = await message.ShowDialog<bool>(this.GetVisualRoot() as TrainerMenu);

            if (result)
            {
                using (var context = new DiplomHorseClubContext())
                {
                    // Находим полную информацию о заявке
                    var training = context.BreederTrainings
                        .Include(t => t.User)
                            .ThenInclude(u => u.Wallets)
                        .FirstOrDefault(t => t.Id == breed.Id);

                    if (training != null && training.Status == "Не принята")
                    {
                        // Находим кошелек клиента
                        var clientWallet = training.User.Wallets.FirstOrDefault();

                        if (clientWallet != null)
                        {
                            try
                            {
                                // Возвращаем деньги клиенту
                                clientWallet.Summ += training.Cost ?? 0m;

                                // Обновляем статус заявки
                                training.Status = "Отклонена";

                                // Сохраняем изменения
                                context.SaveChanges();

                                // Обновляем интерфейс
                                Load_zayavki(); // Перезагружаем список заявок

                                // Обновляем данные пользователя в родительском окне
                                // Получаем родительское окно TrainerMenu
                                if (this.GetVisualRoot() is TrainerMenu trainerMenu)
                                {
                                    // Создаем новый экземпляр TrainMenu
                                    var newTrainMenu = new TrainMenu();
                                    newTrainMenu.trainerMenu = trainerMenu;

                                    // Заменяем содержимое
                                    trainerMenu.Control.Content = newTrainMenu;

                                    // Обновляем данные пользователя
                                    trainerMenu.LoadUserData();
                                }
                            }
                            catch (DbUpdateException ex)
                            {
                                // Обработка ошибок базы данных
                                Console.WriteLine($"Ошибка при отклонении заявки: {ex.InnerException?.Message}");
                            }
                            catch (Exception ex)
                            {
                                // Обработка других ошибок
                                Console.WriteLine($"Ошибка: {ex.Message}");
                            }
                        }
                    }
                }
            }
        }
    }

    private void SubmitButton_Click(object? sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button?.DataContext is BreederTraining breed)
        {
            using (var context = new DiplomHorseClubContext())
            {
                breed.Status = "Принята"; //принята и ожидает выполнения

                var wal = context.Wallets.FirstOrDefault(a => a.UserId == UserForAuthorization.SelectedUser.Id);

                if (wal != null)
                {
                    wal.Summ += (decimal)breed.Cost;
                    context.BreederTrainings.Update(breed);
                    context.Wallets.Update(wal);
                    context.SaveChanges();
                }
            }

            // Получаем родительское окно TrainerMenu
            if (this.GetVisualRoot() is TrainerMenu trainerMenu)
            {
                // Создаем новый экземпляр TrainMenu
                var newTrainMenu = new TrainMenu();
                newTrainMenu.trainerMenu = trainerMenu;

                // Заменяем содержимое
                trainerMenu.Control.Content = newTrainMenu;

                // Обновляем данные пользователя
                trainerMenu.LoadUserData();
            }
        }
    }
}