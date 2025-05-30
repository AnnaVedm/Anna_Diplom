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
    private User _user; // ������� ������������
    private Horse _horse;
    private Bitmap? _currentUserImage; // �������� ������������
    private Bitmap? _currentHorseImage;
    private readonly string _imagesFolderPath; // ���� � ����� � �������������

    public TrainMenu()
    {
        InitializeComponent();
        // ������������� ���� � ����� � ������������� � ���������� ����������
        _imagesFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images");
        // ������� �����, ���� ��� �� ����������
        Directory.CreateDirectory(_imagesFolderPath);
        // ��������� ������ ������������
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
                .Where(a => a.Status == "�� �������")
                .ToList();

            Zayavki_spisok.Clear();
            foreach (var item in zayavki)
            {
                Zayavki_spisok.Add(item);
            }

            // ��������� ���� ������� ������
            iszayvaexcist = Zayavki_spisok.Any();
            OnPropertyChanged(nameof(iszayvaexcist));
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

    // �������� ������������ � ������������ �� ���������

    public Horse horse
    {
        get => _horse;
        set
        {
            _horse = value;
            // ���������� �� ���������
            OnPropertyChanged(nameof(horse));
            // ��������� ����������� ������������
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


    // �������� ����������� ������������ � ������������ �� ���������
    public Bitmap? CurrentHorseImage
    {
        get => _currentHorseImage;
        set
        {
            _currentHorseImage = value;
            OnPropertyChanged(nameof(CurrentHorseImage));
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

    private void LoadHorseImage()
    {
        // ���������, ���� �� � ������������ ����
        if (user != null && !string.IsNullOrEmpty(horse.HorsePhoto))
        {
            // ��������� ������ ���� � �����������
            var imagePath = Path.Combine(_imagesFolderPath, horse.HorsePhoto);
            // ���� ���� ���������� - ���������, ����� null
            CurrentHorseImage = File.Exists(imagePath) ? new Bitmap(imagePath) : null;
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

    private void NewsBorder_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (this.GetVisualRoot() is TrainerMenu menuWindow)
        {
            // 1. �������� ��������� � ���������� ������
            menuWindow.HideNavigationPointer();

            // 2. ������ �������
            menuWindow.Control.Content = new ClubNews();
        }
    }


    // ���������� ������� �� ������ "�������"
    private void HomeBorder_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (this.GetVisualRoot() is TrainerMenu menuWindow)
        {
            menuWindow.SetActiveNavigationButton((int)TrainerMenu.NavButtonIndex.Home);
        }
    }

    // ���������� ������� �� ������ "�������"
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
            var message = new CustomMessageBox("�� �������, ��� ������ ��������� ������?", true);
            var result = await message.ShowDialog<bool>(this.GetVisualRoot() as TrainerMenu);

            if (result)
            {
                using (var context = new DiplomHorseClubContext())
                {
                    // ������� ������ ���������� � ������
                    var training = context.BreederTrainings
                        .Include(t => t.User)
                            .ThenInclude(u => u.Wallets)
                        .FirstOrDefault(t => t.Id == breed.Id);

                    if (training != null && training.Status == "�� �������")
                    {
                        // ������� ������� �������
                        var clientWallet = training.User.Wallets.FirstOrDefault();

                        if (clientWallet != null)
                        {
                            try
                            {
                                // ���������� ������ �������
                                clientWallet.Summ += training.Cost ?? 0m;

                                // ��������� ������ ������
                                training.Status = "���������";

                                // ��������� ���������
                                context.SaveChanges();

                                // ��������� ���������
                                Load_zayavki(); // ������������� ������ ������

                                // ��������� ������ ������������ � ������������ ����
                                // �������� ������������ ���� TrainerMenu
                                if (this.GetVisualRoot() is TrainerMenu trainerMenu)
                                {
                                    // ������� ����� ��������� TrainMenu
                                    var newTrainMenu = new TrainMenu();
                                    newTrainMenu.trainerMenu = trainerMenu;

                                    // �������� ����������
                                    trainerMenu.Control.Content = newTrainMenu;

                                    // ��������� ������ ������������
                                    trainerMenu.LoadUserData();
                                }
                            }
                            catch (DbUpdateException ex)
                            {
                                // ��������� ������ ���� ������
                                Console.WriteLine($"������ ��� ���������� ������: {ex.InnerException?.Message}");
                            }
                            catch (Exception ex)
                            {
                                // ��������� ������ ������
                                Console.WriteLine($"������: {ex.Message}");
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
                breed.Status = "�������"; //������� � ������� ����������

                var wal = context.Wallets.FirstOrDefault(a => a.UserId == UserForAuthorization.SelectedUser.Id);

                if (wal != null)
                {
                    wal.Summ += (decimal)breed.Cost;
                    context.BreederTrainings.Update(breed);
                    context.Wallets.Update(wal);
                    context.SaveChanges();
                }
            }

            // �������� ������������ ���� TrainerMenu
            if (this.GetVisualRoot() is TrainerMenu trainerMenu)
            {
                // ������� ����� ��������� TrainMenu
                var newTrainMenu = new TrainMenu();
                newTrainMenu.trainerMenu = trainerMenu;

                // �������� ����������
                trainerMenu.Control.Content = newTrainMenu;

                // ��������� ������ ������������
                trainerMenu.LoadUserData();
            }
        }
    }
}