using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.ComponentModel;
using TyutyunnikovaAnna_Diplom.AccountData;
using TyutyunnikovaAnna_Diplom.Context;

namespace TyutyunnikovaAnna_Diplom;

public partial class AdminMenu : Window, INotifyPropertyChanged
{
    public enum NavButtonIndex
    {
        Home = 0,           // Главная страница
        Stables = 1,        // Денники
        Competitions = 2    //Соревнования
    }

    private bool _isNavigationInProgress = false; // Флаг для предотвращения рекурсии


    public AdminMenu()
    {
        InitializeComponent();

        this.Loaded += NavBar_Loaded;

        ChooseUserControl();
        DataContext = this;
    }

    private void ChooseUserControl()
    {
        // Для клиентов (roleid = 3) показываем ClientMenu
        if (UserForAuthorization.SelectedUser.Roleid == 1)
        {
            Control.Content = new AdministrMenu();
            // Активируем кнопку "Главная"
            SetActiveNavigationButton((int)NavButtonIndex.Home);
        }
    }

    private void UpdateNavigationPointer(ToggleButton activeButton)
    {
        // Вычисляем позицию по центру кнопки
        double pointerPos = activeButton.Bounds.Left + (activeButton.Bounds.Width / 2) - 3;
        navPointer.Margin = new Thickness(pointerPos, 0, 0, 0);
        navPointer.IsVisible = true;
    }

    private void NavBar_Loaded(object? sender, RoutedEventArgs e)
    {
        // При загрузке устанавливаем индикатор на первую кнопку
        if (navList.Children.Count > 0 && navList.Children[0] is ToggleButton homeBtn)
        {
            UpdateNavigationPointer(homeBtn);
        }
    }

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

    private void StablesToggleButton_Click(object? sender, RoutedEventArgs e)
    {
        SetActiveNavigationButton((int)NavButtonIndex.Stables);
    }

    private void CompetitionsToggleButton_Click(object? sender, RoutedEventArgs e)
    {
        SetActiveNavigationButton((int)NavButtonIndex.Competitions);
    }

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
                            Control.Content = new AdministrMenu();
                            break;
                        case NavButtonIndex.Stables:
                            var stables = new EditStables();
                            Control.Content = stables;
                            break;
                        case NavButtonIndex.Competitions:
                            Control.Content = new EditCompetitions();
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

    //будем прятать поинтер при переходе на страницу новостей
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


    private void ExitButton_Click(object sender, RoutedEventArgs e)
    {
        var authorization = new MainWindow();
        authorization.Show();
        this.Close();
    }

    // Событие для уведомления об изменениях свойств (для привязок данных)
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}