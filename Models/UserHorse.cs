using System;
using System.Collections.Generic;

namespace TyutyunnikovaAnna_Diplom.Models;

public partial class UserHorse
{
    public int Id { get; set; }

    public int? Userid { get; set; }

    public int? Horseid { get; set; }

    public virtual Horse? Horse { get; set; }

    public virtual User? User { get; set; }
}
