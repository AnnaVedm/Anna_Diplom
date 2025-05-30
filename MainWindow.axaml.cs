using Avalonia.Controls;
using Avalonia.Input;
using System.Globalization;
using BCrypt;
using TyutyunnikovaAnna_Diplom.Models;
using TyutyunnikovaAnna_Diplom.Context;
using System.Linq;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using TyutyunnikovaAnna_Diplom.Repositories;
using System;
using TyutyunnikovaAnna_Diplom.AccountData;

namespace TyutyunnikovaAnna_Diplom
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //GenerateHashes();
        }

        //private void GenerateHashes()
        //{
        //    string hash = "password1234";
        //    BCrypt.Net.BCrypt.HashPassword(hash);
        //}

        //============================Авторизация===================================
        private bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        private async void LoginButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) //Авторизация, добавить распределние по ролям пользователей
        {
            using (var context = new DiplomHorseClubContext())
            {
                // Сначала проверяем заполненность полей
                if (string.IsNullOrWhiteSpace(EmailTextBox.Text) || string.IsNullOrWhiteSpace(PasswordTextBox.Text))
                {
                    CustomMessageBox messageBox = new CustomMessageBox("Ошибка! Вы не можете войти в систему. Не заполнены поля.");
                    await messageBox.ShowDialog(this);
                    return; // Важно: прекращаем выполнение если поля пустые
                }

                string user_email = EmailTextBox.Text;
                string password = PasswordTextBox.Text;
                string login = EmailTextBox.Text;

                var user = HorseClub_Repository.LoadUser(user_email, login);

                if (user == null)
                {
                    CustomMessageBox messageBox = new CustomMessageBox("Ошибка! Пользователь не найден.");
                    await messageBox.ShowDialog(this);
                    return;
                }

                // Проверяем пароль
                if (!VerifyPassword(password, user.PasswordHash))
                {
                    CustomMessageBox messageBox = new CustomMessageBox("Ошибка! Неверный пароль.");
                    await messageBox.ShowDialog(this);
                    return;
                }

                else if (user != null && VerifyPassword(password, user.PasswordHash))
                {
                    CustomMessageBox messageBox = new CustomMessageBox("Вы успешно авторизованы!");
                    await messageBox.ShowDialog(this);

                    UserForAuthorization.SelectedUser = user;

                    if (user.Roleid == 3)
                    {
                        Menu menu = new Menu();
                        menu.Show();
                        this.Close();
                    }
                    if (user.Roleid == 2)
                    {
                        TrainerMenu menu1 = new TrainerMenu();
                        menu1.Show();
                        this.Close();
                    }
                    if (user.Roleid == 1)
                    {
                        AdminMenu menu = new AdminMenu();
                        menu.Show();
                        this.Close();
                    }
                }
            }
        }

        private void ExitButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Close();
        }

        //==================================================================================================================================

        private async void GoogleButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                var authenticator = new Services.GoogleAuthenticator();
                var userInfo = await authenticator.AuthenticateAsync();

                using (var context = new DiplomHorseClubContext())
                {
                    var existingUser = context.Users.FirstOrDefault(u => u.Email == userInfo.Email);

                    if (existingUser == null)
                    {
                        var newUser = new User
                        {
                            Id = context.Users.Max(a => a.Id) + 1,
                            Email = userInfo.Email,
                            Name = userInfo.GivenName,
                            Surname = userInfo.FamilyName,
                            PasswordHash = "google_auth",
                            Roleid = 3,
                            UserPhoto = "zaglushka.png",
                            GoogleId = userInfo.Id
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

                        userLoginExists(newUser); //проверка логина пользователя

                        await new CustomMessageBox("Вы успешно зарегистрированы через Google!").ShowDialog(this);
                    }
                    else
                    {
                        await new CustomMessageBox("Добро пожаловать обратно, " + existingUser.Name + "!").ShowDialog(this);

                        UserForAuthorization.SelectedUser = existingUser;

                        Menu menu = new Menu();  //Добавить switch с переходами к разным окнам(берейтор, администратор).
                        menu.Show();

                        userLoginExists(existingUser);
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                await new CustomMessageBox("Ошибка Google авторизации: " + ex.Message).ShowDialog(this);
            }
        }

        private async void userLoginExists(User user)
        {
            if (user.Login == null)
            {
                AddLoginForGoogle addlog = new AddLoginForGoogle(user);
                var result = await addlog.ShowDialog<bool>(this);
                if (result)
                {
                    this.Close();
                }
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e) //Для красивого отображения надписи Google
        {
            this.BeginMoveDrag(e);
        }

        private async void RegistrationButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Registration reg = new Registration();
            await reg.ShowDialog(this);
        }

        private async void ForgetPasswordButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ForgetPassword forget = new ForgetPassword();
            await forget.ShowDialog(this);
        }
    }
}