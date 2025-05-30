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

            // ���������, ���� �� ������, � �� ��������� �� ������
            if (_isPaid && !_horseAdded)
            {
                e.Cancel = true; // ��������� ��������, ���� �� ����������

                var confirmDialog = new CustomMessageBox(
                    "�� �� �������� ������. ������� ������ �� ����?",
                    true); // true � ���� ������ "������"

                var result = await confirmDialog.ShowDialog<bool>(this);

                if (result) // ���� ���������� ������� ������
                {
                    await ReturnMoneyToWallet();
                    _isClosing = true;
                    Close(); // ��������� ���� ����� ��������
                }
                // ���� ����� "������" � ���� �������� �������� (e.Cancel = true)
            }
            else
            {
                // ���� ������ �� ���� ��� ������ ��������� � ��������� ��� ��������
                base.OnClosing(e);
            }
        }

        private async void CloseWindow(object sender, RoutedEventArgs e)
        {
            if (_isPaid && !_horseAdded)
            {
                var confirmDialog = new CustomMessageBox(
                    "�� �� �������� ������. ������� ������ �� ����?",
                    true);

                var result = await confirmDialog.ShowDialog<bool>(this);

                if (result) // ���� ���������� ������� ������
                {
                    await ReturnMoneyToWallet();
                    _isClosing = true;
                    Close();
                }
                // ���� ����� "������" � ���� �������� ��������
            }
            else
            {
                _isClosing = true;
                Close(); // ��������� ��� ��������, ���� ��� ������
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
                await new CustomMessageBox($"������ ��� �������� �����: {ex.Message}").ShowDialog(this);
            }
        }

        private ObservableCollection<string> _genders = new ObservableCollection<string>
        {
                    "������",
                    "�����",
                    "�������"
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

        public Menu OwnerMenu { get; set; } //������ �� ���� ��� ���������� ���������

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
                Title = "�������� ���� ������",
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

                // ���������� ���������� � ��������� �������
                SelectedStableInfo.IsVisible = true;
                StableCodeText.Text = $"������";
                StableTypeText.Text = $"���: {_selectedStable.Name}";
                StablePriceText.Text = $"����: {_stablePrice} ���./���";

                // ���������� ���� �� ������
                PaymentInfo.IsVisible = true;
                PaymentAmountText.Text = $"{_stablePrice} ���.";
                PaymentDescriptionText.Text = $"������ �� ������ ������� �� 1 �����";

                // ��������� ������ ���������� �� ������
                AddHorseButton.IsEnabled = false;
            }
            else
            {
                CustomMessageBox messageBox = new CustomMessageBox("�� ������ ������!");
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
                    await new CustomMessageBox("������������ ������� ���� ����� ����������!").ShowDialog(this);
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

                // ������������� ����� ���������
                _isPaid = true;
                _selectedStableId = _selectedStable?.Id;

                // ������������ ������ ����������
                AddHorseButton.IsEnabled = true;

                // ��������� ������ ������ �������
                SelectStableButton.IsEnabled = false;

                // ��������� ������ ������
                PayButton.IsEnabled = false;

                PaymentStatusText.Text = "��������";
                PaymentStatusText.Foreground = Avalonia.Media.Brushes.Green;
            }
            else
            {
                await new CustomMessageBox("������������ ������� ��� ������!").ShowDialog(this);
            }
        }

        private async void AddHorse_Click(object sender, RoutedEventArgs e)
        {
            // ��������� ������������ �����
            if (string.IsNullOrWhiteSpace(HorseNameTextBox.Text))
            {
                CustomMessageBox messageBox = new CustomMessageBox("������� ��� ������!");
                await messageBox.ShowDialog(this);
                return;
            }

            if (string.IsNullOrWhiteSpace(BreedTextBox.Text))
            {
                CustomMessageBox messageBox = new CustomMessageBox("������� ������ ������!");
                await messageBox.ShowDialog(this);
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedGender))
            {
                CustomMessageBox messageBox = new CustomMessageBox("�������� ��� ������ �� ������!");
                await messageBox.ShowDialog(this);
                return;
            }

            if (!DateTime.TryParse(BirthdayTextBox.Text, out var birthDate))
            {
                CustomMessageBox messageBox = new CustomMessageBox("������������ ������ ���� �������� (����������� ��.��.����)");
                await messageBox.ShowDialog(this);
                return;
            }

            if (!VetPassportCheckBox.IsChecked.HasValue || !VetPassportCheckBox.IsChecked.Value)
            {
                CustomMessageBox messageBox = new CustomMessageBox("���������� ����������� ������� ������������� ��������!");
                await messageBox.ShowDialog(this);
                return;
            }

            if (_selectedStable == null)
            {
                CustomMessageBox messageBox = new CustomMessageBox("�������� � �������� ������ ��� ������!");
                await messageBox.ShowDialog(this);
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
                    // ���������� ���� �� ���������
                    photoName = "tatu.png";
                }

                using (var context = new DiplomHorseClubContext())
                {
                    var horse = new Horse
                    {
                        Id = context.Horses.Any() ? context.Horses.Max(a => a.Id) + 1 : 1,
                        HorseName = HorseNameTextBox.Text.Trim(),
                        Breed = BreedTextBox.Text.Trim(),
                        Gender = SelectedGender, // ���������� SelectedGender ������ GenderTextBox.Text
                        Datebirth = birthDate,
                        VetPasport = VetPassportCheckBox.IsChecked.Value,
                        HorsePhoto = photoName
                    };

                    // ���������� � ���� ������
                    //_dbContext.Horses.Add(horse);
                    context.Horses.Add(horse);
                    await context.SaveChangesAsync();
                    _horseAdded = true;

                    // �������� ������ � ������������
                    var userHorse = new UserHorse
                    {
                        Id = context.UserHorses.Any() ? context.UserHorses.Max(a => a.Id) + 1 : 1,
                        Userid = UserForAuthorization.SelectedUser.Id,
                        Horseid = horse.Id
                    };
                    //_dbContext.UserHorses.Add(userHorse);
                    context.UserHorses.Add(userHorse);

                    //��������� stable � ����������� ��� � Horse stable
                    var stable = new Stable
                    {
                        Id = context.Stables.Any() ? context.Stables.Max(a => a.Id) + 1 : 1,
                        StableCode = context.Stables.Any() ? (context.Stables.Max(a => Convert.ToInt32(a.StableCode)) + 1).ToString() : 1.ToString(),
                        Typeid = _selectedStable.Id
                    };

                    //��������� ���-�� ��������� �������� ������� ���� �� 1
                    _selectedStable.StablesArendable -= 1;
                    context.Stabletypes.Update(_selectedStable);
                    //await context.SaveChangesAsync();

                    //_dbContext.Stables.Add(stable);
                    context.Stables.Add(stable);

                    // ���������� �������
                    var horseStable = new HorseStable
                    {
                        Id = context.HorseStables.Any() ? context.HorseStables.Max(a => a.Id) + 1 : 1,
                        Horseid = horse.Id,
                        Stableid = stable.Id,
                        Assignmentdate = DateOnly.FromDateTime(DateTime.Now.AddMonths(1)),
                    };

                    //_dbContext.HorseStables.Add(userHorse);
                    context.HorseStables.Add(horseStable);

                    // ���������� ���������
                    //await _dbContext.SaveChangesAsync();
                    await context.SaveChangesAsync();

                    CustomMessageBox messageBox = new CustomMessageBox("������ ������� ���������!");
                    await messageBox.ShowDialog(this);

                    // �������� ���� � ������������� �����������
                    Close(true);
                }
            }
            catch (DbUpdateException dbEx)
            {
                CustomMessageBox messageBox = new CustomMessageBox("�� ������� ��������� ������!");
                await messageBox.ShowDialog(this);
            }
            catch (IOException ioEx)
            {
                CustomMessageBox messageBox = new CustomMessageBox("������ �������� �������. �� ������� ��������� ����!");
                await messageBox.ShowDialog(this);
            }
            catch (Exception ex)
            {

                CustomMessageBox messageBox = new CustomMessageBox("�� ������� �������� ������!");
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