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

namespace TyutyunnikovaAnna_Diplom;

// Класс меню клиента, реализует INotifyPropertyChanged для привязки данных
public partial class ClientMenu : UserControl, INotifyPropertyChanged
{
    private User _user; // Текущий пользователь 
    private Bitmap? _currentUserImage; // Аватарка пользователя
    private readonly string _imagesFolderPath; // Путь к папке с изображениями

    public ClientMenu()
    {
        InitializeComponent();
        // Устанавливаем путь к папке с изображениями в директории приложения
        _imagesFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images");
        // Создаем папку, если она не существует
        Directory.CreateDirectory(_imagesFolderPath);
        // Загружаем данные пользователя
        LoadUserData();
        DataContext = this;
    }

    // Свойство пользователя с уведомлением об изменении
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

    // Обработчик нажатия на плитку "Тренерские услуги"
    private void TrainerBorder_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (this.GetVisualRoot() is Menu menuWindow)
        {
            // 1. Скрываем индикатор и сбрасываем кнопки
            menuWindow.HideNavigationPointer();

            // 2. Меняем контент
            menuWindow.Control.Content = new TrainerServices();
        }
    }

    // Обработчик нажатия на плитку "Мои лошади"
    private void MyHorsesBorder_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (this.GetVisualRoot() is Menu menuWindow)
        {
            menuWindow.SetActiveNavigationButton((int)Menu.NavButtonIndex.MyHorses);
        }
    }

    // Обработчик нажатия на плитку "Главная"
    private void HomeBorder_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (this.GetVisualRoot() is Menu menuWindow)
        {
            menuWindow.SetActiveNavigationButton((int)Menu.NavButtonIndex.Home);
        }
    }

    // Обработчик нажатия на плитку "Профиль"
    private void ProfileBorder_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (this.GetVisualRoot() is Menu menuWindow)
        {
            menuWindow.SetActiveNavigationButton((int)Menu.NavButtonIndex.ClientProfile);
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

    private void StableInfoBorder_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (this.GetVisualRoot() is Menu menuWindow)
        {
            // 1. Скрываем индикатор и сбрасываем кнопки
            menuWindow.HideNavigationPointer();

            // 2. Меняем контент
            menuWindow.Control.Content = new StableInfo();
        }
    }

    private void NewsBorder_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (this.GetVisualRoot() is Menu menuWindow)
        {
            // 1. Скрываем индикатор и сбрасываем кнопки
            menuWindow.HideNavigationPointer();

            // 2. Меняем контент
            menuWindow.Control.Content = new ClubNews();
        }
    }
    private void Competition_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (this.GetVisualRoot() is Menu menuWindow)
        {
            // 1. Скрываем индикатор и сбрасываем кнопки
            menuWindow.HideNavigationPointer();

            // 2. Меняем контент
            menuWindow.Control.Content = new Competitions();
        }
    }

}