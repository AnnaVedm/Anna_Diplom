using System;
using System.Collections.Generic;

namespace TyutyunnikovaAnna_Diplom.Models;

public partial class Stable
{
    public int Id { get; set; }

    public string? StableCode { get; set; }

    public int? Typeid { get; set; }

    public virtual ICollection<HorseStable> HorseStables { get; } = new List<HorseStable>();

    public virtual Stabletype? Type { get; set; }
}
