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
    public partial class ToDoubleArendStable : Window
    {
        private readonly Horse _selectedHorse;
        private decimal _rentalPrice;
        public Menu Ownermenu { get; set; }

        public ToDoubleArendStable()
        {
            InitializeComponent();
        }

        public ToDoubleArendStable(Horse selectedHorse)
        {
            InitializeComponent();
            _selectedHorse = selectedHorse;
            LoadRentalPrice();
            DataContext = this;
        }

        private async void LoadRentalPrice()
        {
            try
            {
                using (var context = new DiplomHorseClubContext())
                {
                    // �������� ��� ������� ��� ����������� ����
                    var stableType = await context.HorseStables
                        .Include(hs => hs.Stable)
                            .ThenInclude(s => s.Type)
                        .Where(hs => hs.Horseid == _selectedHorse.Id)
                        .Select(hs => hs.Stable.Type)
                        .FirstOrDefaultAsync();

                    if (stableType != null && stableType.Cost.HasValue)
                    {
                        _rentalPrice = (decimal)stableType.Cost.Value; // ����� �������������� int � decimal
                        PriceTextBlock.Text = $"��������� ���������: {_rentalPrice} ���.";
                    }
                    else
                    {
                        ErrorTextBlock.Text = "�� ������� ���������� ��������� ������ (��������� �� �����������)";
                        ExtendButton.IsEnabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorTextBlock.Text = $"������: {ex.Message}";
                ExtendButton.IsEnabled = false;
            }
        }

        private async void OnExtendButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new DiplomHorseClubContext())
                {
                    // 1. ��������� ������ ������������
                    var user = UserForAuthorization.SelectedUser;
                    var wallet = await context.Wallets.FirstOrDefaultAsync(w => w.UserId == user.Id);

                    if (wallet == null)
                    {
                        ErrorTextBlock.Text = "������� ������������ �� ������";
                        return;
                    }

                    if (wallet.Summ < _rentalPrice)
                    {
                        ErrorTextBlock.Text = "������������ ������� �� �����. ������� ��������� ������!";
                        return;
                    }

                    // 2. ������� ������ � ������� ������
                    var horseStable = await context.HorseStables
                        .FirstOrDefaultAsync(hs => hs.Horseid == _selectedHorse.Id);

                    if (horseStable == null)
                    {
                        ErrorTextBlock.Text = "�� ������� ���������� � �������";
                        return;
                    }

                    if (horseStable.Assignmentdate == null)
                    {
                        ErrorTextBlock.Text = "���� ��������� ������ �� �����������";
                        return;
                    }

                    // 3. �������� �������
                    wallet.Summ -= _rentalPrice;
                    context.Wallets.Update(wallet);

                    // 4. ���������� ������ �� 30 ����
                    horseStable.Assignmentdate = horseStable.Assignmentdate.Value.AddDays(30);
                    context.HorseStables.Update(horseStable);

                    // 5. ��������� ���������
                    await context.SaveChangesAsync();
                    Ownermenu.LoadUserData();

                    // ��������� ���� � ����������� true
                    Close(true);
                }
            }
            catch (Exception ex)
            {
                ErrorTextBlock.Text = $"������: {ex.Message}";
            }
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}