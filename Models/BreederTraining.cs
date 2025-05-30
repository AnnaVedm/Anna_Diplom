using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace TyutyunnikovaAnna_Diplom.Models;

public partial class BreederTraining
{ 

    public int Id { get; set; }

    public int Breedertrainingtypeid { get; set; }

    public int Horseid { get; set; }

    public int Userid { get; set; }

    public string? description { get; set; }

    public DateTime Startdate { get; set; }

    public DateTime? Enddate { get; set; }

    public string? Status { get; set; }

    public decimal? Cost { get; set; }

    [NotMapped]
    public string RealTrainingStatus { get; set; } //отражает статус заявки более детально, учитывая те которых нет в бд

    public bool? IsNotificationSent { get; set; }

    [NotMapped]
    public string StatusColor { get; set; } = "Black";

    [NotMapped]
    public bool IsCancelButtonVisible { get; set; }

    public virtual BreederTrainingType Breedertrainingtype { get; set; } = null!;

    public virtual Horse Horse { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public void UpdateRealStatus()
    {
        var now = DateTime.Now;

        if (string.IsNullOrEmpty(Status))
        {
            RealTrainingStatus = "Неизвестно";
            StatusColor = "Gray";
            IsCancelButtonVisible = false;
            return;
        }

        switch (Status)
        {
            case "Отклонена":
                RealTrainingStatus = "Отклонена";
                StatusColor = "Red";
                IsCancelButtonVisible = false;
                break;

            case "Не принята":
                RealTrainingStatus = "Не принята";
                StatusColor = "#fcba03";
                IsCancelButtonVisible = true;
                break;

            case "Принята":
                if (now < Startdate)
                {
                    RealTrainingStatus = "Принята";
                    StatusColor = "#eb7f1f";
                    IsCancelButtonVisible = false;
                }
                if (Enddate.HasValue)
                {
                    if (now >= Enddate.Value)
                    {
                        RealTrainingStatus = "Выполнена";
                        StatusColor = "#47A76A"; // Чёрный для выполнена
                        IsCancelButtonVisible = false;
                    }
                    else if (now >= Startdate && now <= Enddate.Value)
                    {
                        RealTrainingStatus = "В процессе";
                        StatusColor = "Blue";
                        IsCancelButtonVisible = false;
                    }
                    else
                    {
                        RealTrainingStatus = "Принята";
                        StatusColor = "#47A76A";
                        IsCancelButtonVisible = false;
                    }
                }
                else
                {
                    if (now >= Startdate)
                    {
                        RealTrainingStatus = "В процессе";
                        StatusColor = "Blue";
                        IsCancelButtonVisible = false;
                    }
                    else
                    {
                        RealTrainingStatus = "Принята";
                        StatusColor = "#47A76A";
                        IsCancelButtonVisible = false;
                    }
                }
                break;

            default:
                RealTrainingStatus = Status;
                StatusColor = "Black";
                IsCancelButtonVisible = false;
                break;
        }
    }
}
