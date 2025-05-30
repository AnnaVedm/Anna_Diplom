using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;

namespace TyutyunnikovaAnna_Diplom.Models;

public partial class Horse
{
    public int Id { get; set; }

    public string? HorseName { get; set; }

    public string? Breed { get; set; }

    public string? Gender { get; set; }

    public DateTime? Datebirth { get; set; }

    public bool? VetPasport { get; set; }

    public string? HorsePhoto { get; set; }

    public Bitmap HorseImage
    {
        get { return new Bitmap(AppDomain.CurrentDomain.BaseDirectory + "/images/" + HorsePhoto); }
    }

    public virtual ICollection<BreederTraining> BreederTrainings { get; } = new List<BreederTraining>();

    public virtual ICollection<HorseStable> HorseStables { get; } = new List<HorseStable>();

    public virtual ICollection<UserHorse> UserHorses { get; } = new List<UserHorse>();
}
