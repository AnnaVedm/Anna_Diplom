using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Linq;
using TyutyunnikovaAnna_Diplom.AccountData;
using TyutyunnikovaAnna_Diplom.Context;
using TyutyunnikovaAnna_Diplom.Models;

namespace TyutyunnikovaAnna_Diplom;

public partial class TrainerMenu : Window, INotifyPropertyChanged
{
    public TrainerMenu()
    {
        InitializeComponent();
        // Добавляем подписку на событие загрузки
        this.Loaded += NavBar_Loaded;
        // Устанавливаем контекст данных
        DataContext = this;
        // Загружаем данные пользователя
        LoadUserData();
        // Выбираем начальный UserControl
        ChooseUserControl(); // Добавляем вызов этого метода
    }

    private User _user; // Текущий авторизованный пользователь
    private bool _isNavigationInProgress = false; // Флаг для предотвращения рекурсии

    public enum NavButtonIndex
    {
        Home = 0,
        Zayavki = 1,// Главная страница тренера
        ClientProfile = 2
       
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

    private void ClientProfileToggleButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SetActiveNavigationButton((int)NavButtonIndex.ClientProfile);
    }

    // Метод для уведомления об изменении свойств
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
        if (UserForAuthorization.SelectedUser.Roleid == 2)
        {
            TrainMenu trainMenu = new TrainMenu();
            trainMenu.trainerMenu = this; // Устанавливаем ссылку на текущее окно
            Control.Content = trainMenu;
            SetActiveNavigationButton((int)NavButtonIndex.Home);
        }
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
                            Control.Content = new TrainMenu();
                            break;
                        case NavButtonIndex.ClientProfile:
                            Control.Content = new ClientProfile();
                            break;
                        case NavButtonIndex.Zayavki:
                            Control.Content = new ZayvkiTrainer();
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



    // Обработчик события Checked для кнопок навигации
    private void ToggleButton_Checked(object? sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton checkedBtn)
        {
            // Активируем соответствующую кнопку
            SetActiveNavigationButton(navList.Children.IndexOf(checkedBtn));
        }
    }

    private void HomeToggleButton_Click(object? sender, RoutedEventArgs e)
    {
        SetActiveNavigationButton((int)NavButtonIndex.Home);
    }

    // Обновление баланса кошелька
    private async void UpdateWalletBalanceButton_Click(object? sender, RoutedEventArgs e)
    {
        var walletDialog = new WalletBalanceUpdate();
        walletDialog.OwnerTrainerMenu = this;
        if (await walletDialog.ShowDialog<bool>(this))
        {
            LoadUserData();
        }
    }

    private void ExitButton_Click(object? sender, RoutedEventArgs e)
    {
        UserForAuthorization.SelectedUser = null;
        new MainWindow().Show();
        this.Close();
    }

    private void ZayavkiToggleButton_Checked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SetActiveNavigationButton((int)NavButtonIndex.Zayavki);
    }
}