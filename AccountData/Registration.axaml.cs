using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TyutyunnikovaAnna_Diplom.Context;
using TyutyunnikovaAnna_Diplom.Models;
using System.Linq;

namespace TyutyunnikovaAnna_Diplom;

public partial class Registration : Window
{
    public Registration()
    {
        InitializeComponent();
    }

    private async void RegisterButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // ��������� ������ �� �����
        var name = NameTextBox.Text;
        var surname = SurnameTextBox.Text;
        var login = LoginTextBox.Text;
        var email = EmailTextBox.Text;
        var password = PasswordTextBox.Text;
        var doublePassword = DoublePasswordTextBox.Text;

        // ��������� ������
        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(surname) ||
            string.IsNullOrWhiteSpace(login) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            CustomMessageBox messageBox = new CustomMessageBox("��� ���� ������ ���� ���������!");
            await messageBox.ShowDialog(this);
            return;
        }

        if (password != doublePassword)
        {
            CustomMessageBox messageBox = new CustomMessageBox("������ �� ���������!");
            await messageBox.ShowDialog(this);
            return;
        }

        if (!IsValidPassword(password))
        {
            CustomMessageBox messageBox = new CustomMessageBox("������ ������ ��������� ������� 8 ��������, ������� ����� � �����!");
            await messageBox.ShowDialog(this);
            return;
        }

        if (!IsValidEmail(email))
        {
            CustomMessageBox messageBox = new CustomMessageBox("������������ ������ email!");
            await messageBox.ShowDialog(this);
            return;
        }

        // �������� ������������ ������ � email
        using (var context = new DiplomHorseClubContext())
        {
            if (context.Users.Any(u => u.Login == login))
            {
                CustomMessageBox message = new CustomMessageBox("������������ � ����� ������� ��� ����������!");
                await message.ShowDialog(this);
                return;
            }

            if (context.Users.Any(u => u.Email == email))
            {
                CustomMessageBox message = new CustomMessageBox("������������ � ����� email ��� ����������!");
                await message.ShowDialog(this);
                return;
            }

            // �������� ������ ������������
            var newUser = new User
            {
                Id = context.Users.Max(u => u.Id) + 1,
                Name = name,
                Surname = surname,
                Login = login,
                Email = email,
                UserPhoto = "zaglushka.png",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12),
                Roleid = 3 // ID ���� "������"
            };

            var wallet = new Wallet
            {
                Id = context.Wallets.Max(u => u.Id) + 1,
                UserId = newUser.Id,
                Summ = 0
            };

            context.Wallets.Add(wallet);
            context.Users.Add(newUser);
            await context.SaveChangesAsync();


            CustomMessageBox messageBox = new CustomMessageBox("����������� ������ �������!");
            await messageBox.ShowDialog(this);
            this.Close();
        }
    }

    private bool IsValidPassword(string password)// ��������: ������� 8 ��������, ���� �� ���� ����� � ���� �����
    {
        var regex = new Regex(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$");
        return regex.IsMatch(password);
    }

    private bool IsValidEmail(string email) //�������� ������������ ����� �������� email
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private void ExitButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.Close();
    }
}