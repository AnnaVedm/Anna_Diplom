using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using System;
using System.IO;
using System.Linq;
using TyutyunnikovaAnna_Diplom.Models;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;
using TyutyunnikovaAnna_Diplom.Context;
using TyutyunnikovaAnna_Diplom.AccountData;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace TyutyunnikovaAnna_Diplom
{
    public partial class AddHorse : Window, INotifyPropertyChanged
    {

        private bool _isPaid = false;
        private bool _horseAdded = false;
        private int? _selectedStableId = null;
        private bool _isClosing = false;

        protected override async void OnClosing(WindowClosingEventArgs e)
        {
            if (_isClosing)
            {
                base.OnClosing(e);
                return;
            }

            // Проверяем, была ли оплата, и не добавлена ли лошадь
            if (_isPaid && !_horseAdded)
            {
                e.Cancel = true; // Блокируем закрытие, пока не обработаем

                var confirmDialog = new CustomMessageBox(
                    "Вы не добавили лошадь. Вернуть деньги на счет?",
                    true); // true — есть кнопка "Отмена"

                var result = await confirmDialog.ShowDialog<bool>(this);

                if (result) // Если согласился вернуть деньги
                {
                    await ReturnMoneyToWallet();
                    _isClosing = true;
                    Close(); // Закрываем окно после возврата
                }
                // Если нажал "Отмена" — окно остается открытым (e.Cancel = true)
            }
            else
            {
                // Если оплаты не было или лошадь добавлена — закрываем без вопросов
                base.OnClosing(e);
            }
        }

        private async void CloseWindow(object sender, RoutedEventArgs e)
        {
            if (_isPaid && !_horseAdded)
            {
                var confirmDialog = new CustomMessageBox(
                    "Вы не добавили лошадь. Вернуть деньги на счет?",
                    true);

                var result = await confirmDialog.ShowDialog<bool>(this);

                if (result) // Если согласился вернуть деньги
                {
                    await ReturnMoneyToWallet();
                    _isClosing = true;
                    Close();
                }
                // Если нажал "Отмена" — окно остается открытым
            }
            else
            {
                _isClosing = true;
                Close(); // Закрываем без вопросов, если нет оплаты
            }
        }

        private async Task ReturnMoneyToWallet()
        {
            try
            {
                using (var context = new DiplomHorseClubContext())
                {
                    var user = UserForAuthorization.SelectedUser;
                    var wallet = context.Wallets.FirstOrDefault(w => w.UserId == user.Id);

                    if (wallet != null && _isPaid)
                    {
                        wallet.Summ += _stablePrice;

                        if (_selectedStableId != null)
                        {
                            var stableType = context.Stabletypes.FirstOrDefault(st => st.Id == _selectedStableId);
                            if (stableType != null)
                            {
                                stableType.StablesArendable += 1;
                                context.Update(stableType);
                            }
                        }

                        await context.SaveChangesAsync();
                        OwnerMenu?.LoadUserData();
                        _isPaid = false;
                        _selectedStableId = null;
                    }
                }
            }
            catch (Exception ex)
            {
                await new CustomMessageBox($"Ошибка при возврате денег: {ex.Message}").ShowDialog(this);
            }
        }

        private ObservableCollection<string> _genders = new ObservableCollection<string>
        {
                    "Кобыла",
                    "Мерин",
                    "Жеребец"
        };

        private string _selectedGender;

        public ObservableCollection<string> Genders
        {
            get => _genders;
            set
            {
                _genders = value;
                OnPropertyChanged(nameof(Genders));
            }
        }

        public string SelectedGender
        {
            get => _selectedGender;
            set
            {
                _selectedGender = value;
                OnPropertyChanged(nameof(SelectedGender));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private readonly DiplomHorseClubContext _dbContext;
        
        private int _stablePrice;
        private string _photoPath;

        //private Stable _selectedStable;
        private Stabletype _selectedStable;

        public Menu OwnerMenu { get; set; } //ссылка на меню для обновления стоимости

        public AddHorse()
        {
            InitializeComponent();
            _dbContext = new DiplomHorseClubContext();
            LoadDefaultImage();
            DataContext = this;
        }

        //private void InitializeComponent()
        //{
        //    AvaloniaXamlLoader.Load(this);
        //}

        private void LoadDefaultImage()
        {
            var defaultImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "default_horse.png");
            if (File.Exists(defaultImagePath))
            {
                HorseImage.Source = new Bitmap(defaultImagePath);
            }
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
                HorseImage.Source = new Bitmap(_photoPath);
            }
        }

        private async void SelectStable_Click(object sender, RoutedEventArgs e)
        {
            var selectStableWindow = new SelectStable();

            var result = await selectStableWindow.ShowDialog<bool>(this);

            if (result && selectStableWindow.SelectedStable != null)
            {
                _selectedStable = selectStableWindow.SelectedStable;
                _stablePrice = selectStableWindow.StablePrice;

                //using (var context = new )

                // Показываем информацию о выбранном деннике
                SelectedStableInfo.IsVisible = true;
                StableCodeText.Text = $"Денник";
                StableTypeText.Text = $"Тип: {_selectedStable.Name}";
                StablePriceText.Text = $"Цена: {_stablePrice} руб./мес";

                // Показываем счет на оплату
                PaymentInfo.IsVisible = true;
                PaymentAmountText.Text = $"{_stablePrice} руб.";
                PaymentDescriptionText.Text = $"Оплата за аренду денника на 1 месяц";

                // Блокируем кнопку добавления до оплаты
                AddHorseButton.IsEnabled = false;
            }
            else
            {
                CustomMessageBox messageBox = new CustomMessageBox("Не выбран денник!");
                await messageBox.ShowDialog(this);
                return;
            }
        }

        private async void PayButton_Click(object sender, RoutedEventArgs e)
        {
            var user = UserForAuthorization.SelectedUser;
            var wallet = _dbContext.Wallets.FirstOrDefault(w => w.UserId == user.Id);

            if (wallet == null || wallet.Summ < _stablePrice)
            {
                var walletWindow = new WalletBalanceUpdate();
                walletWindow.OwnerMenu = this.OwnerMenu;
                var result = await walletWindow.ShowDialog<bool>(this);

                if (!result) return;

                using var context = new DiplomHorseClubContext();
                wallet = context.Wallets.FirstOrDefault(w => w.UserId == user.Id);

                if (wallet == null || wallet.Summ < _stablePrice)
                {
                    await new CustomMessageBox("Недостаточно средств даже после пополнения!").ShowDialog(this);
                    return;
                }
            }

            if (wallet.Summ >= _stablePrice)
            {
                using var context = new DiplomHorseClubContext();
                wallet.Summ -= _stablePrice;
                context.Update(wallet);
                await context.SaveChangesAsync();

                OwnerMenu.LoadUserData();

                // Устанавливаем флаги состояния
                _isPaid = true;
                _selectedStableId = _selectedStable?.Id;

                // Разблокируем кнопку добавления
                AddHorseButton.IsEnabled = true;

                // Блокируем кнопку выбора денника
                SelectStableButton.IsEnabled = false;

                // Блокируем кнопку оплаты
                PayButton.IsEnabled = false;

                PaymentStatusText.Text = "Оплачено";
                PaymentStatusText.Foreground = Avalonia.Media.Brushes.Green;
            }
            else
            {
                await new CustomMessageBox("Недостаточно средств для оплаты!").ShowDialog(this);
            }
        }

        private async void AddHorse_Click(object sender, RoutedEventArgs e)
        {
            // Валидация обязательных полей
            if (string.IsNullOrWhiteSpace(HorseNameTextBox.Text))
            {
                CustomMessageBox messageBox = new CustomMessageBox("Введите имя лошади!");
                await messageBox.ShowDialog(this);
                return;
            }

            if (string.IsNullOrWhiteSpace(BreedTextBox.Text))
            {
                CustomMessageBox messageBox = new CustomMessageBox("Введите породу лошади!");
                await messageBox.ShowDialog(this);
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedGender))
            {
                CustomMessageBox messageBox = new CustomMessageBox("Выберите пол лошади из списка!");
                await messageBox.ShowDialog(this);
                return;
            }

            if (!DateTime.TryParse(BirthdayTextBox.Text, out var birthDate))
            {
                CustomMessageBox messageBox = new CustomMessageBox("Некорректный формат даты рождения (используйте дд.мм.гггг)");
                await messageBox.ShowDialog(this);
                return;
            }

            if (!VetPassportCheckBox.IsChecked.HasValue || !VetPassportCheckBox.IsChecked.Value)
            {
                CustomMessageBox messageBox = new CustomMessageBox("Необходимо подтвердить наличие ветеринарного паспорта!");
                await messageBox.ShowDialog(this);
                return;
            }

            if (_selectedStable == null)
            {
                CustomMessageBox messageBox = new CustomMessageBox("Выберите и оплатите денник для лошади!");
                await messageBox.ShowDialog(this);
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
                    var horse = new Horse
                    {
                        Id = context.Horses.Any() ? context.Horses.Max(a => a.Id) + 1 : 1,
                        HorseName = HorseNameTextBox.Text.Trim(),
                        Breed = BreedTextBox.Text.Trim(),
                        Gender = SelectedGender, // Используем SelectedGender вместо GenderTextBox.Text
                        Datebirth = birthDate,
                        VetPasport = VetPassportCheckBox.IsChecked.Value,
                        HorsePhoto = photoName
                    };

                    // Добавление в базу данных
                    //_dbContext.Horses.Add(horse);
                    context.Horses.Add(horse);
                    await context.SaveChangesAsync();
                    _horseAdded = true;

                    // Привязка лошади к пользователю
                    var userHorse = new UserHorse
                    {
                        Id = context.UserHorses.Any() ? context.UserHorses.Max(a => a.Id) + 1 : 1,
                        Userid = UserForAuthorization.SelectedUser.Id,
                        Horseid = horse.Id
                    };
                    //_dbContext.UserHorses.Add(userHorse);
                    context.UserHorses.Add(userHorse);

                    //Добавляем stable и присваиваем его в Horse stable
                    var stable = new Stable
                    {
                        Id = context.Stables.Any() ? context.Stables.Max(a => a.Id) + 1 : 1,
                        StableCode = context.Stables.Any() ? (context.Stables.Max(a => Convert.ToInt32(a.StableCode)) + 1).ToString() : 1.ToString(),
                        Typeid = _selectedStable.Id
                    };

                    //Уменьшаем кол-во доступных денников данного типа на 1
                    _selectedStable.StablesArendable -= 1;
                    context.Stabletypes.Update(_selectedStable);
                    //await context.SaveChangesAsync();

                    //_dbContext.Stables.Add(stable);
                    context.Stables.Add(stable);

                    // Назначение денника
                    var horseStable = new HorseStable
                    {
                        Id = context.HorseStables.Any() ? context.HorseStables.Max(a => a.Id) + 1 : 1,
                        Horseid = horse.Id,
                        Stableid = stable.Id,
                        Assignmentdate = DateOnly.FromDateTime(DateTime.Now.AddMonths(1)),
                    };

                    //_dbContext.HorseStables.Add(userHorse);
                    context.HorseStables.Add(horseStable);

                    // Сохранение изменений
                    //await _dbContext.SaveChangesAsync();
                    await context.SaveChangesAsync();

                    CustomMessageBox messageBox = new CustomMessageBox("Лошадь успешно добавлена!");
                    await messageBox.ShowDialog(this);

                    // Закрытие окна с положительным результатом
                    Close(true);
                }
            }
            catch (DbUpdateException dbEx)
            {
                CustomMessageBox messageBox = new CustomMessageBox("Не удалось сохранить данные!");
                await messageBox.ShowDialog(this);
            }
            catch (IOException ioEx)
            {
                CustomMessageBox messageBox = new CustomMessageBox("Ошибка файловой системы. Не удалось сохранить фото!");
                await messageBox.ShowDialog(this);
            }
            catch (Exception ex)
            {

                CustomMessageBox messageBox = new CustomMessageBox("Не удалось добавить лошадь!");
                await messageBox.ShowDialog(this);
            }
        }

        private async Task ShowMessage(string title, string message)
        {
            var dialog = new Window
            {
                Title = title,
                Content = new TextBlock { Text = message },
                SizeToContent = SizeToContent.WidthAndHeight
            };
            await dialog.ShowDialog(this);
        }


    }
}