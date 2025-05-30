using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace TyutyunnikovaAnna_Diplom.Models;

public partial class BreederTrainingType : INotifyPropertyChanged
{
    public int Id { get; set; }

    public int Breederid { get; set; }

    public int Trainingtypeid { get; set; }

    public decimal? Costoverride { get; set; }

    private bool _isEditTrainingEnabled = false;

    [NotMapped]
    public bool IsEditTrainingEnabled
    {
        get => _isEditTrainingEnabled;
        set
        {
            if (_isEditTrainingEnabled != value)
            {
                _isEditTrainingEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    public virtual User Breeder { get; set; } = null!;

    public virtual ICollection<BreederTraining> BreederTrainings { get; } = new List<BreederTraining>();

    public virtual TrainingType Trainingtype { get; set; } = null!;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
