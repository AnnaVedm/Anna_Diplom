using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;

namespace TyutyunnikovaAnna_Diplom.Models;

public partial class Clubnews
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateOnly Date { get; set; }

    public string? Author { get; set; }

    public Bitmap NewsPhoto
    {
        get { return new Bitmap(AppDomain.CurrentDomain.BaseDirectory + "/images/" + Photo); }
    }

    public string? Photo { get; set; }
}
