using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using RestaurantManagement.Models;
using static RestaurantManagement.ViewModels.MenuViewModel;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using System.Windows.Media.Imaging;


namespace RestaurantManagement.ViewModels
{
    public class MenuViewModel : BaseViewModel
    {
        public ICommand OrderCommand { get; set; }
        public ICommand DeleteAllCommand { get; set; }
        public ICommand DeleteSelectedItemCommand { get; set; }

        public ICommand NotifyCookingCommand { get; set; }
        public MenuViewModel()
        {
            LoadDataFromDatabase();
            LoadMenuItems();
            ControlsEnabled = true;
            OrderCommand = new RelayCommand(OrderDish, CanExecute);
            DeleteSelectedItemCommand = new RelayCommand(DeleteSelectedItem, CanExecuteDelete);
            DeleteAllCommand = new RelayCommand(DeleteAllSelectedItems, CanExecuteDelete);
            NotifyCookingCommand = new RelayCommand(NotifyCooking, CanExecuteNotify);
            SelectedItems = new ObservableCollection<SelectedItem>();

        }
        public void LoadMenuItems()
        {
            using (var context = new QlnhContext())
            {
                var menuItems = context.Doanuongs.ToList();
                var linkAnhDoAnUong = new ObservableCollection<BitmapImage>();
                MenuItemCollection = new ObservableCollection<Doanuong>(menuItems);

                foreach (var dish in menuItems)
                {
                    BitmapImage image = new BitmapImage();

                    if (!string.IsNullOrEmpty(dish.AnhDoAnUong))
                    {

                        image.BeginInit();
                        image.UriSource = new Uri(dish.AnhDoAnUong, UriKind.Absolute);
                        image.EndInit();
                    }
                    else
                    {
                        image.BeginInit();
                        image.UriSource = new Uri("https://upload.wikimedia.org/wikipedia/commons/thumb/a/ac/No_image_available.svg/800px-No_image_available.svg.png?fbclid=IwZXh0bgNhZW0CM...oXkgwfg", UriKind.Absolute);
                        image.EndInit();
                    }

                    linkAnhDoAnUong.Add(image);
                }
            }
            InitializeMenuView();
        }

        //SEARCH
        private string _searchKeyword;
        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                if (_searchKeyword != value)
                {
                    _searchKeyword = value;
                    OnPropertyChanged();
                    MenuItemsView.Refresh();
                }
            }
        }
        private void MenuItems_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is Doanuong dish)
            {
                if (string.IsNullOrEmpty(SearchKeyword))
                {
                    e.Accepted = true;
                }
                else
                {
                    e.Accepted = dish.TenDoAnUong.ToLower().Contains(SearchKeyword.ToLower());
                }
            }
        }


        //TONG_TIEN
        private decimal _sum;
        public decimal SUM
        {
            get => _sum;
            set
            {
                if (_sum != value)
                {
                    _sum = value;
                    OnPropertyChanged();
                }
            }
        }
        private ObservableCollection<BitmapImage> _anhDoAnUong;

        public ObservableCollection<BitmapImage> AnhDoAnUong
        {
            get => _anhDoAnUong;
            set
            {
                _anhDoAnUong = value;
                OnPropertyChanged();
            }
        }

        private void UpdateSum()
        {
            SUM = SelectedItems.Sum(item => item.GiaMon * item.SoLuong);
        }

        //DELETE_ITEMS

        private void DeleteSelectedItem(object parameter)
        {
            if (parameter is SelectedItem itemToRemove)
            {
                using (var context = new QlnhContext())
                {
                    var hoaDon = context.Hoadons.FirstOrDefault(h => h.Idban == SelectedTable.Id);
                    if (hoaDon != null)
                    {
                        var doAnUong = context.Doanuongs.FirstOrDefault(d => d.Id == itemToRemove.IdDoAnUong);
                        if (doAnUong != null)
                        {
                            var chiTietHoaDon = context.Cthds.FirstOrDefault(c => c.IddoAnUong == doAnUong.Id && c.IdhoaDon == hoaDon.Id);
                            if (chiTietHoaDon != null)
                            {
                                context.Cthds.Remove(chiTietHoaDon);
                            }
                        }
                    }

                    context.SaveChanges();
                }

                SelectedItems.Remove(itemToRemove);
                UpdateSum();
            }
        }




        //Menu Items
        public ObservableCollection<Doanuong> MenuItemCollection { get; set; }


        //Day
        public string Day
        {
            get
            {
                return DateTime.Today.DayOfWeek.ToString() + ", " + DateTime.Now.ToString("dd/MM/yyyy");
            }
        }

        //TABLES
        public ObservableCollection<Ban> DanhSachBan { get; set; }

        public void LoadTablesFromDatabase()
        {
            using (var context = new QlnhContext())
            {
                var tables = context.Bans.ToList();
                DanhSachBan = new ObservableCollection<Ban>(tables);
            }
        }

        //ORDER
        public class OrderItem
        {
            public int Id { get; set; }
            public string? TenDoAnUong { get; set; }
            public int? SoLuong { get; set; }
            public decimal? DonGia { get; set; }
        }

        private void OrderDish(object parameter)
        {
            if (parameter is Doanuong dish)
            {
                var existingItem = SelectedItems.FirstOrDefault(item => item.IdDoAnUong == dish.Id);

                if (existingItem != null)
                {
                    existingItem.SoLuong++;
                }
                else
                {
                    SelectedItems.Add(new SelectedItem
                    {
                        IdDoAnUong = dish.Id,
                        TenDoAnUong = dish.TenDoAnUong,
                        GiaMon = dish.DonGia ?? 0,
                        SoLuong = 1
                    });
                }
                UpdateSum();
                OnPropertyChanged();
            }
        }

        private Ban _selectedTable;
        public Ban SelectedTable
        {
            get => _selectedTable;
            set
            {
                _selectedTable = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        //SELECTED ITEMS
        private ObservableCollection<SelectedItem> _selectedItems;
        public ObservableCollection<SelectedItem> SelectedItems
        {
            get => _selectedItems;
            set
            {
                _selectedItems = value;
                OnPropertyChanged();
            }
        }

        public class SelectedItem : INotifyPropertyChanged
        {
            private string _tenDoAnUong;
            private decimal _giaMon;
            private int _soLuong;
            private string _thoiGian;
            public int IdDoAnUong { get; set; }
            public string ThoiGian
            {
                get => _thoiGian;
                set
                {
                    _thoiGian = value;
                    OnPropertyChanged(nameof(ThoiGian));
                }
            }

            public string TenDoAnUong
            {
                get => _tenDoAnUong;
                set
                {
                    _tenDoAnUong = value;
                    OnPropertyChanged(nameof(TenDoAnUong));
                }
            }

            public decimal GiaMon
            {
                get => _giaMon;
                set
                {
                    _giaMon = value;
                    OnPropertyChanged(nameof(GiaMon));
                }
            }

            public int SoLuong
            {
                get => _soLuong;
                set
                {
                    _soLuong = value;
                    OnPropertyChanged(nameof(SoLuong));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void LoadDataFromDatabase()
        {
            try
            {
                using (var context = new QlnhContext())
                {
                    var tables = context.Bans.ToList();
                    DanhSachBan = new ObservableCollection<Ban>(tables);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data: {ex.Message}");
            }
        }
        ////ADD IMAGES
        private void LoadImageFromUri(string imagePath, string defaultImg)
        {
            try
            {
                Uri uri = new Uri(imagePath, UriKind.Absolute);
                BitmapImage bmi = new BitmapImage();
                bmi.BeginInit();
                bmi.UriSource = uri;
                bmi.EndInit();
                AnhDoAnUong.Add(bmi);
            }
            catch (UriFormatException)
            {
                Uri defaultUri = new Uri(defaultImg, UriKind.Absolute);
                BitmapImage bmi = new BitmapImage();
                bmi.BeginInit();
                bmi.UriSource = defaultUri;
                bmi.EndInit();
                AnhDoAnUong.Add(bmi);
            }
        }


        //DELETE

        private void RemoveSelectedItems()
        {
            using (var context = new QlnhContext())
            {
                foreach (var item in SelectedItems)
                {
                    var doAnUong = context.Doanuongs.FirstOrDefault(d => d.TenDoAnUong == item.TenDoAnUong);
                    var hoaDon = context.Hoadons.FirstOrDefault(h => h.Idban == SelectedTable.Id);
                    if (doAnUong != null)
                    {
                        var chiTietHoaDon = context.Cthds.FirstOrDefault(c => c.IddoAnUong == doAnUong.Id && c.IdhoaDon == hoaDon.Id);
                        if (chiTietHoaDon != null)
                        {
                            context.Cthds.Remove(chiTietHoaDon);
                        }
                    }
                }
                context.SaveChanges();
            }
            SelectedItems.Clear();
            MessageBox.Show("Các món ăn đã được xóa thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteAllSelectedItems(object parameter)
        {
            SelectedItems.Clear();
            UpdateSum();
            OnPropertyChanged();
        }
        private void DeleteAll(object parameter)
        {
            RemoveSelectedItems();
            SelectedItems.Clear();
            UpdateSum();
        }

        ////SORT
        private readonly CollectionViewSource _menuItemsView = new CollectionViewSource();
        private string _myComboboxSelection = "A -> Z";
        public ObservableCollection<string> ComboBoxItems { get; set; } = new ObservableCollection<string>
        {
            "A -> Z",
            "Z -> A",
            "Giá thấp -> cao",
            "Giá cao -> thấp"
        };
        public string MyComboboxSelection
        {
            get => _myComboboxSelection;
            set
            {
                _myComboboxSelection = value;
                OnPropertyChanged();
                SortMenuItems();
            }
        }
        public void SortMenuItems()
        {
            _menuItemsView.SortDescriptions.Clear();

            if (MyComboboxSelection == "Giá cao -> thấp")
            {
                _menuItemsView.SortDescriptions.Add(new SortDescription("DonGia", ListSortDirection.Descending));
            }
            else if (MyComboboxSelection == "Giá thấp -> cao")
            {
                _menuItemsView.SortDescriptions.Add(new SortDescription("DonGia", ListSortDirection.Ascending));
            }
            else if (MyComboboxSelection == "A -> Z")
            {
                _menuItemsView.SortDescriptions.Add(new SortDescription("TenDoAnUong", ListSortDirection.Ascending));
            }
            else if (MyComboboxSelection == "Z -> A")
            {
                _menuItemsView.SortDescriptions.Add(new SortDescription("TenDoAnUong", ListSortDirection.Descending));
            }
            MenuItemsView.Refresh();
        }

        public ICollectionView MenuItemsView => _menuItemsView.View;

        private void InitializeMenuView()
        {
            _menuItemsView.Source = MenuItemCollection;
            _menuItemsView.Filter += MenuItems_Filter;
            _menuItemsView.SortDescriptions.Add(new SortDescription("TenDoAnUong", ListSortDirection.Ascending));
        }

        //BAO CHE BIEN
        private void NotifyCooking(object parameter)
        {
            if (SelectedTable != null && SelectedItems.Any())
            {
                try
                {
                    using (var context = new QlnhContext())
                    {
                        // Kiểm tra tồn kho nguyên liệu cho các món được chọn
                        foreach (var item in SelectedItems)
                        {
                            var doAnUong = context.Doanuongs.FirstOrDefault(d => d.Id == item.IdDoAnUong);
                            if (doAnUong != null)
                            {
                                var ingredients = context.Ctmonans
                                    .Where(ct => ct.IddoAnUong == doAnUong.Id)
                                    .ToList();

                                foreach (var ingredient in ingredients)
                                {
                                    var nguyenLieu = context.Nguyenlieus
                                        .FirstOrDefault(nl => nl.Id == ingredient.IdnguyenLieu && nl.IsDeleted == false);

                                    if (nguyenLieu != null)
                                    {
                                        var stock = context.Ctkhos
                                            .FirstOrDefault(k => k.IdnguyenLieu == nguyenLieu.Id && k.IsDeleted == false);

                                        if (stock != null && stock.SoLuongTonDu <= 0)
                                        {
                                            MessageBox.Show($"Nguyên liệu {nguyenLieu.TenNguyenLieu} hết hàng, không thể đặt món.", "Không thể đặt món", MessageBoxButton.OK, MessageBoxImage.Warning);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                        var table = context.Bans.FirstOrDefault(b => b.Id == SelectedTable.Id);
                        table.TrangThai = true;
                        context.Bans.Update(table);
                        // Kiểm tra và xử lý hóa đơn (Hóa đơn mới hoặc đã tồn tại)
                        var hoaDon = context.Hoadons.FirstOrDefault(h => h.Idban == SelectedTable.Id && h.IsDeleted == false);

                        if (hoaDon == null)
                        {
                            // Tạo mới hóa đơn
                            hoaDon = new Hoadon
                            {
                                Idban = SelectedTable.Id,
                                IdnhanVien = 18, // Ví dụ: ID nhân viên được mặc định là 2
                                NgayHoaDon = DateTime.Now,
                                TongGia = SUM,
                                IsDeleted = false,
                            };
                            context.Hoadons.Add(hoaDon);
                            context.SaveChanges();

                            // Tạo Chebien khi hóa đơn mới được tạo
                            if (hoaDon.Id > 0)
                            {
                                var cheBien = new Chebien
                                {
                                    IdhoaDon = hoaDon.Id,
                                    ThoiGianChuanBi = 0
                                };
                                context.Chebiens.Add(cheBien);
                                context.SaveChanges();
                            }
                        }
                        else
                        {
                            // Nếu hóa đơn đã tồn tại, cộng thêm tổng giá
                            hoaDon.TongGia += SUM;
                            context.SaveChanges();
                        }

                        // Thêm chi tiết hóa đơn cho từng món ăn trong SelectedItems
                        foreach (var item in SelectedItems)
                        {
                            var doAnUong = context.Doanuongs.FirstOrDefault(d => d.Id == item.IdDoAnUong);
                            if (doAnUong != null)
                            {
                                var existingDish = context.Cthds.FirstOrDefault(e => e.IdhoaDon == hoaDon.Id && e.IddoAnUong == doAnUong.Id);

                                if (existingDish == null)
                                {
                                    // Nếu món ăn chưa có trong chi tiết hóa đơn, tạo mới
                                    var chiTietHoaDon = new Cthd
                                    {
                                        IdhoaDon = hoaDon.Id,
                                        IddoAnUong = doAnUong.Id,
                                        SoLuong = item.SoLuong,
                                        GiaMon = item.GiaMon,
                                        IsDeleted = false
                                    };
                                    context.Cthds.Add(chiTietHoaDon);
                                }
                                else
                                {
                                    // Nếu món ăn đã có trong chi tiết hóa đơn, tăng số lượng
                                    existingDish.SoLuong += item.SoLuong;
                                    var cheBien = context.Chebiens.FirstOrDefault(cb => cb.IdhoaDon == hoaDon.Id);
                                    if (cheBien != null)
                                    {
                                        cheBien.IsDeleted = false;
                                    }
                                }
                            }
                        }

                        // Lưu lại thay đổi vào cơ sở dữ liệu
                        context.SaveChanges();

                        // Cập nhật lại dữ liệu trong giao diện
                        LoadDataFromDatabase();

                        // Thông báo thành công
                        MessageBox.Show($"Đã gửi thông báo chế biến cho bàn {SelectedTable.Id}!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Xóa danh sách các món đã chọn và cập nhật tổng
                        SelectedItems.Clear();
                        UpdateSum();
                    }
                }
                catch (Exception ex)
                {
                    // Hiển thị lỗi nếu có vấn đề xảy ra
                    MessageBox.Show($"Có lỗi xảy ra khi gửi chế biến: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                // Thông báo nếu không chọn bàn hoặc món
                MessageBox.Show("Vui lòng chọn bàn và ít nhất một món để gửi thông báo chế biến.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }



        private bool _controlsEnabled;
        public bool ControlsEnabled
        {
            get => _controlsEnabled;
            set
            {
                _controlsEnabled = value;
                OnPropertyChanged();
            }
        }

        //CAN_EXECUTE
        private bool CanExecute(object parameter)
        {
            return ControlsEnabled && SelectedTable != null && parameter is Doanuong;
        }

        private bool CanExecuteDelete(object parameter) => ControlsEnabled;
        private bool CanExecuteNotify(object parameter) => ControlsEnabled;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}