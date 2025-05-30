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
    // ������������ ��� ������������� ������ ���������
    public enum NavButtonIndex
    {
        Home = 0,        // ������� ��������
        MyHorses = 1,    // ��� ������
        ClientProfile = 2 
    }

    private User _user; // ������� �������������� ������������
    private bool _isNavigationInProgress = false; // ���� ��� �������������� ��������

    private DispatcherTimer _timer; // ��������� ������ ��� ������������ ������
    public Menu()
    {
        InitializeComponent();
        // ������������� �� ������� �������� ����
        this.Loaded += NavBar_Loaded;
        // ������������� �������� ������
        DataContext = this;
        // ��������� ������ ������������
        LoadUserData();
        // �������� ��������� UserControl � ����������� �� ����
        ChooseUserControl();

        // ������������� �������
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(1000)
        };
        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

    private bool _isDialogOpen = false;

    private async void Timer_Tick(object sender, EventArgs e) //������, ���������� ����� �������� �� ��� ������ ����������� ������ �������
    {
        if (_isDialogOpen)
            return; // ���� ������ ������ - ���������� ��������

        await CheckZayavkiAsync();
    }

    private async Task CheckZayavkiAsync() //����� ��� ������������ ����������� ������ ������������
    {
        // �������� ������ � ������� ������
        var data = await Task.Run(() =>
        {
            using var context = new DiplomHorseClubContext();
            return context.BreederTrainings
                .Include(h => h.Horse)
                .Include(u => u.User)
                .FirstOrDefault(a => a.Userid == UserForAuthorization.SelectedUser.Id &&
                                     a.IsNotificationSent == false &&
                                     a.Status == "���������");
        });

        if (data == null)
            return;

        _isDialogOpen = true; // ��������� ��������� �������� ���� ����� 200��

        var message = $"���������� ������ {data.Horse.HorseName} " +
                      $"� �������� {data.User.Name} {data.User.Surname} " +
                      $"{data.Startdate:dd.MM.yyyy} ���������";

        var messageBox = new CustomMessageBox(message, false);
        await messageBox.ShowDialog(this);

        // ����� �������� ������� ��������� ������
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

        _isDialogOpen = false; // ������������ ��������
    }


    // ������� ��� ����������� �� ���������� ������� (��� �������� ������)
    public event PropertyChangedEventHandler? PropertyChanged;

    // �������� ������������ � ������������ �� ���������
    public User User
    {
        get => _user;
        set
        {
            _user = value;
            // ���������� �� ��������� ������������ � �������
            OnPropertyChanged(nameof(User));
            OnPropertyChanged(nameof(UserBalance));
        }
    }

    // ����������� �������� ������� ������������
    public decimal? UserBalance => User?.Wallets.FirstOrDefault()?.Summ;

    // �������� ������ ������������ �� ���� ������
    public void LoadUserData()
    {
        using (var context = new DiplomHorseClubContext())
        {
            // �������� ������������ � ��� ���������
            User = context.Users
                .Include(a => a.Wallets)
                .FirstOrDefault(a => a.Id == UserForAuthorization.SelectedUser.Id);
        }
    }

    // ����� ���������� UserControl � ����������� �� ���� ������������
    private void ChooseUserControl()
    {
        // ��� �������� (roleid = 3) ���������� ClientMenu
        if (UserForAuthorization.SelectedUser.Roleid == 3)
        {
            Control.Content = new ClientMenu();
            // ���������� ������ "�������"
            SetActiveNavigationButton((int)NavButtonIndex.Home);
        }
    }

    // ����� ��� ��������� ������ ���������
    public void SetActiveNavigationButton(int index, bool updateContent = true)
    {
        // ���� ��������� ��� � �������� - �������
        if (_isNavigationInProgress) return;
        _isNavigationInProgress = true;

        try
        {
            navPointer.IsVisible = true;
            // ���������� ��� ������ ����� ���������
            foreach (var child in navList.Children)
            {
                if (child is ToggleButton toggleButton && toggleButton != navList.Children[index])
                {
                    toggleButton.IsChecked = false;
                    toggleButton.IsEnabled = true;
                }
            }

            // ���������� ��������� ������
            if (index >= 0 && index < navList.Children.Count &&
                navList.Children[index] is ToggleButton activeButton)
            {
                activeButton.IsChecked = true;
                activeButton.IsEnabled = false;
                // ��������� ������� ����������
                UpdateNavigationPointer(activeButton);

                // ��� ������������� ��������� �������
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
            // ������� ���������� ���������
            _isNavigationInProgress = false;
        }
    }

    // ���������������� ����� ��� ������� ����������
    public void HideNavigationPointer()
    {
        navPointer.IsVisible = false;

        // ���������� ��� ������ ���������
        foreach (var child in navList.Children)
        {
            if (child is ToggleButton toggleButton)
            {
                toggleButton.IsChecked = false;
                toggleButton.IsEnabled = true;
            }
        }
    }

    // ���������� ������� ���������� ���������
    private void UpdateNavigationPointer(ToggleButton activeButton)
    {
        // ��������� ������� �� ������ ������
        double pointerPos = activeButton.Bounds.Left + (activeButton.Bounds.Width / 2) - 3;
        navPointer.Margin = new Thickness(pointerPos, 0, 0, 0);
        navPointer.IsVisible = true;
    }

    // ���������� �������� ������������� ������
    private void NavBar_Loaded(object? sender, RoutedEventArgs e)
    {
        // ��� �������� ������������� ��������� �� ������ ������
        if (navList.Children.Count > 0 && navList.Children[0] is ToggleButton homeBtn)
        {
            UpdateNavigationPointer(homeBtn);
        }
    }

    // ���������� ������� Checked ��� ������ ���������
    private void ToggleButton_Checked(object? sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton checkedBtn)
        {
            // ���������� ��������������� ������
            SetActiveNavigationButton(navList.Children.IndexOf(checkedBtn));
        }
    }

    // ����������� ������ ��� ���������� ������
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




    //����� �������� ������ �� ��� ������ �����������
    private async void CheckZayavki()
    {
        using (var context = new DiplomHorseClubContext())
        {
            var userBreederTraining = context.BreederTrainings
                .Include(h => h.Horse)
                .Include(u => u.User)
                .FirstOrDefault(a => a.Userid == UserForAuthorization.SelectedUser.Id &&
                a.IsNotificationSent == false &&
                a.Status == "���������");      

            if (userBreederTraining != null)
            {
                var message = 
                    $"���������� ������ {userBreederTraining.Horse.HorseName}" +
                    $" � �������� {userBreederTraining.User.Name} {userBreederTraining.User.Surname} {userBreederTraining.Startdate} ���������";
                
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

    // ����� �� ��������
    private void ExitButton_Click(object? sender, RoutedEventArgs e)
    {
        _timer?.Stop(); //������������� ������
        
        UserForAuthorization.SelectedUser = null;
        new MainWindow().Show();
        this.Close();
    }

    // ���������� ������� ��������
    private async void UpdateWalletBalanceButton_Click(object? sender, RoutedEventArgs e)
    {
        var walletDialog = new WalletBalanceUpdate();
        walletDialog.OwnerMenu = this;
        if (await walletDialog.ShowDialog<bool>(this))
        {
            LoadUserData();
        }
    }

    // ����� ��� ����������� �� ��������� �������
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}