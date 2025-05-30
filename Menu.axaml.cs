using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TyutyunnikovaAnna_Diplom.AccountData;
using TyutyunnikovaAnna_Diplom.Context;
using TyutyunnikovaAnna_Diplom.Models;

namespace TyutyunnikovaAnna_Diplom;

public partial class Menu : Window, INotifyPropertyChanged
{
    // Перечисление для идентификации кнопок навигации
    public enum NavButtonIndex
    {
        Home = 0,        // Главная страница
        MyHorses = 1,    // Мои лошади
        ClientProfile = 2 
    }

    private User _user; // Текущий авторизованный пользователь
    private bool _isNavigationInProgress = false; // Флаг для предотвращения рекурсии

    private DispatcherTimer _timer; // Объявляем таймер для отслеживания заявок
    public Menu()
    {
        InitializeComponent();
        // Подписываемся на событие загрузки окна
        this.Loaded += NavBar_Loaded;
        // Устанавливаем контекст данных
        DataContext = this;
        // Загружаем данные пользователя
        LoadUserData();
        // Выбираем начальный UserControl в зависимости от роли
        ChooseUserControl();

        // Инициализация таймера
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(1000)
        };
        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

    private bool _isDialogOpen = false;

    private async void Timer_Tick(object sender, EventArgs e) //таймер, вызывающий метод проверки БД для вывода уведомлений каждую секунду
    {
        if (_isDialogOpen)
            return; // Если диалог открыт - пропускаем проверку

        await CheckZayavkiAsync();
    }

    private async Task CheckZayavkiAsync() //метод для отслеживания отклоненных заявок пользователя
    {
        // Получаем заявку в фоновом потоке
        var data = await Task.Run(() =>
        {
            using var context = new DiplomHorseClubContext();
            return context.BreederTrainings
                .Include(h => h.Horse)
                .Include(u => u.User)
                .FirstOrDefault(a => a.Userid == UserForAuthorization.SelectedUser.Id &&
                                     a.IsNotificationSent == false &&
                                     a.Status == "Отклонена");
        });

        if (data == null)
            return;

        _isDialogOpen = true; // Блокируем повторное открытие окна через 200мс

        var message = $"Тренировка лошади {data.Horse.HorseName} " +
                      $"с тренером {data.User.Name} {data.User.Surname} " +
                      $"{data.Startdate:dd.MM.yyyy} отклонена";

        var messageBox = new CustomMessageBox(message, false);
        await messageBox.ShowDialog(this);

        // После закрытия диалога обновляем запись
        await Task.Run(() =>
        {
            using var context = new DiplomHorseClubContext();
            var entity = context.BreederTrainings.Find(data.Id);
            if (entity != null)
            {
                entity.IsNotificationSent = true;
                context.SaveChanges();
            }
        });

        _isDialogOpen = false; // Разблокируем проверку
    }


    // Событие для уведомления об изменениях свойств (для привязок данных)
    public event PropertyChangedEventHandler? PropertyChanged;

    // Свойство пользователя с уведомлением об изменении
    public User User
    {
        get => _user;
        set
        {
            _user = value;
            // Уведомляем об изменении пользователя и баланса
            OnPropertyChanged(nameof(User));
            OnPropertyChanged(nameof(UserBalance));
        }
    }

    // Вычисляемое свойство баланса пользователя
    public decimal? UserBalance => User?.Wallets.FirstOrDefault()?.Summ;

    // Загрузка данных пользователя из базы данных
    public void LoadUserData()
    {
        using (var context = new DiplomHorseClubContext())
        {
            // Получаем пользователя с его кошельком
            User = context.Users
                .Include(a => a.Wallets)
                .FirstOrDefault(a => a.Id == UserForAuthorization.SelectedUser.Id);
        }
    }

    // Выбор начального UserControl в зависимости от роли пользователя
    private void ChooseUserControl()
    {
        // Для клиентов (roleid = 3) показываем ClientMenu
        if (UserForAuthorization.SelectedUser.Roleid == 3)
        {
            Control.Content = new ClientMenu();
            // Активируем кнопку "Главная"
            SetActiveNavigationButton((int)NavButtonIndex.Home);
        }
    }

    // Метод для активации кнопки навигации
    public void SetActiveNavigationButton(int index, bool updateContent = true)
    {
        // Если навигация уже в процессе - выходим
        if (_isNavigationInProgress) return;
        _isNavigationInProgress = true;

        try
        {
            navPointer.IsVisible = true;
            // Сбрасываем все кнопки кроме выбранной
            foreach (var child in navList.Children)
            {
                if (child is ToggleButton toggleButton && toggleButton != navList.Children[index])
                {
                    toggleButton.IsChecked = false;
                    toggleButton.IsEnabled = true;
                }
            }

            // Активируем выбранную кнопку
            if (index >= 0 && index < navList.Children.Count &&
                navList.Children[index] is ToggleButton activeButton)
            {
                activeButton.IsChecked = true;
                activeButton.IsEnabled = false;
                // Обновляем позицию индикатора
                UpdateNavigationPointer(activeButton);

                // При необходимости обновляем контент
                if (updateContent)
                {
                    switch ((NavButtonIndex)index)
                    {
                        case NavButtonIndex.Home:
                            Control.Content = new ClientMenu();
                            break;
                        case NavButtonIndex.MyHorses:
                            var myHorses = new MyHorses();
                            myHorses.OwnerMenu = this;
                            Control.Content = myHorses;
                            break;
                        case NavButtonIndex.ClientProfile:
                            Control.Content = new ClientProfile();
                            break;
                    }
                }
            }
        }
        finally
        {
            // Снимаем блокировку навигации
            _isNavigationInProgress = false;
        }
    }

    // Модифицированный метод для скрытия индикатора
    public void HideNavigationPointer()
    {
        navPointer.IsVisible = false;

        // Сбрасываем все кнопки навигации
        foreach (var child in navList.Children)
        {
            if (child is ToggleButton toggleButton)
            {
                toggleButton.IsChecked = false;
                toggleButton.IsEnabled = true;
            }
        }
    }

    // Обновление позиции индикатора навигации
    private void UpdateNavigationPointer(ToggleButton activeButton)
    {
        // Вычисляем позицию по центру кнопки
        double pointerPos = activeButton.Bounds.Left + (activeButton.Bounds.Width / 2) - 3;
        navPointer.Margin = new Thickness(pointerPos, 0, 0, 0);
        navPointer.IsVisible = true;
    }

    // Обработчик загрузки навигационной панели
    private void NavBar_Loaded(object? sender, RoutedEventArgs e)
    {
        // При загрузке устанавливаем индикатор на первую кнопку
        if (navList.Children.Count > 0 && navList.Children[0] is ToggleButton homeBtn)
        {
            UpdateNavigationPointer(homeBtn);
        }
    }

    // Обработчик события Checked для кнопок навигации
    private void ToggleButton_Checked(object? sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton checkedBtn)
        {
            // Активируем соответствующую кнопку
            SetActiveNavigationButton(navList.Children.IndexOf(checkedBtn));
        }
    }

    // Обработчики кликов для конкретных кнопок
    private void HomeToggleButton_Click(object? sender, RoutedEventArgs e)
    {
        SetActiveNavigationButton((int)NavButtonIndex.Home);
    }

    private void MyHorsesToggleButton_Click(object? sender, RoutedEventArgs e)
    {
        SetActiveNavigationButton((int)NavButtonIndex.MyHorses);
    }

    private void ClientProfileToggleButton_Click(object? sender, RoutedEventArgs e)
    {
        SetActiveNavigationButton((int)NavButtonIndex.ClientProfile);
    }




    //метод проверки данных БД для вывода уведомления
    private async void CheckZayavki()
    {
        using (var context = new DiplomHorseClubContext())
        {
            var userBreederTraining = context.BreederTrainings
                .Include(h => h.Horse)
                .Include(u => u.User)
                .FirstOrDefault(a => a.Userid == UserForAuthorization.SelectedUser.Id &&
                a.IsNotificationSent == false &&
                a.Status == "Отклонено");      

            if (userBreederTraining != null)
            {
                var message = 
                    $"Тренировка лошади {userBreederTraining.Horse.HorseName}" +
                    $" с тренером {userBreederTraining.User.Name} {userBreederTraining.User.Surname} {userBreederTraining.Startdate} отклонена";
                
                var messageBox = new CustomMessageBox(message, false);
                await messageBox.ShowDialog(this);

                userBreederTraining.IsNotificationSent = true;

                context.BreederTrainings.Update(userBreederTraining);
                context.SaveChanges();
            }
        }
    }

    //private void TrainerToggleButton_Click(object? sender, RoutedEventArgs e)
    //{
    //    SetActiveNavigationButton((int)NavButtonIndex.TrainerServices);
    //}

    // Выход из аккаунта
    private void ExitButton_Click(object? sender, RoutedEventArgs e)
    {
        _timer?.Stop(); //останавливаем таймер
        
        UserForAuthorization.SelectedUser = null;
        new MainWindow().Show();
        this.Close();
    }

    // Обновление баланса кошелька
    private async void UpdateWalletBalanceButton_Click(object? sender, RoutedEventArgs e)
    {
        var walletDialog = new WalletBalanceUpdate();
        walletDialog.OwnerMenu = this;
        if (await walletDialog.ShowDialog<bool>(this))
        {
            LoadUserData();
        }
    }

    // Метод для уведомления об изменении свойств
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}