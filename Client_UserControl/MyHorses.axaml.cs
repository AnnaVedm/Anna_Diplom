using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using TyutyunnikovaAnna_Diplom.AccountData;
using TyutyunnikovaAnna_Diplom.Context;
using TyutyunnikovaAnna_Diplom.Models;

namespace TyutyunnikovaAnna_Diplom;

public partial class MyHorses : UserControl
{
    public ObservableCollection<Horse> Horses_list { get; set; } = new ObservableCollection<Horse>();
    string search_result = " ";
    string sort_result;
    string filter_result;
    public Menu OwnerMenu { get; set; }

    public MyHorses()
    {
        InitializeComponent();
        DataContext = this;
        // Проверяем просроченных лошадей при создании контрола
        CheckAndRemoveExpiredHorses();

        Load_horses();

        // Опционально: добавляем фоновую проверку
        StartBackgroundCheck();
    }

    private IDisposable _backgroundTimer;
    private void StartBackgroundCheck()
    {
        // Проверка каждые 12 часов (можно настроить интервал)
        _backgroundTimer = DispatcherTimer.Run(() =>
        {
            CheckAndRemoveExpiredHorses();
            return true; // продолжать работу таймера
        }, TimeSpan.FromHours(12), DispatcherPriority.Background);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        _backgroundTimer?.Dispose();
        base.OnUnloaded(e);
    }

    public void Load_horses()
    {
        using (var context = new DiplomHorseClubContext())
        {
            var userHorses = context.UserHorses
                .Include(a => a.Horse)
                    .ThenInclude(a => a.HorseStables)
                        .ThenInclude(a => a.Stable)
                            .ThenInclude(a => a.Type)
                .Where(a => a.Userid == UserForAuthorization.SelectedUser.Id)
                .Select(a => a.Horse)
                .ToList();

            Horses_list.Clear();
            foreach (var horse in userHorses)
            {
                Horses_list.Add(horse);
            }

            // Обновляем видимость элементов
            NoHorsesPanel.IsVisible = !Horses_list.Any();
            HorsesListBox.IsVisible = Horses_list.Any();
        }
    }

    private void SearchTextBox_KeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (!string.IsNullOrEmpty(Search_Textbox.Text))
        {
            search_result = Search_Textbox.Text.ToLower();
            filter_search_sort();
        }
        else
        {
            search_result = " ";
            filter_search_sort();
        }
    }

    private void filter_search_sort()
    {
        string[] search_list = search_result.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

        using (var context = new DiplomHorseClubContext())
        {
            // Загружаем данные с ВСЕМИ связями, включая Type
            var filtered_search_sort_Horses = context.UserHorses
                .Include(a => a.Horse)
                    .ThenInclude(h => h.HorseStables)
                        .ThenInclude(hs => hs.Stable)
                            .ThenInclude(s => s.Type) // Важно: загружаем Type!
                .Where(a => a.Userid == UserForAuthorization.SelectedUser.Id)
                .Select(a => a.Horse)
                .AsNoTracking() // Отключаем отслеживание, чтобы избежать конфликтов
                .ToList();

            // Поиск по имени или породе
            filtered_search_sort_Horses = filtered_search_sort_Horses
                .Where(horse => search_list.All(tapword =>
                    horse.HorseName.ToLower().Contains(tapword) ||
                    horse.Breed.ToLower().Contains(tapword)))
                .ToList();

            // Фильтрация по типу конюшни
            switch (filter_result)
            {
                case "Малый":
                    filtered_search_sort_Horses = filtered_search_sort_Horses
                        .Where(horse => horse.HorseStables.Any(hs => hs.Stable.Typeid == 1))
                        .ToList();
                    break;
                case "Средний":
                    filtered_search_sort_Horses = filtered_search_sort_Horses
                        .Where(horse => horse.HorseStables.Any(hs => hs.Stable.Typeid == 2))
                        .ToList();
                    break;
                case "Большой":
                    filtered_search_sort_Horses = filtered_search_sort_Horses
                        .Where(horse => horse.HorseStables.Any(hs => hs.Stable.Typeid == 3))
                        .ToList();
                    break;
                case "Фильтрация":
                default:
                    break;
            }

            switch(sort_result)
            {
                case "По дате окончания аренды(сначала те, что скоро)":
                    filtered_search_sort_Horses = filtered_search_sort_Horses
                        .OrderBy(horse =>
                            horse.HorseStables
                                .OrderBy(hs => hs.Assignmentdate)  // Сначала сортируем по возрастанию даты
                                .FirstOrDefault()?.Assignmentdate ?? DateOnly.MaxValue)  // Берем самую раннюю дату
                        .ToList();
                    break;
                case "По дате окончания аренды(сначала те, что не скоро)":
                    filtered_search_sort_Horses = filtered_search_sort_Horses
                        .OrderByDescending(horse =>
                            horse.HorseStables
                                .OrderBy(hs => hs.Assignmentdate)  // Сначала сортируем по возрастанию даты
                                .FirstOrDefault()?.Assignmentdate ?? DateOnly.MinValue)  // Берем самую раннюю дату и сортируем по убыванию
                        .ToList();
                    break;
                default:
                    break;
            }

            filtered_search_sort_Horses = filtered_search_sort_Horses.ToList(); //Для одновременной работы

            // Обновляем список
            Horses_list.Clear();
            foreach (var horse in filtered_search_sort_Horses)
            {
                Horses_list.Add(horse);
            }
        }
    }

    private void FilterComboBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        ComboBox combobox = sender as ComboBox;
        if (combobox != null)
        {
            ComboBoxItem item = combobox.SelectedItem as ComboBoxItem;
            if (item != null)
            {
                filter_result = item.Content.ToString();

                filter_search_sort();
            }
        }
    }

    private void SortComboBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        ComboBox combobox = sender as ComboBox;
        if (combobox != null)
        {
            ComboBoxItem item = combobox.SelectedItem as ComboBoxItem;
            if (item != null)
            {
                sort_result = item.Content.ToString();
                filter_search_sort();
            }
        }
    }

    private async void AddHorseButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        AddHorse add = new AddHorse(/*UserForAuthorization.SelectedUser*/);
        var Content = this.GetVisualRoot() as Menu;

        add.OwnerMenu = Content; // Передаем ссылку на контейнер
        await add.ShowDialog<bool>(Content);

        Load_horses();
    }

    private async void DeleteHorseButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is Horse horseToDelete)
        {
            // Подтверждение удаления
            var confirmDialog = new CustomMessageBox("Вы уверены, что хотите удалить лошадь и все связанные данные?", true);
            var Content = this.GetVisualRoot() as Menu;
            var confirmResult = await confirmDialog.ShowDialog<bool>(Content);
            if (!confirmResult) return;

            try
            {
                using (var context = new DiplomHorseClubContext())
                {
                    // Начинаем транзакцию
                    using (var transaction = await context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            // 1. Загружаем лошадь со всеми связями
                            var horse = await context.Horses
                                .Include(h => h.UserHorses)
                                .Include(h => h.HorseStables)
                                    .ThenInclude(hs => hs.Stable)
                                        .ThenInclude(s => s.Type)
                                .Include(h => h.BreederTrainings)
                                .FirstOrDefaultAsync(h => h.Id == horseToDelete.Id);

                            if (horse == null)
                            {
                                await new CustomMessageBox("Лошадь не найдена!").ShowDialog(Content);
                                return;
                            }

                            // 2. Обновляем количество доступных денников
                            foreach (var horseStable in horse.HorseStables)
                            {
                                if (horseStable.Stable?.Type != null)
                                {
                                    horseStable.Stable.Type.StablesArendable += 1;
                                    context.Stabletypes.Update(horseStable.Stable.Type);
                                }
                            }

                            // 3. Удаляем тренировки берейторов
                            context.BreederTrainings.RemoveRange(horse.BreederTrainings);

                            // 4. Удаляем связи пользователей
                            context.UserHorses.RemoveRange(horse.UserHorses);

                            // 5. Получаем денники для удаления
                            var stablesToDelete = horse.HorseStables
                                .Select(hs => hs.Stable)
                                .Where(s => s != null)
                                .ToList();

                            // 6. Удаляем связи лошадь-денник
                            context.HorseStables.RemoveRange(horse.HorseStables);

                            // 7. Удаляем сами денники
                            context.Stables.RemoveRange(stablesToDelete);

                            // 8. Удаляем саму лошадь
                            context.Horses.Remove(horse);

                            // Сохраняем изменения
                            int changes = await context.SaveChangesAsync();
                            await transaction.CommitAsync();

                            if (changes > 0)
                            {
                                await new CustomMessageBox($"Удалено {changes} записей").ShowDialog(Content);
                                Load_horses(); // Обновляем список
                            }
                            else
                            {
                                await new CustomMessageBox("Не удалось удалить данные").ShowDialog(Content);
                            }
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            await new CustomMessageBox($"Ошибка при удалении: {ex.Message}").ShowDialog(Content);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await new CustomMessageBox($"Критическая ошибка: {ex.Message}").ShowDialog(Content);
            }
        }
    }

    private async void ToDoubleArendStable_ButtonClick(object? sender, RoutedEventArgs e) // Продление
    {
        if (sender is Button button && button.DataContext is Horse selectedHorse)
        {
            var dialog = new ToDoubleArendStable(selectedHorse);
            dialog.Ownermenu = this.GetVisualRoot() as Menu;
            var result = await dialog.ShowDialog<bool>(this.GetVisualRoot() as Window);

            if (result)
            {
                Load_horses(); // Обновляем список если было продление
                               // Обновляем данные пользователя напрямую
                using (var context = new DiplomHorseClubContext())
                {
                    var user = await context.Users
                        .Include(u => u.Wallets)
                        .FirstOrDefaultAsync(u => u.Id == UserForAuthorization.SelectedUser.Id);
                    UserForAuthorization.SelectedUser = user;
                }
            }
        }
    }

    private async void CheckAndRemoveExpiredHorses() // удаление лошади
    {
        try
        {
            if (UserForAuthorization.SelectedUser?.Id == null)
            {
                Console.WriteLine("Пользователь не авторизован");
                return;
            }

            using (var context = new DiplomHorseClubContext())
            {
                var currentDate = DateOnly.FromDateTime(DateTime.Now);

                var expiredHorses = await context.Horses
                    .Include(h => h.UserHorses)
                    .Include(h => h.HorseStables)
                        .ThenInclude(hs => hs.Stable)
                            .ThenInclude(s => s.Type)
                    .Include(h => h.BreederTrainings)
                    .Where(h => h.UserHorses.Any(uh => uh.Userid == UserForAuthorization.SelectedUser.Id) &&
                               h.HorseStables != null &&
                               h.HorseStables.Any(hs => hs.Assignmentdate != null &&
                                                      hs.Assignmentdate < currentDate))
                    .AsNoTracking() // Чтобы избежать проблем с отслеживанием
                    .ToListAsync();

                if (!expiredHorses.Any()) return;

                var parentWindow = this.GetVisualRoot() as Window;
                var confirm = await new CustomMessageBox(
                    $"Найдено {expiredHorses.Count} лошадей с истекшей арендой. Они удалены.")
                    .ShowDialog<bool>(parentWindow);

                if (!confirm) return;

                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        foreach (var horse in expiredHorses)
                        {
                            // Обновляем доступные денники
                            foreach (var stable in horse.HorseStables
                                .Where(hs => hs.Stable?.Type != null)
                                .Select(hs => hs.Stable))
                            {
                                stable.Type.StablesArendable += 1;
                                context.Entry(stable.Type).State = EntityState.Modified;
                            }

                            // Удаляем связанные данные
                            context.BreederTrainings.RemoveRange(horse.BreederTrainings);
                            context.UserHorses.RemoveRange(horse.UserHorses);

                            var stables = horse.HorseStables
                                .Select(hs => hs.Stable)
                                .Where(s => s != null)
                                .ToList();

                            context.HorseStables.RemoveRange(horse.HorseStables);
                            context.Stables.RemoveRange(stables);

                            context.Horses.Remove(horse);
                        }

                        var changes = await context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        await new CustomMessageBox($"Удалено {changes} записей")
                            .ShowDialog<bool>(parentWindow);

                        Load_horses();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        await new CustomMessageBox($"Ошибка: {ex.Message}")
                            .ShowDialog<bool>(parentWindow);
                        Console.WriteLine($"TRANSACTION ERROR: {ex}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FATAL ERROR: {ex}");
            var parent = this.GetVisualRoot() as Window;
            await new CustomMessageBox("Системная ошибка при обработке")
                .ShowDialog<bool>(parent);
        }
    }

    private async void ChangeStableButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is Horse selectedHorse)
        {
            try
            {
                using (var context = new DiplomHorseClubContext())
                {
                    // Загружаем лошадь с текущим денником
                    var horse = await context.Horses
                        .Include(h => h.HorseStables)
                            .ThenInclude(hs => hs.Stable)
                                .ThenInclude(s => s.Type)
                        .FirstOrDefaultAsync(h => h.Id == selectedHorse.Id);

                    if (horse?.HorseStables.FirstOrDefault()?.Stable == null)
                    {
                        await new CustomMessageBox("Ошибка: не найден текущий денник лошади").ShowDialog(this.GetVisualRoot() as Window);
                        return;
                    }

                    var currentHorseStable = horse.HorseStables.First();
                    var currentStable = currentHorseStable.Stable;
                    var currentType = currentStable.Type;

                    // Открываем окно выбора нового типа денника
                    var selectStableDialog = new SelectStable();
                    var selectedType = await selectStableDialog.ShowDialog<bool>(this.GetVisualRoot() as Window);

                    if (!selectedType) return;

                    var NewStable = selectStableDialog.SelectedStable;

                    // Проверяем, не выбрал ли пользователь тот же тип денника
                    if (NewStable.Id == currentType?.Id)
                    {
                        await new CustomMessageBox("Вы выбрали тот же тип денника").ShowDialog(this.GetVisualRoot() as Window);
                        return;
                    }

                    // Загружаем пользователя с кошельком
                    var user = await context.Users
                        .Include(u => u.Wallets)
                        .FirstOrDefaultAsync(u => u.Id == UserForAuthorization.SelectedUser.Id);

                    if (user?.Wallets.FirstOrDefault() == null)
                    {
                        await new CustomMessageBox("Ошибка: не найден кошелек пользователя").ShowDialog(this.GetVisualRoot() as Window);
                        return;
                    }

                    var wallet = user.Wallets.First();

                    // Проверяем баланс
                    if (wallet.Summ < NewStable.Cost)
                    {
                        var confirm = await new CustomMessageBox(
                            $"Недостаточно средств. Требуется: {NewStable.Cost} руб. Хотите пополнить баланс?",
                            true).ShowDialog<bool>(this.GetVisualRoot() as Window);

                        if (confirm)
                        {
                            var walletDialog = new WalletBalanceUpdate();
                            walletDialog.OwnerMenu = this.GetVisualRoot() as Menu;
                            await walletDialog.ShowDialog(this.GetVisualRoot() as Window);
                            return;
                        }
                        return;
                    }

                    // Начинаем транзакцию
                    using (var transaction = await context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            // 1. Освобождаем текущий денник
                            if (currentType != null)
                            {
                                currentType.StablesArendable += 1;
                                context.Stabletypes.Update(currentType);
                            }

                            // 2. Занимаем новый денник
                            var newType = await context.Stabletypes.FindAsync(NewStable.Id);
                            if (newType == null)
                            {
                                await new CustomMessageBox("Ошибка: выбранный тип денника не найден").ShowDialog(this.GetVisualRoot() as Window);
                                return;
                            }

                            newType.StablesArendable -= 1;
                            context.Stabletypes.Update(newType);


                            // Получаем все StableCode, пытаемся преобразовать их в int, игнорируя ошибки
                            var stableCodes = context.Stables
                                .Select(s => s.StableCode)
                                .ToList(); // сначала выгружаем данные в память

                            int maxStableCode = 0;
                            foreach (var code in stableCodes)
                            {
                                if (int.TryParse(code, out int parsedCode))
                                {
                                    if (parsedCode > maxStableCode)
                                        maxStableCode = parsedCode;
                                }
                            }

                            var newStable = new Stable
                            {
                                Id = context.Stables.Any() ? context.Stables.Max(s => s.Id) + 1 : 1,
                                Typeid = NewStable.Id,
                                StableCode = (maxStableCode + 1).ToString()
                            };

                            context.Stables.Add(newStable);
                            await context.SaveChangesAsync(); // Сохраняем, чтобы получить ID

                            // 4. Обновляем связь лошадь-денник
                            currentHorseStable.Stableid = newStable.Id;
                            currentHorseStable.Assignmentdate = DateOnly.FromDateTime(DateTime.Now.AddMonths(1));
                            context.HorseStables.Update(currentHorseStable);

                            // 5. Удаляем старый денник
                            context.Stables.Remove(currentStable);

                            // 6. Списание средств
                            wallet.Summ -= NewStable.Cost ?? 0;
                            context.Wallets.Update(wallet);

                            // Сохраняем все изменения
                            await context.SaveChangesAsync();
                            await transaction.CommitAsync();
                            OwnerMenu.LoadUserData();
                            

                            // Обновляем данные пользователя
                            UserForAuthorization.SelectedUser = user;

                            await new CustomMessageBox("Денник успешно изменен!").ShowDialog(this.GetVisualRoot() as Window);
                            Load_horses();
                        }
                        catch (DbUpdateException dbEx)
                        {
                            await transaction.RollbackAsync();
                            var errorMessage = dbEx.InnerException != null
                                ? dbEx.InnerException.Message
                                : dbEx.Message;
                            await new CustomMessageBox($"Ошибка базы данных: {errorMessage}")
                                .ShowDialog(this.GetVisualRoot() as Window);
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            await new CustomMessageBox($"Ошибка: {ex.Message}").ShowDialog(this.GetVisualRoot() as Window);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await new CustomMessageBox($"Неожиданная ошибка: {ex.Message}").ShowDialog(this.GetVisualRoot() as Window);
            }
        }
    }
}