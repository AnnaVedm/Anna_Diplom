using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using TyutyunnikovaAnna_Diplom.AccountData;

namespace TyutyunnikovaAnna_Diplom;
//

public partial class AdministrMenu : UserControl
{
    public string adminEmail => UserForAuthorization.SelectedUser.Email;

    public AdministrMenu()
    {
        InitializeComponent();

        DataContext = this;
    }

    private async void StablesAdmin_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (this.GetVisualRoot() is AdminMenu menuWindow)
        {
            menuWindow.SetActiveNavigationButton((int)AdminMenu.NavButtonIndex.Stables);
        }
    }

    private async void Competitions_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (this.GetVisualRoot() is AdminMenu menuWindow)
        {
            menuWindow.SetActiveNavigationButton((int)AdminMenu.NavButtonIndex.Competitions);
        }
    }

    private async void AdminNews_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (this.GetVisualRoot() is AdminMenu menuWindow)
        {
            //Меняем контент
            var addnews = new AddClubNews();
            await addnews.ShowDialog(menuWindow);
        }
    }

    private async void BreederAdmin_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (this.GetVisualRoot() is AdminMenu menuWindow)
        {
            var adminmenu = new AddBreeder();
            await adminmenu.ShowDialog(menuWindow);
        }
    }
}