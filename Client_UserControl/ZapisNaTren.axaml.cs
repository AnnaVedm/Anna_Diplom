using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TyutyunnikovaAnna_Diplom.AccountData;
using TyutyunnikovaAnna_Diplom.Context;
using TyutyunnikovaAnna_Diplom.Models;

namespace TyutyunnikovaAnna_Diplom;

public partial class ZapisNaTren : Window, INotifyPropertyChanged
{
    int selectedHorse;
    public int Breedertrainingtypeid { get; set; } // Сохраняем оригинальное название
    public User Current_breeder { get; set; }

    public Menu OwnerMenu { get; set; }

    public ObservableCollection<UserHorse> Horses_spisok { get; set; } = new ObservableCollection<UserHorse>();

    // Добавьте это для поддержки уведомлений об изменении свойств
    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private decimal _costoverriding;
    public decimal Costoverriding
    {
        get => _costoverriding;
        set
        {
            _costoverriding = value;
            // Уведомляем об изменении свойства
            OnPropertyChanged(nameof(Costoverriding));
        }
    }

    private int _duration;
    public int Duration
    {
        get => _duration;
        set
        {
            _duration = value;
            OnPropertyChanged(nameof(Duration));
        }
    }

    private string _uslugaName;
    public string UslugaName
    {
        get => _uslugaName;
        set
        {
            _uslugaName = value;
            OnPropertyChanged(nameof(UslugaName));
        }
    }

    public ZapisNaTren()
    {
        InitializeComponent();
        DataContext = this;
        InitializeComponent();
        DataContext = this;
        TrainDatePicker.MinYear = DateTime.Today; // Установка минимальной даты
        Load_Horse();
    }

    //public void Load_Duration()
    //{
    //    using(var context = new DiplomHorseClubContext())
    //    {
    //        var trainingType = context.TrainingTypes
    //                .FirstOrDefault(tt => tt.Id == Breedertrainingtypeid);

    //        if (trainingType != null)
    //        {
    //            Duration = (int)trainingType.Duration;
    //        }
    //    }
    //}

    public void Load_Horse()
    {
        using(var context = new DiplomHorseClubContext())
        {
            var horse = context.UserHorses.Include(a => a.Horse).Where(a => a.Userid == UserForAuthorization.SelectedUser.Id).ToList();

            Horses_spisok.Clear();

            foreach (var s in horse)
            {
                Horses_spisok.Add(s);
            }
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void HorseComboBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        using(var context = new DiplomHorseClubContext())
        {
            ComboBox combobox = sender as ComboBox;
            if(combobox != null)
            {
                UserHorse item = combobox.SelectedItem as UserHorse;
                if (item != null)
                {
                    selectedHorse = (int)item.Horseid;
                }
            }
        }
    }

    private async void ZapisButton_Click(object? sender, RoutedEventArgs e)
    {
        // Проверка выбора лошади
        if (selectedHorse == 0)
        {
            ErrorTextBlock.Text = "Пожалуйста, выберите лошадь";
            return;
        }

        // Проверка выбора даты
        if (TrainDatePicker.SelectedDate == null || TrainTimePicker.SelectedTime == null)
        {
            ErrorTextBlock.Text = "Пожалуйста, выберите дату и время тренировки";
            return;
        }

        try
        {
            using (var context = new DiplomHorseClubContext())
            {
                // 1. Получаем кошелек пользователя
                var userWallet = context.Wallets
                    .FirstOrDefault(w => w.UserId == UserForAuthorization.SelectedUser.Id);

                if (userWallet == null)
                {
                    ErrorTextBlock.Text = "Кошелек пользователя не найден";
                    return;
                }

                // 2. Получаем информацию о типе тренировки
                var trainingType = context.TrainingTypes
                    .FirstOrDefault(tt => tt.Id == Breedertrainingtypeid);

                if (trainingType == null)
                {
                    ErrorTextBlock.Text = "Тип тренировки не найден";
                    return;
                }

                //Duration = (int)trainingType.Duration;

                // 3. Проверяем баланс
                if (userWallet.Summ < Costoverriding)
                {
                    ErrorTextBlock.Text = $"Недостаточно средств. На вашем счете: {userWallet.Summ} руб.";
                    return;
                }

                // 4. Списание средств
                userWallet.Summ -= Costoverriding;
                context.Wallets.Update(userWallet);

                context.SaveChanges(); //сохранили изменения в бд перед обновлением UI

                //Обновить интерфейс через LoadUserData класса Menu
                OwnerMenu.LoadUserData();

                var startDate = TrainDatePicker.SelectedDate.Value.Date + TrainTimePicker.SelectedTime.Value;

                // 5. Создаем запись о тренировке
                var newTraining = new BreederTraining
                {
                    Id = context.BreederTrainings.Any() ? context.BreederTrainings.Max(a => a.Id) + 1 : 1,
                    Breedertrainingtypeid = Breedertrainingtypeid,
                    Horseid = selectedHorse,
                    Userid = UserForAuthorization.SelectedUser.Id,
                    Startdate = startDate,
                    Enddate = startDate.AddMinutes((double)trainingType.Duration),
                    Status = "Не принята",
                    description = descriptTextBox.Text,
                    Cost = Costoverriding,
                    IsNotificationSent = false
                };

                context.BreederTrainings.Add(newTraining);

                // 6. Сохраняем изменения
                context.SaveChanges();

                var messageBox = new CustomMessageBox("Заявка на тренировку успешно сформирована!");

                await messageBox.ShowDialog(OwnerMenu);

                // Успешное завершение
                this.Close(true);
            }
        }
        catch (DbUpdateException dbEx)
        {
            ErrorTextBlock.Text = "Ошибка базы данных: " + dbEx.InnerException?.Message;
        }
        catch (Exception ex)
        {
            ErrorTextBlock.Text = "Ошибка: " + ex.Message;
        }
    }
}