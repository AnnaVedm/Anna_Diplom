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
                errorTextBlock.Text = "Введите корректное число!";
                return;
            }

            if (amount <= 0)
            {
                errorTextBlock.Text = "Сумма должна быть больше 0!";
                return;
            }

            if (amount > 500000)
            {
                errorTextBlock.Text = "Максимальная сумма пополнения - 500 000 руб!";
                return;
            }

            errorTextBlock.Text = "";
        }

        private async void TopUpButton_Click(object sender, RoutedEventArgs e)
        {
            var amountTextBox = this.FindControl<TextBox>("AmountTextBox");
            var errorTextBlock = this.FindControl<TextBlock>("ErrorTextBlock");

            // Проверка на пустое значение
            if (string.IsNullOrWhiteSpace(amountTextBox.Text))
            {
                errorTextBlock.Text = "Введите сумму пополнения!";
                return;
            }

            // Проверка на число
            if (!decimal.TryParse(amountTextBox.Text, out decimal amount))
            {
                errorTextBlock.Text = "Введите корректное число!";
                return;
            }

            // Проверка на отрицательное значение
            if (amount <= 0)
            {
                errorTextBlock.Text = "Сумма должна быть больше 0!";
                return;
            }

            // Проверка на максимальную сумму
            if (amount > 500000)
            {
                errorTextBlock.Text = "Максимальная сумма пополнения - 500 000 руб!";
                return;
            }

            try
            {
                // Обновление баланса
                var wallet = _currentUser.Wallets.FirstOrDefault();
                if (wallet == null)
                {
                    wallet = new Wallet { UserId = _currentUser.Id, Summ = 0 };
                    _context.Wallets.Add(wallet);
                }

                wallet.Summ += amount;
                await _context.SaveChangesAsync();

                OwnerMenu.LoadUserData(); //перезагрузили интерфейс с балансом
                // Закрытие окна с результатом true
                this.Close(true);
            }
            catch (Exception ex)
            { 
                errorTextBlock.Text = "Ошибка при пополнении баланса: " + ex.Message;
            }
        }
    }
}