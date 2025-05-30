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

public partial class Competitions : UserControl
{
    string search_result = string.Empty;
    string sort_result;
    string filter_result;
    public ObservableCollection<Competition> Competition_spisok { get; set; } = new ObservableCollection<Competition>();
    public Competitions()
    {
        InitializeComponent();
        DataContext = this;
        LoadComp();
    }

    public void LoadComp()
    {
        using(var context = new DiplomHorseClubContext())
        {
            var comp = context.Competitions.ToList();

            Competition_spisok.Clear();
            foreach(var s in comp)
            {
                Competition_spisok.Add(s);
            }
        }
    }

    private void SearchTextBox_KeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if(!string.IsNullOrEmpty(Search_Textbox.Text))
        {
            search_result = Search_Textbox.Text.ToLower();
            filter_search_sort();
        }
        else
        {
            search_result = string.Empty;
            LoadComp();
        }
    }

    private void filter_search_sort()
    {
        string[] search_list = search_result.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        
        using(var context = new DiplomHorseClubContext())
        {
            var f_s_s = context.Competitions.ToList();

            f_s_s = f_s_s.Where(competition => search_list.All(tapword => (competition.Competitiontype.ToLower().Contains(tapword))
            || (competition.Name.ToLower().Contains(tapword)))).ToList();
        
            switch(sort_result)
            {
                case "По дате (сначала ближайшие)":
                    f_s_s = f_s_s.OrderBy(a => a.Date).ToList();
                    break;
                case "По дате (сначала дальние)":
                    f_s_s = f_s_s.OrderByDescending(a => a.Date).ToList();
                    break;
                default:
                    break;
            }

            switch(filter_result)
            {
                case "Грядущие":
                    f_s_s = f_s_s.Where(a => a.Date > DateOnly.FromDateTime(DateTime.Now)).ToList();
                    break;
                case "Прошедшие":
                    f_s_s = f_s_s.Where(a => a.Date < DateOnly.FromDateTime(DateTime.Now)).ToList();
                    break;
                default:
                    break;
            }

            Competition_spisok.Clear();
            //f_s_s = f_s_s;
            foreach(var c in f_s_s)
            {
                Competition_spisok.Add(c);
            }
        }
    }

    private void SortComboBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        using(var context = new DiplomHorseClubContext())
        {
            ComboBox combobox = sender as ComboBox;
            if(combobox != null)
            {
                ComboBoxItem item = combobox.SelectedItem as ComboBoxItem;
                if(item != null)
                {
                    sort_result = item.Content.ToString();
                    filter_search_sort();
                }
            }
        }
    }

    private void FilterComboBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        using (var context = new DiplomHorseClubContext())
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
    }
}