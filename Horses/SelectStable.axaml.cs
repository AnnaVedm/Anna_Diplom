using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using TyutyunnikovaAnna_Diplom.Context;
using TyutyunnikovaAnna_Diplom.Models;

namespace TyutyunnikovaAnna_Diplom
{
    public partial class SelectStable : Window
    {
        public ObservableCollection<Stabletype> Stabletype_spisok { get; set; } = new ObservableCollection<Stabletype>();
        //public Stable SelectedStable { get; private set; }

        public Stabletype SelectedStable { get; set; }

        public int StablePrice { get; private set; }

        public SelectStable()
        {
            InitializeComponent();
            LoadStables();
            DataContext = this;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void LoadStables()
        {
            using (var context = new DiplomHorseClubContext())
            {
                // ѕолучаем все денники, которые еще не зан€ты
                var StablesTypes = context.Stabletypes.ToList();

                Stabletype_spisok.Clear();

                foreach (var c in StablesTypes)
                {
                    Stabletype_spisok.Add(c);
                }

                //var availableStables = context.Stables
                //    .Include(s => s.Type)
                //    .Where(s => !occupiedStables.Contains(s.Id) && s.Type.StablesArendable > 0)
                //    .ToList();

                ////тут был items
                //this.FindControl<ListBox>("StablesListBox").ItemsSource = availableStables;
            } 
        }

        private void SelectStable_DoubleTapped(object sender, TappedEventArgs e)
        {
            if (sender is Border border && border.DataContext is Stabletype stabletypeSelected)
            {
                using (var context = new DiplomHorseClubContext())
                {
                    //»щем выбранный денник через тип
                    //var selectedStable = context.Stables
                    //    .Include(t => t.Type) 
                    //    .FirstOrDefault(a => a.Typeid == stabletypeSelected.Id);

                    SelectedStable = stabletypeSelected;

                    StablePrice = SelectedStable.Cost ?? 0;
                    Close(true);
                }
            }
            else
            {
                Close(false);
            }
        }

        private void CancelButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close(false);
        }
    }
}