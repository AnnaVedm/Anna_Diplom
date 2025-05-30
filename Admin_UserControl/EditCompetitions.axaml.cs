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

public partial class EditCompetitions : UserControl
{
    public ObservableCollection<Competition> Competition_spisok { get; set; } = new ObservableCollection<Competition>();
    public EditCompetitions()
    {
        InitializeComponent();
        LoadComp();
        DataContext = this;
    }

    public void LoadComp()
    {
        using (var context = new DiplomHorseClubContext())
        {
            var comp = context.Competitions.ToList();

            Competition_spisok.Clear();
            foreach (var s in comp)
            {
                Competition_spisok.Add(s);
            }
        }
    }

    private async void AddCompetitionButton_Click(object sender, RoutedEventArgs e)
    {
        var addComp = new AddCompetition();
        var result = await addComp.ShowDialog<Competition>(this.GetVisualRoot() as AdminMenu);
        if (result != null)
        {
            Competition_spisok.Add(result);
            //добавляем соревнование в список для отображения в UI
        }
    }

    private async void DeleteCompetitionButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is Competition competition)
        {
            var message = new CustomMessageBox("Вы уверены, что хотите удалить соревнование?", true);
            var result = await message.ShowDialog<bool>(this.GetVisualRoot() as AdminMenu);

            if (result == true)
            {
                using (var context = new DiplomHorseClubContext())
                {
                    Competition_spisok.Remove(competition);

                    context.Competitions.Remove(competition);
                    context.SaveChanges();

                    message = new CustomMessageBox("Соревнование успешно удалено!", true);
                    await message.ShowDialog(this.GetVisualRoot() as AdminMenu);
                }
            }
        }
    }
}