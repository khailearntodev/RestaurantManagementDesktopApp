using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestaurantManagement.Models;
using RestaurantManagement.Views;
using System.Collections.ObjectModel;
using System.Runtime.Intrinsics.Arm;
using System.Windows.Input;
using System.Windows;
using System.IO;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using RestaurantManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Microsoft.EntityFrameworkCore.Diagnostics;
using static RestaurantManagement.ViewModels.AddIngredientViewModel;
using System.Windows.Media;
using System.Windows.Data;
using System.ComponentModel;
using System.Net.Http;

namespace RestaurantManagement.ViewModels
{
    public class FoodMenuViewModel : BaseViewModel
    {
        private readonly QlnhContext dbContext; // Manipulating database
        const string defaultImg = "https://upload.wikimedia.org/wikipedia/commons/thumb/a/ac/No_image_available.svg/800px-No_image_available.svg.png?fbclid=IwZXh0bgNhZW0CMTEAAR3HmVFzh45Ckm3scd4gjRC0ZsaUr87zn14IsZ07-fKfsdHZUnNXY9FyGh4_aem_wkm0VmC8XQ3-paBoXkgwfg";
        
        #region declare commands

        public ICommand AddDishesCommand { get; set; }
        public ICommand SwitchToAddDishesViewCommand { get; set; }
        public ICommand DeleteDishesCommand { get; set; }
        public ICommand ViewIngredientsCommand { get; set; }
        public ICommand EditDishImageCommand { get; set; }
        public ICommand AddDishImageCommand { get; set; }
        public ICommand SelectedDishCommand { get; set; }
        public ICommand EditDishInfoCommand { get; set; }
        public ICommand CheckImageCommand { get; set; }
        public ICommand CheckImageForAddCommand { get; set; }

        #endregion

        private ObservableCollection<Doanuong> _listDishes;
        public ObservableCollection<Doanuong> ListDishes
        {
            get => _listDishes;
            set
            {
                _listDishes = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Doanuong> displayListDishes;
        public ObservableCollection<Doanuong> DisplayListDishes
        {
            get => displayListDishes;
            set
            {
                displayListDishes = value;
                OnPropertyChanged();
            }
        }

        private bool isChange = false;

        private Doanuong _selectedDish;
        public Doanuong SelectedDish
        {
            get => _selectedDish;
            set
            {
                _selectedDish = value;
                OnPropertyChanged();

                if (_selectedDish != null && _selectedDish.AnhDoAnUong != null)
                {
                    //var converter = new ImageConverter();

                    try
                    {
                        Uri uri = new Uri(_selectedDish.AnhDoAnUong, UriKind.Absolute);

                        if (Uri.IsWellFormedUriString(uri.ToString(), UriKind.Absolute))
                        {
                            BitmapImage bmi = new BitmapImage();
                            bmi.BeginInit();
                            bmi.UriSource = uri;
                            bmi.EndInit();

                            EditedImage = bmi;
                        }
                        else
                        {
                            EditedImage = null;
                        }
                    }
                    catch (UriFormatException)
                    {
                        Uri uri = new Uri(defaultImg, UriKind.Absolute);

                        if (Uri.IsWellFormedUriString(uri.ToString(), UriKind.Absolute))
                        {
                            BitmapImage bmi = new BitmapImage();
                            bmi.BeginInit();
                            bmi.UriSource = uri;
                            bmi.EndInit();

                            EditedImage = bmi;
                        }
                        else
                        {
                            EditedImage = null;
                        }
                    }
                }
            }
        }

        private Visibility _editView;
        public Visibility EditView
        {
            get
            {
                return _editView;
            }
            set
            {
                _editView = value;
                OnPropertyChanged();
            }
        }

        private Visibility _addView;
        public Visibility AddView
        {
            get
            {
                return _addView;
            }
            set
            {
                _addView = value;
                OnPropertyChanged();
            }
        }

        private Doanuong _addedDish;
        public Doanuong AddedDish
        {
            get => _addedDish;
            set
            {
                _addedDish = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> DishType { get; set; } = new ObservableCollection<string> { "Đồ ăn", "Thức uống" };

        private string _selectedDishType;
        public string SelectedDishType
        {
            get => _selectedDishType;
            set
            {
                _selectedDishType = value;
                OnPropertyChanged();
            }
        }

        private BitmapImage addedImage;
        public BitmapImage AddedImage
        {
            get => addedImage;
            set
            {
                addedImage = value;
                OnPropertyChanged(nameof(AddedImage));
            }
        }

        private BitmapImage editedImage;
        public BitmapImage EditedImage
        {
            get => editedImage;
            set
            {
                editedImage = value;
                OnPropertyChanged(nameof(EditedImage));
            }
        }

        private string foodMenuFilterText;
        public string FoodMenuFilterText
        {
            get => foodMenuFilterText;
            set
            {
                foodMenuFilterText = value;
                OnPropertyChanged(nameof(FoodMenuFilterText));
                FoodMenu_Filter();
            }
        }

        public FoodMenuViewModel()
        {
            dbContext = new QlnhContext();
            _ = LoadData();
            InitializeCommands();
        }

        private async Task LoadData()
        {
            try
            {
                ListDishes = new ObservableCollection<Doanuong>(await dbContext.Doanuongs.Where(dau => dau.IsDeleted == false && dau.TinhTrang == true).ToListAsync());
                DisplayListDishes = new ObservableCollection<Doanuong>(ListDishes);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Lỗi khi tải dữ liệu món ăn: {ex.Message}\nChi tiết: {ex.InnerException?.Message}");
            }
        }

        private void InitializeCommands()
        {
            AddedDish = new Doanuong();
            AddDishesCommand = new RelayCommand(AddDishes);
            SwitchToAddDishesViewCommand = new RelayCommand<object>(CanExecuteSwitchToAddDishesView, ExecuteSwitchToAddDishesView);
            DeleteDishesCommand = new RelayCommand<object>(CanExecuteDeleteDish, ExecuteDeleteDish);
            SelectedDishCommand = new RelayCommand<object>(CanExecuteSelectDish, ExecuteSelectDish);
            EditDishImageCommand = new RelayCommand(EditDishImageAsync);
            ViewIngredientsCommand = new RelayCommand(ViewIngredients);
            EditDishInfoCommand = new RelayCommand(EditDishInfo);
            CheckImageCommand = new RelayCommand(async (o) => await CheckImage());
            CheckImageForAddCommand = new RelayCommand(async(o) => await CheckImageForAdd());
        }

        #region define commands

        private bool CanExecuteSwitchToAddDishesView(object? parameter)
        {
            return SelectedDish != null;
        }

        private void ExecuteSwitchToAddDishesView(object? parameter)
        {
            //AddedDish = new Doanuong();
            EditView = Visibility.Collapsed;
            AddView = Visibility.Visible;
        }

        private void AddDishes(object? parameter)
        {
            try
            {
                // Validate input

                if (string.IsNullOrWhiteSpace(AddedDish.TenDoAnUong))
                {
                    System.Windows.Forms.MessageBox.Show("Vui lòng nhập tên nguyên liệu");
                    return;
                }

                if (!AddedDish.TenDoAnUong.All(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)))
                {
                    System.Windows.Forms.MessageBox.Show("Tên món ăn không được chứa ký tự đặc biệt");
                    return;
                }

                if (!decimal.TryParse(AddedDish.DonGia?.ToString(), out var unitPrice) || unitPrice <= 0)
                {
                    System.Windows.Forms.MessageBox.Show("Vui lòng nhập lại đơn giá");
                    return;
                }

                if (!decimal.TryParse(AddedDish.ThoiGianChuanBi?.ToString(), out var prepareTime) || prepareTime <= 0)
                {
                    System.Windows.Forms.MessageBox.Show("Vui lòng nhập lại thời gian chuẩn bị");
                    return;
                }

                if (string.IsNullOrWhiteSpace(SelectedDishType))
                {
                    System.Windows.Forms.MessageBox.Show("Vui lòng chọn loại món!");
                    return;
                }

                if (AddedDish.AnhDoAnUong == null || AddedDish.AnhDoAnUong.Length == 0)
                {
                    System.Windows.Forms.MessageBox.Show("Vui lòng chọn ảnh món ăn!");
                    return;
                }

                var newDish = new Doanuong
                {
                    TenDoAnUong = AddedDish.TenDoAnUong.Trim(),
                    DonGia = AddedDish.DonGia,
                    ThoiGianChuanBi = AddedDish.ThoiGianChuanBi,
                    Loai = SelectedDishType == "Thức uống",
                    TinhTrang = true,
                    AnhDoAnUong = AddedDish.AnhDoAnUong
                };

                using var context = new QlnhContext();
                context.Doanuongs.Add(newDish);
                context.SaveChanges();

                _ = LoadData();

                System.Windows.Forms.MessageBox.Show("Thêm món thành công!");
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Lỗi khi thêm món: {ex.Message}\nChi tiết: {ex.InnerException?.Message}");
            }
        }

        private void EditDishInfo(object? parameter)
        {
            try
            {
                if (SelectedDish == null) return;

                var dishToChange = dbContext.Doanuongs.FirstOrDefault(dau => dau.Id == SelectedDish.Id);

                if (dishToChange != null)
                {
                    dishToChange.ThoiGianChuanBi = SelectedDish.ThoiGianChuanBi;
                    dishToChange.DonGia = SelectedDish.DonGia;
                    dishToChange.TenDoAnUong = SelectedDish.TenDoAnUong;

                    dbContext.SaveChanges();
                    _ = LoadData();
                }

                System.Windows.Forms.MessageBox.Show("Sửa thông tin món ăn thành công");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Có lỗi khi sửa thông tin nguyên liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        private bool CanExecuteDeleteDish(object? parameter)
        {
            return SelectedDish != null;
        }

        private void ExecuteDeleteDish(object? parameter)
        {
            try
            {
                if (SelectedDish != null)
                {
                    var confirmResult = System.Windows.MessageBox.Show(
                        $"Bạn có chắc muốn xóa món {SelectedDish.TenDoAnUong} không",
                        "Xác nhận xóa",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question
                        );
                    if (confirmResult == MessageBoxResult.Yes)
                    {
                        using var context = new QlnhContext();
                        var dishToRemove = context.Doanuongs.Find(SelectedDish.Id);

                        if (dishToRemove != null)
                        {
                            dishToRemove.IsDeleted = true;
                            context.SaveChangesAsync();
                            _ = LoadData();
                            System.Windows.MessageBox.Show("Xóa thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("Không tìm thấy món ăn để xóa.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Có lỗi khi xóa món ăn xảy ra: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteSelectDish(object? parameter)
        {
            return true;
        }

        private void ExecuteSelectDish(object? parameter)
        {
            if (SelectedDish != null)
            {
                EditView = Visibility.Visible;
                AddView = Visibility.Collapsed;
                OnPropertyChanged(nameof(SelectedDish));
            }
        }

        private async Task<bool> IsImageUrlValid(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Gửi yêu cầu GET đến URL
                    HttpResponseMessage response = await client.GetAsync(url);

                    // Kiểm tra xem mã trạng thái HTTP có phải là 200 OK
                    return response.IsSuccessStatusCode && response.Content.Headers.ContentType.MediaType.StartsWith("image");
                }
            }
            catch
            {
                // Nếu xảy ra lỗi trong quá trình yêu cầu (ví dụ: không thể kết nối), trả về false
                return false;
            }
        }

        private async Task CheckImageForAdd()
        {
            string? imgUrl = AddedDish.AnhDoAnUong;

            if (await IsImageUrlValid(imgUrl))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imgUrl, UriKind.Absolute);
                bitmap.EndInit();

                AddedImage = bitmap;
            }
            else
            {
                System.Windows.MessageBox.Show("Ảnh không tồn tại hoặc URL không hợp lệ.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                AddedDish.AnhDoAnUong = defaultImg;

                // No image available
                Uri uri = new Uri(defaultImg, UriKind.Absolute);

                if (Uri.IsWellFormedUriString(uri.ToString(), UriKind.Absolute))
                {
                    BitmapImage bmi = new BitmapImage();
                    bmi.BeginInit();
                    bmi.UriSource = uri;
                    bmi.EndInit();

                    AddedImage = bmi;
                }
                else
                {
                    AddedImage = null;
                }
            }
        }

        private async Task CheckImage()
        {
            string? imgUrl = SelectedDish.AnhDoAnUong;

            if (await IsImageUrlValid(imgUrl))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imgUrl, UriKind.Absolute);
                bitmap.EndInit();

                EditedImage = bitmap;
            }
            else
            {
                System.Windows.MessageBox.Show("Ảnh không tồn tại hoặc URL không hợp lệ.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                SelectedDish.AnhDoAnUong = defaultImg;

                // No image available
                Uri uri = new Uri(defaultImg, UriKind.Absolute);

                if (Uri.IsWellFormedUriString(uri.ToString(), UriKind.Absolute))
                {
                    BitmapImage bmi = new BitmapImage();
                    bmi.BeginInit();
                    bmi.UriSource = uri;
                    bmi.EndInit();

                    EditedImage = bmi;
                }
                else
                {
                    EditedImage = null;
                }
            }
        }

        private void EditDishImageAsync(object? parameter)
        {
            using (var context = new QlnhContext())
            {
                var doAnUong = context.Doanuongs.Where(dau => dau.Id == SelectedDish.Id).FirstOrDefault();
                if (doAnUong != null)
                {
                    doAnUong.AnhDoAnUong = SelectedDish.AnhDoAnUong;
                    context.SaveChanges();
                }
            }

            System.Windows.MessageBox.Show("LƯU ẢNH THÀNH CÔNG", "Thông báo",
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ViewIngredients(object? paramter)
        {
            AddIngredientsView addIngredientsView = new AddIngredientsView
            {
                DataContext = new AddIngredientViewModel
                {
                    SelectedDish_Ingredient = SelectedDish,
                    AddedIngredients = new ObservableCollection<DishDetails>(),
                    DeletedIngredients = new ObservableCollection<DishDetails>(),

                    IngredientOfDish = new ObservableCollection<DishDetails>
                    (
                        dbContext.Ctmonans
                        .Where(ctma => ctma.IddoAnUong == SelectedDish.Id)
                        .Select(ctma => new DishDetails
                        {
                            IdDoAnUong = SelectedDish.Id,
                            IdNguyenLieu = ctma.IdnguyenLieuNavigation.Id,
                            TenNguyenLieu = ctma.IdnguyenLieuNavigation.TenNguyenLieu,
                            SoLuongNguyenLieu = ctma.SoLuongNguyenLieu
                        })
                        .ToList()
                    )
                }
            };
            addIngredientsView.ShowDialog();
        }

        #endregion

        public void FoodMenu_Filter()
        {
            if (ListDishes == null || !ListDishes.Any()) return;
            var filteredList = ListDishes.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(FoodMenuFilterText))
            {
                filteredList = filteredList.Where(b => b.TenDoAnUong.Contains(FoodMenuFilterText, StringComparison.OrdinalIgnoreCase));
            }
            DisplayListDishes = new ObservableCollection<Doanuong>(filteredList);
        }
    }
}