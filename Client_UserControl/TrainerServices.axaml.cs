using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TyutyunnikovaAnna_Diplom.Context;
using TyutyunnikovaAnna_Diplom.Models;

namespace TyutyunnikovaAnna_Diplom;

public partial class TrainerServices : UserControl
{
    string search_result = string.Empty;

    public ObservableCollection<User> Trainers_spisok { get; set; } = new ObservableCollection<User>();

    public TrainerServices()
    {
        InitializeComponent();
        DataContext = this;
        LoadTrainers();
    }

    public void LoadTrainers()
    {
        using (var context = new DiplomHorseClubContext())
        {
            var trainers = context.Users
                .Where(u => u.Roleid == 2)
                .Include(u => u.BreederTrainingTypes)
                .ThenInclude(btt => btt.Trainingtype)
                .ToList();

            Trainers_spisok.Clear();
            foreach (var trainer in trainers)
            {
                Trainers_spisok.Add(trainer);
            }
        }
    }

    private void SearchTextBox_KeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if(!string.IsNullOrEmpty(Search_textbox.Text))
        {
            search_result = Search_textbox.Text.ToLower();
            Filter_search_sort();
        }
        else
        {
            search_result = string.Empty;
            LoadTrainers();
        }
    }

    private void Filter_search_sort()
    {
        string[] search_list = search_result.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

        using (var context = new DiplomHorseClubContext())
        {
            var f_s_s = context.Users.Where(a => a.Roleid == 2).ToList();

            f_s_s = f_s_s.Where(trainer => search_list.All(tapword => (trainer.Name.ToLower().Contains(tapword)))).ToList();

            Trainers_spisok.Clear();

            foreach(var s in f_s_s)
            {
                Trainers_spisok.Add(s);
            }
        }
    }

    private async void ZapisButton_Click(object? sender, RoutedEventArgs e)
    {
        var tran = this.GetVisualRoot() as Menu;     
        var button = sender as Button;

        if (button != null)
        {
            // Получаем выбранный тип тренировки из ComboBox
            var selectedItem = BreederTrainingType;

            if (selectedItem == null)
            {
                // Показать ошибку, если тип не выбран
                return;
            }

            var tran1 = new ZapisNaTren();

            tran1.Current_breeder = (User)button.DataContext;
            tran1.Costoverriding = this.Costoverride;

            // Передаем ID типа тренировки
            //tran1.Breedertrainingtypeid = selectedItem.Id;
            //        

            using(var context = new DiplomHorseClubContext())
            {
                var trainingType = context.TrainingTypes
                     .Include(b => b.BreederTrainingTypes)
                     .FirstOrDefault(t => t.Id == BreederTrainingType.Trainingtypeid);

                if (trainingType != null)
                {
                    tran1.Breedertrainingtypeid = trainingType.Id;
                    tran1.Duration = (int)trainingType.Duration;
                    tran1.UslugaName = trainingType.Name;
                }

                //var trainingType = context.TrainingTypes
                //    .FirstOrDefault(tt => tt.Id == tran1.Breedertrainingtypeid);

                //tran1.Duration = (int)trainingType.Duration;
                //tran1.UslugaName = trainingType.Name;
            }

            //контекст базового окна
            var content = this.GetVisualRoot() as Menu;
            tran1.OwnerMenu = content;

            var result = await tran1.ShowDialog<bool>(tran);

            if (result)
            {
                LoadTrainers();
            }
        }
    }

    public decimal Costoverride { get; set; }

    public BreederTrainingType BreederTrainingType { get; set; }

    private void ChooseTrainComboBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        using (var context = new DiplomHorseClubContext())
        {
            ComboBox combobox = sender as ComboBox;
            if (combobox != null)
            {
                var item = combobox.SelectedItem as BreederTrainingType;
                if (item != null)
                {
                    Costoverride = (decimal)item.Costoverride;
                    BreederTrainingType = item;
                }
            }
        }
    }
}