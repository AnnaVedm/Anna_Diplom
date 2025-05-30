using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using System.Collections.ObjectModel;
using System.Linq;
using TyutyunnikovaAnna_Diplom.Context;
using TyutyunnikovaAnna_Diplom.Models;

namespace TyutyunnikovaAnna_Diplom;

public partial class EditStables : UserControl
{
    //��� ����� ����� ���� �������� � ������������ ������������� ���������� � ���������
    public ObservableCollection<Stabletype> Stables_spisok { get; set; } = new ObservableCollection<Stabletype>();
    public EditStables()
    {
        InitializeComponent();
        LoadStables_spisok();
        DataContext = this;
    }

    private void LoadStables_spisok()
    {
        using (var context = new DiplomHorseClubContext())
        {
            var stables = context.Stabletypes.OrderBy(a => a.Id).ToList();

            Stables_spisok.Clear();
            foreach (var item in stables)
            {
                Stables_spisok.Add(item);
            }
        }
    }

    //����������� ������ - � + ��� ��������� ���-�� ��������� ��������
    //�������� ���������� ����� �� ����������� ui ����� ��� ������ ��������� � ����������� ������ � ��
    private async void PlusButton_Click(object? sender, RoutedEventArgs e)
    {
        using (var context = new DiplomHorseClubContext())
        {
            if (sender is Button btn && btn.DataContext is Stabletype stable)
            {
                stable.StablesArendable = (stable.StablesArendable ?? 0) + 1;
                context.Update(stable);
                await context.SaveChangesAsync();
            }
        }
    }

    //�������� ���������� ����� �� ����������� ui ����� ��� ������ ��������� � ����������� ������ � ��
    private async void MinusButton_Click(object? sender, RoutedEventArgs e)
    {
        using (var context = new DiplomHorseClubContext())
        {
            if (sender is Button btn && btn.DataContext is Stabletype stable)
            {
                if ((stable.StablesArendable ?? 0) > 0)
                {
                    stable.StablesArendable = stable.StablesArendable - 1;
                    context.Update(stable);
                    await context.SaveChangesAsync();
                }    
            }
        }
    }

    private void EnableEditButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is Stabletype stable)
        {
            stable.IsStableEditable = true;
        }
    }

    private async void ApplyStableEditButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is Stabletype stable)
        {
            using (var context = new DiplomHorseClubContext())
            {
                // �������� �������� �� ���� �� ����� (Id)
                var stableUpdate = context.Stabletypes.Find(stable.Id);

                if (stableUpdate != null)
                {
                    // ���������, ��� Costoverride - �����
                    if (decimal.TryParse(stable.Cost.ToString(), out decimal cost))
                    {
                        stableUpdate.Cost = (int?)cost;

                        context.Stabletypes.Update(stableUpdate); // ��������� ���������
                        //context.BreederTrainingTypes.Update(training); // ��������� ���������
                        context.SaveChanges();

                        //������������� ������
                        LoadStables_spisok();

                        var message = new CustomMessageBox($"��������� ������� {stable.Name} ������� ��������!", false); ;
                        await message.ShowDialog(this.GetVisualRoot() as AdminMenu);

                    }
                    else
                    {
                        // ���� �� ������� �������������, �������� �� ������
                        var message = new CustomMessageBox("�������� ������ ���������. ������� �����.", false);
                        await message.ShowDialog(this.GetVisualRoot() as AdminMenu);
                    }
                }
            }

            // ��������� ����� ��������������
            stable.IsStableEditable = false;
        }
    }
}