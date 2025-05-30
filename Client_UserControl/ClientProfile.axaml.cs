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
    private User _user; // ������� ������������ 
    private Bitmap? _currentUserImage; // �������� ������������
    private readonly string _imagesFolderPath; // ���� � ����� � �������������

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

    //������ ��� ����������� ������� ������
    public ObservableCollection<BreederTraining> ZayavkiHistory_spisok { get; set; } = new ObservableCollection<BreederTraining>();

    //������ ������ �������� ������ ��� ��������
    public bool IsCurrentUserClient => UserForAuthorization.SelectedUser.Roleid == 3;

    public bool IsBreeder => UserForAuthorization.SelectedUser.Roleid == 2;

    public bool IsEditEnabled { get; set; } = false;

    public ClientProfile()
    {
        InitializeComponent();

        // ������������� ���� � ����� � ������������� � ���������� ����������
        _imagesFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images");
        // ������� �����, ���� ��� �� ����������
        Directory.CreateDirectory(_imagesFolderPath);
        // ��������� ������ ������������
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
            // ���������� �� ���������
            OnPropertyChanged(nameof(user));
            // ��������� ����������� ������������
            LoadUserImage();
        }
    }

    public void updateZayavkiVisibility()
    {
        IsZayavkiExists = ZayavkiHistory_spisok.Any();
        OnPropertyChanged(nameof(IsZayavkiExists));
    }

    //�������� ������� ������
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
                // �������� �������� �� ���� �� ����� (Id)
                var trainingUpdate = context.BreederTrainingTypes.Find(training.Id);

                if (trainingUpdate != null)
                {
                    // ���������, ��� Costoverride - �����
                    if (decimal.TryParse(training.Costoverride.ToString(), out decimal cost))
                    {
                        trainingUpdate.Costoverride = cost;

                        //training.Costoverride = trainingUpdate.Costoverride; // ��������� ��������� �� �������, ������� ������ � UI

                        context.BreederTrainingTypes.Update(trainingUpdate); // ��������� ���������
                        //context.BreederTrainingTypes.Update(training); // ��������� ���������
                        context.SaveChanges();

                        //������������� ������
                        LoadUserData();

                        var message = new CustomMessageBox($"��������� ���������� {training.Trainingtype.Name} ������� ��������!", false);;
                        await message.ShowDialog(this.GetVisualRoot() as TrainerMenu);

                    }
                    else
                    {
                        // ���� �� ������� �������������, �������� �� ������
                        var message = new CustomMessageBox("�������� ������ ���������. ������� �����.", false);
                        await message.ShowDialog(this.GetVisualRoot() as TrainerMenu);
                    }
                }
            }

            // ��������� ����� ��������������
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

            // ��������� �������
            foreach (var z in zayavki)
            {
                z.UpdateRealStatus();
            }

            // ���������� ��� ����������
            var statusPriority = new Dictionary<string, int>
            {
                ["� ��������"] = 1,
                ["�������"] = 2,
                ["�� �������"] = 3,
                ["���������"] = 4,
                ["��������"] = 5
            };

            // ���������, ���� ������ ������ � �� �� "��� ������"
            if (!string.IsNullOrEmpty(filterStatus) && filterStatus != "��� ������")
            {
                zayavki = zayavki.Where(z => z.RealTrainingStatus == filterStatus).ToList();
            }

            // ��������� �� ����������
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

            // ��������� � �������� ��� ���
            Load_ZayavkiHistory(selectedStatus);
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

            // ���������, �������� �� ������������ Google-�������������������
            IsGoogleAuthUser = !string.IsNullOrEmpty(user?.GoogleId);
        }
    }

    private void EditBreederDataButton_Click(object sender, RoutedEventArgs e)
    {
        IsEditEnabled = true;
        OnPropertyChanged(nameof(IsEditEnabled));
    }



    //�������� ��������� COSTOVERRIDE �� ������ �������

    private async void ApplyBreederDataButton_Click(object sender, RoutedEventArgs e)
    {
        using (var context = new DiplomHorseClubContext())
        {
            var currentUser = context.Users.FirstOrDefault(a => a.Id == user.Id); // user �� DataContext

            if (currentUser != null)
            {
                // ��������� ���� �� ������� user, ������� ��� �������� ��������� �� UI
                currentUser.Biography = string.IsNullOrWhiteSpace(user.Biography) ? "������ � ��������� �����������" : user.Biography;
                currentUser.Zasluga1 = string.IsNullOrWhiteSpace(user.Zasluga1) ? "������ ���" : user.Zasluga1;
                currentUser.Zasluga2 = string.IsNullOrWhiteSpace(user.Zasluga2) ? "������ ���" : user.Zasluga2;
                currentUser.Zasluga3 = string.IsNullOrWhiteSpace(user.Zasluga3) ? "������ ���" : user.Zasluga3;
                currentUser.Zasluga4 = string.IsNullOrWhiteSpace(user.Zasluga4) ? "������ ���" : user.Zasluga4;

                context.Users.Update(currentUser);
                context.SaveChanges();

                // ��������� ��������� ������ user �� ����, ���� �����
                LoadUserData();

                var message = new CustomMessageBox("������ ������� ���������!", false);
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
            var confirm = new CustomMessageBox("�� �������, ��� ������ �������� ������?", true);
            var result = await confirm.ShowDialog<bool>(this.GetVisualRoot() as Menu);

            if (result == true)
            {
                //�������� ������
                using (var context = new DiplomHorseClubContext())
                {
                    //���������� ������������ ��������� �� ���������� �����
                    var userWallet = context.Wallets
                        .FirstOrDefault(a => a.UserId == UserForAuthorization.SelectedUser.Id);

                    if (userWallet != null)
                    {
                        userWallet.Summ += (decimal)zayavka.Cost;
                        context.Wallets.Update(userWallet);
                    }

                    //����� ����� �������� ����� ����� �������� � Menu:
                    var ownerMenu = this.GetVisualRoot() as Menu;
                    if (ownerMenu != null)
                    {
                        ownerMenu.LoadUserData();
                    }

                    context.BreederTrainings.Remove(zayavka);
                    context.SaveChanges();

                    //��������� ������ ����� ����
                    Load_ZayavkiHistory();
                    updateZayavkiVisibility();

                    var message = new CustomMessageBox("������ ������� ��������!", false);
                    message.ShowDialog(this.GetVisualRoot() as Menu);
                }
            }
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

    // ���������� ����� ������
    private async void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
    {
        var Content = this.GetVisualRoot() as Menu;
        // ��������� ����� ����� ��������
        if (string.IsNullOrWhiteSpace(CurrentPassword))
        {
            CustomMessageBox message = new CustomMessageBox("������� ������� ������!");
            await message.ShowDialog(Content);
            return;
        }

        if (string.IsNullOrWhiteSpace(NewPassword))
        {
            CustomMessageBox message = new CustomMessageBox("������� ����� ������!");
            await message.ShowDialog(Content);
            return;
        }

        if (NewPassword != ConfirmPassword)
        {
            CustomMessageBox message = new CustomMessageBox("����� ������ �� ���������!");
            await message.ShowDialog(Content);
            return;
        }

        if (!IsValidPassword(NewPassword))
        {
            CustomMessageBox message = new CustomMessageBox("������ ������ ��������� ������� 8 ��������, ������� ����� � �����!");
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
                    CustomMessageBox message1 = new CustomMessageBox("������������ �� ������!");
                    await message1.ShowDialog(Content);
                    return;
                }

                // �������� �������� ������
                if (!BCrypt.Net.BCrypt.Verify(CurrentPassword, dbUser.PasswordHash))
                {
                    CustomMessageBox message2 = new CustomMessageBox("�������� ������� ������!");
                    await message2.ShowDialog(Content);
                    return;
                }

                // ���������� ������
                dbUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(NewPassword, workFactor: 12);
                context.SaveChanges();

                // ������� ����� ����� ��������
                CurrentPassword = "";
                NewPassword = "";
                ConfirmPassword = "";

                CustomMessageBox message = new CustomMessageBox("������ ������� �������!");
                await message.ShowDialog(Content);
            }
        }
        catch (Exception ex)
        {
            CustomMessageBox message = new CustomMessageBox($"������ ��� ��������� ������: {ex.Message}");
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
            "�����������",
            text,
            ButtonEnum.Ok,
            isError ? Icon.Error : Icon.Success);

        await messageBox.ShowAsync();
    }
}