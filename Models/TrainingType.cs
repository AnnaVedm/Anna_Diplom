using System;
using System.Collections.Generic;

namespace TyutyunnikovaAnna_Diplom.Models;

public partial class TrainingType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Basecost { get; set; }

    public int? Duration { get; set; }

    public virtual ICollection<BreederTrainingType> BreederTrainingTypes { get; } = new List<BreederTrainingType>();
}
