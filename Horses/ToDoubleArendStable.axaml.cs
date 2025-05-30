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
                    // Получаем тип денника для определения цены
                    var stableType = await context.HorseStables
                        .Include(hs => hs.Stable)
                            .ThenInclude(s => s.Type)
                        .Where(hs => hs.Horseid == _selectedHorse.Id)
                        .Select(hs => hs.Stable.Type)
                        .FirstOrDefaultAsync();

                    if (stableType != null && stableType.Cost.HasValue)
                    {
                        _rentalPrice = (decimal)stableType.Cost.Value; // Явное преобразование int в decimal
                        PriceTextBlock.Text = $"Стоимость продления: {_rentalPrice} руб.";
                    }
                    else
                    {
                        ErrorTextBlock.Text = "Не удалось определить стоимость аренды (стоимость не установлена)";
                        ExtendButton.IsEnabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorTextBlock.Text = $"Ошибка: {ex.Message}";
                ExtendButton.IsEnabled = false;
            }
        }

        private async void OnExtendButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new DiplomHorseClubContext())
                {
                    // 1. Проверяем баланс пользователя
                    var user = UserForAuthorization.SelectedUser;
                    var wallet = await context.Wallets.FirstOrDefaultAsync(w => w.UserId == user.Id);

                    if (wallet == null)
                    {
                        ErrorTextBlock.Text = "Кошелек пользователя не найден";
                        return;
                    }

                    if (wallet.Summ < _rentalPrice)
                    {
                        ErrorTextBlock.Text = "Недостаточно средств на счету. Сначала пополните кошелёк!";
                        return;
                    }

                    // 2. Находим запись о деннике лошади
                    var horseStable = await context.HorseStables
                        .FirstOrDefaultAsync(hs => hs.Horseid == _selectedHorse.Id);

                    if (horseStable == null)
                    {
                        ErrorTextBlock.Text = "Не найдена информация о деннике";
                        return;
                    }

                    if (horseStable.Assignmentdate == null)
                    {
                        ErrorTextBlock.Text = "Дата окончания аренды не установлена";
                        return;
                    }

                    // 3. Списание средств
                    wallet.Summ -= _rentalPrice;
                    context.Wallets.Update(wallet);

                    // 4. Продлеваем аренду на 30 дней
                    horseStable.Assignmentdate = horseStable.Assignmentdate.Value.AddDays(30);
                    context.HorseStables.Update(horseStable);

                    // 5. Сохраняем изменения
                    await context.SaveChangesAsync();
                    Ownermenu.LoadUserData();

                    // Закрываем окно с результатом true
                    Close(true);
                }
            }
            catch (Exception ex)
            {
                ErrorTextBlock.Text = $"Ошибка: {ex.Message}";
            }
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}