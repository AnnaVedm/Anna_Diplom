using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace TyutyunnikovaAnna_Diplom.Models;

public partial class Stabletype : INotifyPropertyChanged
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? Cost { get; set; }

    public string? Description { get; set; }

    public string? StablePhoto { get; set; }


    //добавили уведоление об изменении для интерфейса
    private int? _stablesArendable;
    public int? StablesArendable
    {
        get => _stablesArendable;
        set
        {
            if (_stablesArendable != value)
            {
                _stablesArendable = value;
                OnPropertyChanged();
            }
        }
    }

    private bool _IsStableEditable = false;

    [NotMapped]
    public bool IsStableEditable
    {
        get => _IsStableEditable;
        set
        {
            if (_IsStableEditable != value)
            {
                _IsStableEditable = value;
                OnPropertyChanged();
            }
        }
    }

    public Bitmap Photo
    {
        get { return new Bitmap(AppDomain.CurrentDomain.BaseDirectory + "/images/" + StablePhoto); }
    }

    public virtual ICollection<Stable> Stables { get; } = new List<Stable>();

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
