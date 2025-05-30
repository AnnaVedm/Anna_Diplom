using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TyutyunnikovaAnna_Diplom.AccountData;
using TyutyunnikovaAnna_Diplom.Context;
using TyutyunnikovaAnna_Diplom.Models;

namespace TyutyunnikovaAnna_Diplom;

public partial class AddLoginForGoogle : Window
{
    private User User { get; set; }
    public AddLoginForGoogle()
    {
        InitializeComponent();
    }
    public AddLoginForGoogle(User user)
    {
        User = user;
        InitializeComponent();
    }

    private void BackButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.Close();
    }

    private async void ConfirmButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        using (var context = new DiplomHorseClubContext())
        {
            if (LoginTextBox.Text == null)
            {
                CustomMessageBox messageBox = new CustomMessageBox("Вы не придумали логин!");
                await messageBox.ShowDialog(this);
            }
            else
            {
                User.Login = LoginTextBox.Text;

                UserForAuthorization.SelectedUser = User;

                context.Users.Update(User);
                context.SaveChanges();

                Menu menu = new Menu();
                menu.Show();
                this.Close(true);
            }
         
        }

    }
}