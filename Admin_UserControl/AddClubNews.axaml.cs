using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using System;
using System.IO;
using System.Linq;
using TyutyunnikovaAnna_Diplom.Context;
using TyutyunnikovaAnna_Diplom.Models;

namespace TyutyunnikovaAnna_Diplom;

public partial class AddClubNews : Window
{
    private string _photoPath;

    public AddClubNews()
    {
        InitializeComponent();
    }

    private async void AddPhoto_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "�������� ���� ������",
            Filters = { new FileDialogFilter { Name = "Images", Extensions = { "jpg", "jpeg", "png" } } },
            AllowMultiple = false
        };

        var result = await dialog.ShowAsync(this);
        if (result != null && result.Length > 0)
        {
            _photoPath = result[0];
            NewsImage.Source = new Bitmap(_photoPath);
        }
    }

    private async void AddNewsButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(newsNameTextBox.Text))
        {
            await new CustomMessageBox("��������� �� ����� ���� ������", false).ShowDialog(this);
            return;
        }

        if (string.IsNullOrEmpty(newsDescriptionTextBox.Text))
        {
            await new CustomMessageBox("�������� �� ����� ���� ������", false).ShowDialog(this);
            return;
        }

        try
        {
            // ���������� ����
            string photoName;
            if (!string.IsNullOrEmpty(_photoPath))
            {
                // ���������� ���������� ��� ��� �����
                photoName = $"{Guid.NewGuid()}{Path.GetExtension(_photoPath)}";
                var imagesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images");

                // ������� ����� ���� �� ���
                if (!Directory.Exists(imagesPath))
                    Directory.CreateDirectory(imagesPath);

                // �������� ����
                File.Copy(_photoPath, Path.Combine(imagesPath, photoName), true);
            }
            else
            {
                // ���������� ���� �� ��������� - ���������� ����� espada
                photoName = "c947323a-4eb6-419b-9a43-09007f18b192.jpg";
            }

            using (var context = new DiplomHorseClubContext())
            {
                var clubnews = context.Clubnews.ToList(); //���������� ������ ������� ��� ��������

                context.Clubnews.RemoveRange(clubnews);
                await context.SaveChangesAsync();

                var news = new Clubnews //��������������� ���� �������, ������ ��������� ������ ���������
                {
                    Id = context.Clubnews.Any() ? context.Clubnews.Max(a => a.Id) + 1 : 1,
                    Title = newsNameTextBox.Text,
                    Content = newsDescriptionTextBox.Text,
                    Author = "������������� Espada",
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Photo = photoName
                };

                context.Clubnews.Add(news);
                await context.SaveChangesAsync();

                await new CustomMessageBox("������� ������� ���������!", false).ShowDialog(this);

                this.Close();
            }
        }
        catch (Exception ex)
        {
            await new CustomMessageBox($"{ex.Message}", false).ShowDialog(this);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}