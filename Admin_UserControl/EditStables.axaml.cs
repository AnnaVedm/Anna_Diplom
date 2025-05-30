using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using System.Collections.ObjectModel;
using System.Linq;
using TyutyunnikovaAnna_Diplom.Context;
using TyutyunnikovaAnna_Diplom.Models;

namespace TyutyunnikovaAnna_Diplom;

public partial class EditStables : UserControl
{
    //тут будет вывод всех денников с возможностью редактировать количество и стоимость
    public ObservableCollection<Stabletype> Stables_spisok { get; set; } = new ObservableCollection<Stabletype>();
    public EditStables()
    {
        InitializeComponent();
        LoadStables_spisok();
        DataContext = this;
    }

    private void LoadStables_spisok()
    {
        using (var context = new DiplomHorseClubContext())
        {
            var stables = context.Stabletypes.OrderBy(a => a.Id).ToList();

            Stables_spisok.Clear();
            foreach (var item in stables)
            {
                Stables_spisok.Add(item);
            }
        }
    }

    //обработчики кнопок - и + для изменения кол-ва доступных денников
    //вызываем асинхронно чтобы не блокировать ui поток при частых операциях с сохранением данных в БД
    private async void PlusButton_Click(object? sender, RoutedEventArgs e)
    {
        using (var context = new DiplomHorseClubContext())
        {
            if (sender is Button btn && btn.DataContext is Stabletype stable)
            {
                stable.StablesArendable = (stable.StablesArendable ?? 0) + 1;
                context.Update(stable);
                await context.SaveChangesAsync();
            }
        }
    }

    //вызываем асинхронно чтобы не блокировать ui поток при частых операциях с сохранением данных в БД
    private async void MinusButton_Click(object? sender, RoutedEventArgs e)
    {
        using (var context = new DiplomHorseClubContext())
        {
            if (sender is Button btn && btn.DataContext is Stabletype stable)
            {
                if ((stable.StablesArendable ?? 0) > 0)
                {
                    stable.StablesArendable = stable.StablesArendable - 1;
                    context.Update(stable);
                    await context.SaveChangesAsync();
                }    
            }
        }
    }

    private void EnableEditButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is Stabletype stable)
        {
            stable.IsStableEditable = true;
        }
    }

    private async void ApplyStableEditButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is Stabletype stable)
        {
            using (var context = new DiplomHorseClubContext())
            {
                // Получаем сущность из базы по ключу (Id)
                var stableUpdate = context.Stabletypes.Find(stable.Id);

                if (stableUpdate != null)
                {
                    // Проверяем, что Costoverride - число
                    if (decimal.TryParse(stable.Cost.ToString(), out decimal cost))
                    {
                        stableUpdate.Cost = (int?)cost;

                        context.Stabletypes.Update(stableUpdate); // Сохраняем изменения
                        //context.BreederTrainingTypes.Update(training); // Сохраняем изменения
                        context.SaveChanges();

                        //перезагружаем данные
                        LoadStables_spisok();

                        var message = new CustomMessageBox($"Стоимость денника {stable.Name} успешно изменена!", false); ;
                        await message.ShowDialog(this.GetVisualRoot() as AdminMenu);

                    }
                    else
                    {
                        // Если не удалось преобразовать, сообщаем об ошибке
                        var message = new CustomMessageBox("Неверный формат стоимости. Введите число.", false);
                        await message.ShowDialog(this.GetVisualRoot() as AdminMenu);
                    }
                }
            }

            // Закрываем режим редактирования
            stable.IsStableEditable = false;
        }
    }
}