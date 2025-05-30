using System;
using System.Collections.Generic;

namespace TyutyunnikovaAnna_Diplom.Models;

public partial class HorseStable
{
    public int Id { get; set; }

    public int? Horseid { get; set; }

    public int? Stableid { get; set; }

    public DateOnly? Assignmentdate { get; set; }

    public virtual Horse? Horse { get; set; }

    public virtual Stable? Stable { get; set; }
}
