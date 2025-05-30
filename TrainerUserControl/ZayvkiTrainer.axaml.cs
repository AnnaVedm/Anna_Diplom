using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TyutyunnikovaAnna_Diplom.AccountData;
using TyutyunnikovaAnna_Diplom.Context;
using TyutyunnikovaAnna_Diplom.Models;

namespace TyutyunnikovaAnna_Diplom;

public partial class ZayvkiTrainer : UserControl
{
    private string _searchresult = string.Empty;
    private string _filterresult = "Фильтрация";
    private string _sortresult = string.Empty;

    public ObservableCollection<BreederTraining> Breedertrainigs_spisok { get; set; } = new ObservableCollection<BreederTraining>();

    public ObservableCollection<string> Trainingtypes_spisok { get; set; } = new ObservableCollection<string>();
    public ZayvkiTrainer()
    {
        InitializeComponent();
        Load_BreederTrainigSpisok();

        DataContext = this;
    }

    private void SearchFilterSortBreederSpisok()
    {
        using (var context = new DiplomHorseClubContext())
        {
            string[] searchwords = _searchresult.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

            var f_s_c = context.BreederTrainings
                    .Include(t => t.Breedertrainingtype)
                        .ThenInclude(t => t.Trainingtype)
                    .Include(h => h.Horse)
                    .Include(u => u.User)
                    .Where(a => a.Status == "Принята" && a.Userid == UserForAuthorization.SelectedUser.Id)
                    .ToList();

            //поиск по владельцу, типу трени и имени лошади

            f_s_c = f_s_c.Where(training =>
                searchwords.All(word =>
                    (training.Breedertrainingtype.Trainingtype.Name != null && training.Breedertrainingtype.Trainingtype.Name.ToLower().Contains(word)) ||
                    (training.Horse.HorseName != null && training.Horse.HorseName.ToLower().Contains(word)) ||
                    (training.User.Name != null && training.User.Name.ToLower().Contains(word)) ||
                    (training.User.Surname != null && training.User.Surname.ToLower().Contains(word)) 
                  )).ToList();

            //фильтрация
            if (_filterresult == "Фильтрация")
            {
                f_s_c = f_s_c.ToList();
            }
            else
            {
                f_s_c = f_s_c.Where(a => a.Breedertrainingtype.Trainingtype.Name == _filterresult).ToList();
            }

            //сортировка
            switch (_sortresult)
            {
                case "По дате тренировки(убывание)":
                    f_s_c = f_s_c.OrderByDescending(a => a.Startdate).ToList();
                    break;
                case "По дате тренировки(возрастание)":
                    f_s_c = f_s_c.OrderBy(a => a.Startdate).ToList();
                    break;
            }

            Breedertrainigs_spisok.Clear();
            foreach (var training in f_s_c)
            {
                Breedertrainigs_spisok.Add(training);
            }
        }
    }

    private void Load_BreederTrainigSpisok()
    {
        using (var context = new DiplomHorseClubContext())
        {
            var trainingSpisok = context.BreederTrainings
                    .Include(t => t.Breedertrainingtype)
                        .ThenInclude(t => t.Trainingtype)
                    .Include(h => h.Horse)
                    .Include(u => u.User)
                    .Where(a => a.Status == "Принята")
                    .ToList();

            Breedertrainigs_spisok.Clear();
            foreach (var item in trainingSpisok)
            {
                Breedertrainigs_spisok.Add(item);
            }

            Load_TrainingTypesComboBox();
        }
    }

    private void Load_TrainingTypesComboBox()
    {
        Trainingtypes_spisok.Clear();

        var types = Breedertrainigs_spisok.Select(a => a.Breedertrainingtype.Trainingtype.Name).Distinct().ToList();

        types.Insert(0, "Фильтрация");
        Trainingtypes_spisok = new ObservableCollection<string>(types);
    }

    private void Search_KeyUp(object sender, KeyEventArgs e)
    {
        if (!string.IsNullOrEmpty(Search_Textbox.Text))
        {
            _searchresult = Search_Textbox.Text.ToLower();
        }
        else
        {
            _searchresult = string.Empty;
        }

        SearchFilterSortBreederSpisok();
    }

    private void FilterType_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox && comboBox.SelectedItem is string training)
        {
            _filterresult = training;
            SearchFilterSortBreederSpisok();
        }
    }

    private void SortType_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem training)
        {
            _sortresult = training.Content.ToString();
            SearchFilterSortBreederSpisok();
        }
    }
}