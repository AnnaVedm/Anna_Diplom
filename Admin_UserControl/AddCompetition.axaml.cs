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

public partial class AddCompetition : Window
{
    private string _photoPath;

    public AddCompetition()
    {
        InitializeComponent();
    }

    private async void AddPhoto_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Выберите фото лошади",
            Filters = { new FileDialogFilter { Name = "Images", Extensions = { "jpg", "jpeg", "png" } } },
            AllowMultiple = false
        };

        var result = await dialog.ShowAsync(this);
        if (result != null && result.Length > 0)
        {
            _photoPath = result[0];
            CompImage.Source = new Bitmap(_photoPath);
        }
    }

    private async void AddCompetitionButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(compNameTextBox.Text))
        {
            await new CustomMessageBox("Наименование соревнования не может быть пустым", false).ShowDialog(this);
            return;
        }

        if (string.IsNullOrEmpty(compTypeTextBox.Text))
        {
            await new CustomMessageBox("Тип соревнования не может быть пустым", false).ShowDialog(this);
            return;
        }

        if (string.IsNullOrEmpty(routeTextBox.Text))
        {
            await new CustomMessageBox("Маршрут не может быть пустым", false).ShowDialog(this);
            return;
        }

        if (string.IsNullOrEmpty(entryTextBox.Text))
        {
            await new CustomMessageBox("Взнос не может быть пустым", false).ShowDialog(this);
            return;
        }

        if (string.IsNullOrEmpty(compDateTextBox.Text))
        {
            await new CustomMessageBox("Дата не может быть пустой", false).ShowDialog(this);
            return;
        }

        if (!decimal.TryParse(entryTextBox.Text, out decimal amount))
        {
            await new CustomMessageBox("Введите корректное число!", false).ShowDialog(this);
            return;
        }

        if (amount <= 0)
        {
            await new CustomMessageBox("Сумма должна быть больше 0!", false).ShowDialog(this);
            return;
        }

        if (!DateTime.TryParse(compDateTextBox.Text, out var compDate))
        {
            await new CustomMessageBox("Некорректный формат даты рождения (используйте дд.мм.гггг)").ShowDialog(this);
            return;
        }

        try
        {
            // Сохранение фото
            string photoName;
            if (!string.IsNullOrEmpty(_photoPath))
            {
                // Генерируем уникальное имя для файла
                photoName = $"{Guid.NewGuid()}{Path.GetExtension(_photoPath)}";
                var imagesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images");

                // Создаем папку если ее нет
                if (!Directory.Exists(imagesPath))
                    Directory.CreateDirectory(imagesPath);

                // Копируем файл
                File.Copy(_photoPath, Path.Combine(imagesPath, photoName), true);
            }
            else
            {
                // Используем фото по умолчанию
                photoName = "tatu.png";
            }

            using (var context = new DiplomHorseClubContext())
            {
                //далее сохраняем само соревнование
                var comp = new Competition
                {
                    Id = context.Competitions.Any() ? context.Competitions.Max(a => a.Id) + 1 : 1,
                    Name = compNameTextBox.Text,
                    Competitiontype = compTypeTextBox.Text,
                    Route = routeTextBox.Text,
                    Entryfee = amount.ToString(),
                    Date = DateOnly.FromDateTime(compDate),
                    Photocomp = photoName
                };

                context.Competitions.Add(comp);
                await context.SaveChangesAsync();

                await new CustomMessageBox("Соревнование успешно добавлено!").ShowDialog(this);

                this.Close(comp);
            }
        }
        catch(Exception ex)
        {
            await new CustomMessageBox($"{ex.Message}").ShowDialog(this);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close(null);
    }
}