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

// ����� ���� �������, ��������� INotifyPropertyChanged ��� �������� ������
public partial class ClientMenu : UserControl, INotifyPropertyChanged
{
    private User _user; // ������� ������������ 
    private Bitmap? _currentUserImage; // �������� ������������
    private readonly string _imagesFolderPath; // ���� � ����� � �������������

    public ClientMenu()
    {
        InitializeComponent();
        // ������������� ���� � ����� � ������������� � ���������� ����������
        _imagesFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images");
        // ������� �����, ���� ��� �� ����������
        Directory.CreateDirectory(_imagesFolderPath);
        // ��������� ������ ������������
        LoadUserData();
        DataContext = this;
    }

    // �������� ������������ � ������������ �� ���������
    public User user
    {
        get => _user;
        set
        {
            _user = value;
            // ���������� �� ���������
            OnPropertyChanged(nameof(user));
            // ��������� ����������� ������������
            LoadUserImage(); 
        }
    }

    // �������� ����������� ������������ � ������������ �� ���������
    public Bitmap? CurrentUserImage
    {
        get => _currentUserImage;
        set
        {
            _currentUserImage = value;
            OnPropertyChanged(nameof(CurrentUserImage));
        }
    }

    // ������� ��� ����������� �� ���������� �������
    public event PropertyChangedEventHandler? PropertyChanged;

    // �������� ������ ������������ �� ���� ������
    private void LoadUserData()
    {
        // ���������� �������� ���� ������
        using (var context = new DiplomHorseClubContext())
        {
            //�������� ������������ � ��� ���������
           user = context.Users
               .Include(a => a.Wallets)
               .FirstOrDefault(a => a.Id == UserForAuthorization.SelectedUser.Id);

        }
    }

    // �������� ����������� ������������
    private void LoadUserImage()
    {
        // ���������, ���� �� � ������������ ����
        if (user != null && !string.IsNullOrEmpty(user.UserPhoto))
        {
            // ��������� ������ ���� � �����������
            var imagePath = Path.Combine(_imagesFolderPath, user.UserPhoto);
            // ���� ���� ���������� - ���������, ����� null
            CurrentUserImage = File.Exists(imagePath) ? new Bitmap(imagePath) : null;
        }
    }

    // ���������� ����� ���� ������������
    private async void ChangePhotoButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // �������� TopLevel ��� ������ � ����������� ������
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            // ��������� ������ ������ �����
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "�������� ����� ����",
                    FileTypeFilter = new[] { new FilePickerFileType("�����������")
                        { Patterns = new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp" } } },
                    AllowMultiple = false
                });

            // ���� ������������ ������ ����
            if (files.Count == 1)
            {
                var selectedFile = files[0];
                // ���������� ���������� ��� �����
                var newFileName = $"{Guid.NewGuid()}{Path.GetExtension(selectedFile.Name)}";
                var newFilePath = Path.Combine(_imagesFolderPath, newFileName);

                // �������� ��������� ���� � ���� �����
                using (var stream = await selectedFile.OpenReadAsync())
                using (var fileStream = File.Create(newFilePath))
                {
                    await stream.CopyToAsync(fileStream);
                }

                // ������� ������ ����, ���� ��� ����
                if (!string.IsNullOrEmpty(user.UserPhoto))
                {
                    var oldFilePath = Path.Combine(_imagesFolderPath, user.UserPhoto);
                    if (File.Exists(oldFilePath)) File.Delete(oldFilePath);
                }

                // ��������� ���������� � ���� ������
                using (var context = new DiplomHorseClubContext())
                {
                    var dbUser = context.Users.FirstOrDefault(u => u.Id == user.Id);
                    if (dbUser != null)
                    {
                        dbUser.UserPhoto = newFileName;
                        context.SaveChanges();
                        user.UserPhoto = newFileName;
                        // ��������� ����� �����������
                        LoadUserImage();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // �������� ������
            Console.WriteLine($"������ ��� ����� ����: {ex.Message}");
        }
    }

    // ���������� ������� �� ������ "���������� ������"
    private void TrainerBorder_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (this.GetVisualRoot() is Menu menuWindow)
        {
            // 1. �������� ��������� � ���������� ������
            menuWindow.HideNavigationPointer();

            // 2. ������ �������
            menuWindow.Control.Content = new TrainerServices();
        }
    }

    // ���������� ������� �� ������ "��� ������"
    private void MyHorsesBorder_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (this.GetVisualRoot() is Menu menuWindow)
        {
            menuWindow.SetActiveNavigationButton((int)Menu.NavButtonIndex.MyHorses);
        }
    }

    // ���������� ������� �� ������ "�������"
    private void HomeBorder_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (this.GetVisualRoot() is Menu menuWindow)
        {
            menuWindow.SetActiveNavigationButton((int)Menu.NavButtonIndex.Home);
        }
    }

    // ���������� ������� �� ������ "�������"
    private void ProfileBorder_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (this.GetVisualRoot() is Menu menuWindow)
        {
            menuWindow.SetActiveNavigationButton((int)Menu.NavButtonIndex.ClientProfile);
        }
    }

    // ����� ��� ����������� �� ��������� �������
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // ������������� ����������� XAML
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void StableInfoBorder_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (this.GetVisualRoot() is Menu menuWindow)
        {
            // 1. �������� ��������� � ���������� ������
            menuWindow.HideNavigationPointer();

            // 2. ������ �������
            menuWindow.Control.Content = new StableInfo();
        }
    }

    private void NewsBorder_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (this.GetVisualRoot() is Menu menuWindow)
        {
            // 1. �������� ��������� � ���������� ������
            menuWindow.HideNavigationPointer();

            // 2. ������ �������
            menuWindow.Control.Content = new ClubNews();
        }
    }
    private void Competition_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (this.GetVisualRoot() is Menu menuWindow)
        {
            // 1. �������� ��������� � ���������� ������
            menuWindow.HideNavigationPointer();

            // 2. ������ �������
            menuWindow.Control.Content = new Competitions();
        }
    }

}