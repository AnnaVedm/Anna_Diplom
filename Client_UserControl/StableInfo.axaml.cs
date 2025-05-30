using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using TyutyunnikovaAnna_Diplom.Context;
using TyutyunnikovaAnna_Diplom.Models;

namespace TyutyunnikovaAnna_Diplom;

public partial class StableInfo : UserControl
{
    public ObservableCollection<Stabletype> Stable_spisok { get; set; } = new ObservableCollection<Stabletype>();
    public StableInfo()
    {
        InitializeComponent();
        DataContext = this;
        StableLoad();
    }

    public void StableLoad()
    {
        using(var context = new DiplomHorseClubContext())
        {
            var stab = context.Stabletypes.ToList();

            foreach(var s in stab)
            {
                Stable_spisok.Add(s);
            }
        }
    }
}