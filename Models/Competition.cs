using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;

namespace TyutyunnikovaAnna_Diplom.Models;

public partial class Competition
{
    public int Id { get; set; }

    public int? Winnerid { get; set; }

    public string Name { get; set; } = null!;

    public string Competitiontype { get; set; } = null!;

    public string? Route { get; set; }

    public string? Entryfee { get; set; }

    public DateOnly Date { get; set; }

    public Bitmap Photocompetition
    {
        get { return new Bitmap(AppDomain.CurrentDomain.BaseDirectory + "/images/" + Photocomp); }
    }

    public string? Photocomp { get; set; }

    public virtual User? Winner { get; set; }
}
