using System;
using System.Collections.Generic;

namespace TyutyunnikovaAnna_Diplom.Models;

public partial class Wallet
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public decimal Summ { get; set; }

    public virtual User User { get; set; } = null!;
}
