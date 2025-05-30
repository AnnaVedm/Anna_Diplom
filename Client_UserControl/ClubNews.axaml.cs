using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using ExCSS;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using TyutyunnikovaAnna_Diplom.Context;
using TyutyunnikovaAnna_Diplom.Models;

namespace TyutyunnikovaAnna_Diplom
{

    public partial class ClubNews : UserControl, INotifyPropertyChanged
    {
        private Clubnews _currentNews;
        public Clubnews CurrentNews
        {
            get => _currentNews;
            set
            {
                _currentNews = value;
                OnPropertyChanged(nameof(CurrentNews));
            }
        }

        public ClubNews()
        {
            InitializeComponent();
            DataContext = this;
            LoadLatestNews(); // ��������� ��������� ������� ��� �������������
        }

        // ����� ��� �������� ���������� ������� �� ID
        public void LoadNews(int newsId)
        {
            using (var context = new DiplomHorseClubContext())
            {
                CurrentNews = context.Clubnews.FirstOrDefault(n => n.Id == newsId);
            }
        }

        // ����� ��� �������� ��������� �������
        public void LoadLatestNews()
        {
            using (var context = new DiplomHorseClubContext())
            {
                CurrentNews = context.Clubnews
                    .OrderByDescending(n => n.Date)
                    .FirstOrDefault();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}