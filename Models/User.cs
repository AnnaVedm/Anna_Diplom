using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TyutyunnikovaAnna_Diplom.Models;

public partial class User
{
    public int Id { get; set; }

    public string? Login { get; set; }

    public string? Email { get; set; }

    public string? PasswordHash { get; set; }

    public string? GoogleId { get; set; }

    public string? Name { get; set; }

    public string? Surname { get; set; }

    public string? Biography { get; set; }

    public int? Roleid { get; set; }

    public string? UserPhoto { get; set; }

    public string? Zasluga1 { get; set; }

    public string? Zasluga2 { get; set; }

    public string? Zasluga3 { get; set; }

    public string? Zasluga4 { get; set; }

    [NotMapped]
    public string Fullname
    {
        get
        {
            return $"{Name} {Surname}";
        }
    }

    [NotMapped]
    public string UserStatus
    {
        get
        {
            var status = "";
            switch (Roleid)
            {
                case 1:
                    status = "Администратор";
                    break;
                case 2:
                    status = "Берейтор";
                    break;
                case 3:
                    status = "Клиент";
                    break;
            }

            return status;
        }
    }

    public Bitmap User_Image
    {
        get { return new Bitmap(AppDomain.CurrentDomain.BaseDirectory + "/images/" + UserPhoto); }
    }

    public virtual ICollection<BreederTrainingType> BreederTrainingTypes { get; } = new List<BreederTrainingType>();

    public virtual ICollection<BreederTraining> BreederTrainings { get; } = new List<BreederTraining>();

    public virtual ICollection<Competition> Competitions { get; } = new List<Competition>();

    public virtual Role? Role { get; set; }

    public virtual ICollection<UserHorse> UserHorses { get; } = new List<UserHorse>();

    public virtual ICollection<Wallet> Wallets { get; } = new List<Wallet>();
}
