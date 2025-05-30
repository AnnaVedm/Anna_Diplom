using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TyutyunnikovaAnna_Diplom;

public partial class CustomMessageBox : Window
{
    public CustomMessageBox(string message, bool isVisible = false)
    {
        InitializeComponent();
        Otmena.IsVisible = isVisible;
        MessageTextBlock.Text = message;
    }

    public CustomMessageBox()
    {
        InitializeComponent();
    }

    private void OKButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close(true);
    }

    private void OtmenaButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.Close(false);
    }
}
