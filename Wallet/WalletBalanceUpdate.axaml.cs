using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using TyutyunnikovaAnna_Diplom.AccountData;
using TyutyunnikovaAnna_Diplom.Context;
using TyutyunnikovaAnna_Diplom.Models;

namespace TyutyunnikovaAnna_Diplom
{
    public partial class WalletBalanceUpdate : Window
    {
        private readonly DiplomHorseClubContext _context;
        private readonly User _currentUser;

        public Menu OwnerMenu { get;set; }

        public TrainerMenu OwnerTrainerMenu { get; set; }

        public WalletBalanceUpdate()
        {
            InitializeComponent();
            _context = new DiplomHorseClubContext();
            _currentUser = _context.Users
                .Include(u => u.Wallets)
                .FirstOrDefault(u => u.Id == UserForAuthorization.SelectedUser.Id);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AmountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            var errorTextBlock = this.FindControl<TextBlock>("ErrorTextBlock");

            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                errorTextBlock.Text = "";
                return;
            }

            if (!decimal.TryParse(textBox.Text, out decimal amount))
            {
                errorTextBlock.Text = "������� ���������� �����!";
                return;
            }

            if (amount <= 0)
            {
                errorTextBlock.Text = "����� ������ ���� ������ 0!";
                return;
            }

            if (amount > 500000)
            {
                errorTextBlock.Text = "������������ ����� ���������� - 500 000 ���!";
                return;
            }

            errorTextBlock.Text = "";
        }

        private async void TopUpButton_Click(object sender, RoutedEventArgs e)
        {
            var amountTextBox = this.FindControl<TextBox>("AmountTextBox");
            var errorTextBlock = this.FindControl<TextBlock>("ErrorTextBlock");

            // �������� �� ������ ��������
            if (string.IsNullOrWhiteSpace(amountTextBox.Text))
            {
                errorTextBlock.Text = "������� ����� ����������!";
                return;
            }

            // �������� �� �����
            if (!decimal.TryParse(amountTextBox.Text, out decimal amount))
            {
                errorTextBlock.Text = "������� ���������� �����!";
                return;
            }

            // �������� �� ������������� ��������
            if (amount <= 0)
            {
                errorTextBlock.Text = "����� ������ ���� ������ 0!";
                return;
            }

            // �������� �� ������������ �����
            if (amount > 500000)
            {
                errorTextBlock.Text = "������������ ����� ���������� - 500 000 ���!";
                return;
            }

            try
            {
                // ���������� �������
                var wallet = _currentUser.Wallets.FirstOrDefault();
                if (wallet == null)
                {
                    wallet = new Wallet { UserId = _currentUser.Id, Summ = 0 };
                    _context.Wallets.Add(wallet);
                }

                wallet.Summ += amount;
                await _context.SaveChangesAsync();

                OwnerMenu.LoadUserData(); //������������� ��������� � ��������
                // �������� ���� � ����������� true
                this.Close(true);
            }
            catch (Exception ex)
            { 
                errorTextBlock.Text = "������ ��� ���������� �������: " + ex.Message;
            }
        }
    }
}