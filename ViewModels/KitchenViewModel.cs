using RestaurantManagement.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace RestaurantManagement.ViewModels
{
    public class KitchenViewModel : BaseViewModel
    {
        private readonly QlnhContext _context;

        // Danh sách các món đang chờ hoàn thành
        private ObservableCollection<KitchenDish> _ListOrder;
        public ObservableCollection<KitchenDish> ListOrder
        {
            get => _ListOrder;
            set { _ListOrder = value; OnPropertyChanged(nameof(ListOrder)); }
        }

        // Danh sách các món đã hoàn thành
        private ObservableCollection<KitchenDish>? _ListDone;
        public ObservableCollection<KitchenDish>? ListDone
        {
            get => _ListDone;
            set { _ListDone = value; OnPropertyChanged(nameof(ListDone)); }
        }

        private KitchenDish? _OrderSelected;
        public KitchenDish? OrderSelected
        {
            get => _OrderSelected;
            set { _OrderSelected = value; OnPropertyChanged(nameof(OrderSelected)); }
        }

        private KitchenDish? _DoneSelected;
        public KitchenDish? DoneSelected
        {
            get => _DoneSelected;
            set { _DoneSelected = value; OnPropertyChanged(nameof(DoneSelected)); }
        }

        public ICommand MarkAsDoneCommand { get; set; }
        public ICommand ServeDishCommand { get; set; }

        public KitchenViewModel()
        {
            _context = new QlnhContext();
            ListOrder = new ObservableCollection<KitchenDish>();
            ListDone = new ObservableCollection<KitchenDish>();

            // Khởi tạo các lệnh
            MarkAsDoneCommand = new RelayCommand<object>(CanExecuteMarkAsDone, ExecuteMarkAsDone);
            ServeDishCommand = new RelayCommand<object>(CanExecuteServeDish, ExecuteServeDish);

            // Tải dữ liệu khi khởi tạo
            LoadData();
        }

        private void LoadData()
        {
            // Lấy danh sách các món đang chế biến (ListOrder) chỉ hiển thị các món chưa phục vụ
            var orders = (from cthd in _context.Cthds
                          join hd in _context.Hoadons on cthd.IdhoaDon equals hd.Id
                          join da in _context.Doanuongs on cthd.IddoAnUong equals da.Id
                          join b in _context.Bans on hd.Idban equals b.Id
                          join cb in _context.Chebiens on hd.Id equals cb.IdhoaDon
                          where cthd.IsDeleted == false && da.Id == cthd.IddoAnUong && cthd.IdhoaDon == hd.Id && cthd.IsReady == false
                          select new KitchenDish
                          {
                              Iddau = da.Id,
                              TenDoAnUong = da.TenDoAnUong,
                              IDBan = b.Id,
                              SoLuong = cthd.SoLuong,
                              IdCheBien = cb.Id
                          }).ToList();

            // Cập nhật lại ListOrder mà không làm mất các món đã phục vụ
            ListOrder = new ObservableCollection<KitchenDish>(orders);

            // Lấy danh sách các món đã hoàn thành (ListDone)
            var doneDishes = (from cthd in _context.Cthds
                              join hd in _context.Hoadons on cthd.IdhoaDon equals hd.Id
                              join da in _context.Doanuongs on cthd.IddoAnUong equals da.Id
                              join b in _context.Bans on hd.Idban equals b.Id
                              join cb in _context.Chebiens on hd.Id equals cb.IdhoaDon into cheBienGroup
                              from cb in cheBienGroup.DefaultIfEmpty()
                              where cthd.IsDeleted != true && cb.IsDeleted == false && cthd.IsReady == true 
                              select new KitchenDish
                              {
                                  Iddau = da.Id,
                                  TenDoAnUong = da.TenDoAnUong,
                                  SoLuong = cthd.SoLuong,
                                  IDBan = b.Id,
                                  IdCheBien = 0
                              }).ToList();

            ListDone = new ObservableCollection<KitchenDish>(doneDishes);
            orders = null;
            doneDishes = null;
        }


        // Thực hiện đánh dấu món ăn là đã hoàn thành
        private void ExecuteMarkAsDone(object parameter)
        {
            if (OrderSelected == null) return;

            var dish = _context.Cthds.FirstOrDefault(x =>
                x.IddoAnUong == OrderSelected.Iddau && x.SoLuong == OrderSelected.SoLuong && x.IsDeleted == false && x.IsReady == false);

            if (dish != null)
            {
                // Cập nhật IsReady của Cthds thành true khi món ăn đã hoàn thành
                dish.IsReady = true;

                // Lưu thay đổi vào cơ sở dữ liệu
                _context.SaveChanges();

                // Chuyển món ăn từ ListOrder sang ListDone
                var dishToMove = OrderSelected;
                ListOrder.Remove(dishToMove);
                ListDone.Add(dishToMove);
            }
        }




        // Thực hiện phục vụ món ăn
        private void ExecuteServeDish(object parameter)
        {
            if (DoneSelected == null) return;

            // Tìm món ăn trong Doanuongs
            var doAn = _context.Doanuongs.FirstOrDefault(da => da.TenDoAnUong == DoneSelected.TenDoAnUong);
            if (doAn != null)
            {
                // Tìm chi tiết hóa đơn tương ứng
                var dish = _context.Cthds.FirstOrDefault(x =>
                    x.IsReady==true && DoneSelected.Iddau == x.IddoAnUong && x.IsDeleted == false);
                dish.IsDeleted = true;
                _context.SaveChanges();
                if (dish != null)
                {
                    // Cập nhật nguyên liệu trong kho
                    var ingredients = (from danl in _context.Ctmonans
                                       join nl in _context.Nguyenlieus on danl.IdnguyenLieu equals nl.Id
                                       where danl.IddoAnUong == doAn.Id
                                       select new { danl.IdnguyenLieu, danl.SoLuongNguyenLieu }).ToList();

                    foreach (var ingredient in ingredients)
                    {
                        var khoItem = _context.Ctkhos.FirstOrDefault(k => k.IdnguyenLieu == ingredient.IdnguyenLieu);

                        if (khoItem != null)
                        {
                            khoItem.SoLuongTonDu -= ingredient.SoLuongNguyenLieu * DoneSelected.SoLuong;
                            if (khoItem.SoLuongTonDu < 0)
                            {
                                khoItem.SoLuongTonDu = 0;
                            }
                        }
                    }



                    var idHoaDon = _context.Cthds
                        .Where(ct => ct.IddoAnUong == dish.IddoAnUong && ct.IsDeleted == false)
                        .Select(ct => ct.IdhoaDon)
                        .FirstOrDefault();


                    if (idHoaDon > 0)
                    {
                        // Tìm Chebien liên quan tới hóa đơn
                        var relatedCheBien = _context.Chebiens.FirstOrDefault(cb => cb.IdhoaDon == idHoaDon);
                        if (relatedCheBien != null)
                        {
                            // Kiểm tra tất cả các món ăn trong hóa đơn
                            var allServed = _context.Cthds
                                .Where(ct => ct.IdhoaDon == idHoaDon)
                                .All(ct => ct.IsDeleted == true);

                            if (allServed)
                            {
                                relatedCheBien.IsDeleted = true; // Đánh dấu chế biến hoàn thành
                                _context.SaveChanges(); // Lưu thay đổi
                            }
                        }
                    }


                    // Lưu thay đổi vào cơ sở dữ liệu
                    _context.SaveChanges();

                    // Cập nhật danh sách
                    ListDone.Remove(DoneSelected);
                }
            }
        }



        // Kiểm tra điều kiện thực thi lệnh "Đánh dấu là đã hoàn thành"
        private bool CanExecuteMarkAsDone(object parameter)
        {
            return OrderSelected != null;
        }

        // Kiểm tra điều kiện thực thi lệnh "Phục vụ món ăn"
        private bool CanExecuteServeDish(object parameter)
        {
            return DoneSelected != null;
        }
    }

    // Lớp đại diện cho món ăn trong bếp
    public class KitchenDish
    {
        public int? Iddau { get; set; }
        public string? TenDoAnUong { get; set; }
        public int? SoLuong { get; set; }
        public int? IDBan { get; set; }
        public int? IdCheBien { get; set; }
    }
}
