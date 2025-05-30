using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System.Linq;
using TyutyunnikovaAnna_Diplom.AccountData;
using TyutyunnikovaAnna_Diplom.Context;

namespace TyutyunnikovaAnna_Diplom
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                //desktop.MainWindow = new MainWindow();

                //------------��� ������������---------------------------------:
                auth();
                desktop.MainWindow = new MainWindow();
                //----------�� ��������� ����������������� ���� ������----------
            }

            base.OnFrameworkInitializationCompleted();
        }

        //����� ��� ������������ - ������� ���� � �������
        private void auth()
        {
            using (var context = new DiplomHorseClubContext())
            {
                //UserForAuthorization.SelectedUser = context.Users.FirstOrDefault(u => u.Login == "Lada");

                UserForAuthorization.SelectedUser = context.Users.FirstOrDefault(u => u.Login == "Lada");
            }
            
        }
    }
}