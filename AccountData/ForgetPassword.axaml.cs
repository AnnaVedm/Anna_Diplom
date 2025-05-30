using Avalonia.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using TyutyunnikovaAnna_Diplom.Models;
using BCrypt.Net;
using TyutyunnikovaAnna_Diplom.Context;
using TyutyunnikovaAnna_Diplom.AccountData;
using MailKit.Net.Smtp;
using MimeKit;

namespace TyutyunnikovaAnna_Diplom
{
    public partial class ForgetPassword : Window
    {
        public ForgetPassword()
        {
            InitializeComponent();
        }

        private void BackButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Close();
        }

        private async void SendPasswordButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var email = EmailTextBox.Text;

            using (var context = new DiplomHorseClubContext())
            {
                var user = context.Users.FirstOrDefault(u => u.Email == email);
                if (user == null)
                {
                    CustomMessageBox message = new CustomMessageBox("������������ � ����� email �� ����������!");
                    await message.ShowDialog(this);
                }
                else
                {
                    // �������� ������ ������
                    PasswordSender passwordSender = new PasswordSender(context);
                    await passwordSender.SendNewPasswordAsync(email);

                    CustomMessageBox message = new CustomMessageBox("����� ������ ��������� �� ���� �����!");
                    await message.ShowDialog(this);
                    this.Close();
                }
            }
        }
    }
}
