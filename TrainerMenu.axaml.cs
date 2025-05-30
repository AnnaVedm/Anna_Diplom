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
        // ��������� �������� �� ������� ��������
        this.Loaded += NavBar_Loaded;
        // ������������� �������� ������
        DataContext = this;
        // ��������� ������ ������������
        LoadUserData();
        // �������� ��������� UserControl
        ChooseUserControl(); // ��������� ����� ����� ������
    }

    private User _user; // ������� �������������� ������������
    private bool _isNavigationInProgress = false; // ���� ��� �������������� ��������

    public enum NavButtonIndex
    {
        Home = 0,
        Zayavki = 1,// ������� �������� �������
        ClientProfile = 2
       
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

    private void ClientProfileToggleButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SetActiveNavigationButton((int)NavButtonIndex.ClientProfile);
    }

    // ����� ��� ����������� �� ��������� �������
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
        if (UserForAuthorization.SelectedUser.Roleid == 2)
        {
            TrainMenu trainMenu = new TrainMenu();
            trainMenu.trainerMenu = this; // ������������� ������ �� ������� ����
            Control.Content = trainMenu;
            SetActiveNavigationButton((int)NavButtonIndex.Home);
        }
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



    // ���������� ������� Checked ��� ������ ���������
    private void ToggleButton_Checked(object? sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton checkedBtn)
        {
            // ���������� ��������������� ������
            SetActiveNavigationButton(navList.Children.IndexOf(checkedBtn));
        }
    }

    private void HomeToggleButton_Click(object? sender, RoutedEventArgs e)
    {
        SetActiveNavigationButton((int)NavButtonIndex.Home);
    }

    // ���������� ������� ��������
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