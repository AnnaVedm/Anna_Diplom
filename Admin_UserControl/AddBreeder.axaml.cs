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
    public ObservableCollection<TrainingType> selected_Types { get; set; } = new ObservableCollection<TrainingType>(); //��������� ��� ��������� ���������� ���������
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
            Title = "�������� ���� ������",
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

    //��� ��������� � ��������� ���������� ���������
    private void AddTrainingtypeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is TrainingType type)
        {
            selected_Types.Add(type);
            Trainingtypes_spisok.Remove(type);
        }
    }

    //��� ������� �� ������ ��������� � ���������� � ��������� ���� ����� ����������
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
                //����� ���� ����� �� ���������
                var existsEmail = CheckEmailExists(); //���� email ���������� ����� ������� ������

                if (existsEmail)
                {
                    var messageBox = new CustomMessageBox("����� Email ��� ����������", false);
                    await messageBox.ShowDialog(this);
                    return;
                }

                if (repeatPasswordTextBox.Text != passwordTextBox.Text)
                {
                    var messageBox = new CustomMessageBox("������ �� ���������", false);
                    await messageBox.ShowDialog(this);
                    return;
                }

                try
                {
                    // ���������� ����
                    string photoName;
                    if (!string.IsNullOrEmpty(_photoPath))
                    {
                        // ���������� ���������� ��� ��� �����
                        photoName = $"{Guid.NewGuid()}{Path.GetExtension(_photoPath)}";
                        var imagesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images");

                        // ������� ����� ���� �� ���
                        if (!Directory.Exists(imagesPath))
                            Directory.CreateDirectory(imagesPath);

                        // �������� ����
                        File.Copy(_photoPath, Path.Combine(imagesPath, photoName), true);
                    }
                    else
                    {
                        // ���������� ���� �� ���������
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
                        Biography = biographyTextBox.Text ?? "������ � ��������� �����������",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordTextBox.Text),
                        UserPhoto = photoName,
                        Zasluga1 = zasluga1.Text ?? "������ ���",
                        Zasluga2 = zasluga2.Text ?? "������ ���",
                        Zasluga3 = zasluga3.Text ?? "������ ���",
                        Zasluga4 = zasluga4.Text ?? "������ ���"
                    };

                    context.Users.Add(newBreeder);

                    var breedWallet = new Wallet
                    {
                        Id = context.Wallets.Any() ? context.Wallets.Max(a => a.Id) + 1 : 1,
                        UserId = newBreeder.Id,
                        Summ = 0
                    };

                    context.Wallets.Add(breedWallet);

                    // �������� context
                    AddBreederTrainings(context, newBreeder.Id);

                    context.SaveChanges();

                    var message = new CustomMessageBox("�������� ������� ��������!", false);
                    await message.ShowDialog(this);

                    this.Close();
                }
                catch (Exception ex)
                {
                    var messageBox = new CustomMessageBox($"������: {ex.Message}", false);
                    await messageBox.ShowDialog(this);
                }
            }
            else
            {
                var messageBox = new CustomMessageBox("�� ��� ���� ���������!", false);
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
        // context.SaveChanges(); // �� �������� ��� ����� �� ������� ������ ������������ ���������
    }

    //��������� ���������� �� ��� � �� �������� email
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