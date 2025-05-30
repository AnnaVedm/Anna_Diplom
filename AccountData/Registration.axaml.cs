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
        // Получение данных из полей
        var name = NameTextBox.Text;
        var surname = SurnameTextBox.Text;
        var login = LoginTextBox.Text;
        var email = EmailTextBox.Text;
        var password = PasswordTextBox.Text;
        var doublePassword = DoublePasswordTextBox.Text;

        // Валидация данных
        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(surname) ||
            string.IsNullOrWhiteSpace(login) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            CustomMessageBox messageBox = new CustomMessageBox("Все поля должны быть заполнены!");
            await messageBox.ShowDialog(this);
            return;
        }

        if (password != doublePassword)
        {
            CustomMessageBox messageBox = new CustomMessageBox("Пароли не совпадают!");
            await messageBox.ShowDialog(this);
            return;
        }

        if (!IsValidPassword(password))
        {
            CustomMessageBox messageBox = new CustomMessageBox("Пароль должен содержать минимум 8 символов, включая буквы и цифры!");
            await messageBox.ShowDialog(this);
            return;
        }

        if (!IsValidEmail(email))
        {
            CustomMessageBox messageBox = new CustomMessageBox("Некорректный формат email!");
            await messageBox.ShowDialog(this);
            return;
        }

        // Проверка уникальности логина и email
        using (var context = new DiplomHorseClubContext())
        {
            if (context.Users.Any(u => u.Login == login))
            {
                CustomMessageBox message = new CustomMessageBox("Пользователь с таким логином уже существует!");
                await message.ShowDialog(this);
                return;
            }

            if (context.Users.Any(u => u.Email == email))
            {
                CustomMessageBox message = new CustomMessageBox("Пользователь с таким email уже существует!");
                await message.ShowDialog(this);
                return;
            }

            // Создание нового пользователя
            var newUser = new User
            {
                Id = context.Users.Max(u => u.Id) + 1,
                Name = name,
                Surname = surname,
                Login = login,
                Email = email,
                UserPhoto = "zaglushka.png",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12),
                Roleid = 3 // ID роли "Клиент"
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


            CustomMessageBox messageBox = new CustomMessageBox("Регистрация прошла успешно!");
            await messageBox.ShowDialog(this);
            this.Close();
        }
    }

    private bool IsValidPassword(string password)// Проверка: минимум 8 символов, хотя бы одна буква и одна цифра
    {
        var regex = new Regex(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$");
        return regex.IsMatch(password);
    }

    private bool IsValidEmail(string email) //Проверка правильности формы введения email
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