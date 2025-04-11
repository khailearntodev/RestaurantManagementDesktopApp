using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;
using RestaurantManagement.Views;
namespace RestaurantManagement.ViewModels
{
    public class EmployeeViewModel : BaseViewModel
    { 
        private readonly QlnhContext _context;
        private ObservableCollection<EmployeeDisplayModel> _employees;
        private ObservableCollection<EmployeeDisplayModel> _originalList;
        private EmployeeDisplayModel _selectedEmployee;
        private string _searchText;
        private int _chucVuSelectedIndex;

        public int ChucVuSelectedIndex
        {
            get => _chucVuSelectedIndex;
            set
            {
                if (_chucVuSelectedIndex != value)
                {
                    _chucVuSelectedIndex = value;
                    OnPropertyChanged();
                }
            }
        }
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }

        public EmployeeDisplayModel SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                _selectedEmployee = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<EmployeeDisplayModel> Employees
        {
            get => _employees;
            set
            {
                _employees = value;
                OnPropertyChanged();
            }
        }
        public ICommand AddEmployeeCommand { get; }
        public ICommand EditEmployeeCommand { get; }
        public ICommand DeleteEmployeeCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand NewEmployeeCommand { get; }
        public ICommand AttendanceCommand { get; }

        public EmployeeViewModel()
        {
            _context = new QlnhContext();
            _originalList = new ObservableCollection<EmployeeDisplayModel>();
            Employees = new ObservableCollection<EmployeeDisplayModel>();
            

            LoadEmployeeData();

            AddEmployeeCommand = new RelayCommand<object>(
                execute: _ => AddEmployee(),
                canExecute: _ => CanAddEmployee());

            EditEmployeeCommand = new RelayCommand<object>(
                execute: _ => UpdateEmployee(),
                canExecute: _ => SelectedEmployee != null && SelectedEmployee.Id > 0);

            DeleteEmployeeCommand = new RelayCommand<object>(
                execute: _ => DeleteEmployee(),
                canExecute: _ => SelectedEmployee != null && SelectedEmployee.Id > 0);

            RefreshCommand = new RelayCommand<object>(
                execute: _ => Refresh(),
                canExecute: _ => true);

            NewEmployeeCommand = new RelayCommand<object>(
                execute: _ => CreateNewEmployee(),
                canExecute: _ => true);
            AttendanceCommand = new RelayCommand<object>(
                execute: _ => OpenAttendance(),
                canExecute: _ => true);
        }
        #region new
        private void CreateNewEmployee()
        {
            SelectedEmployee = new EmployeeDisplayModel
            {
                Id = 0,
                HoTen = string.Empty,
                CongViec = string.Empty,
                LoaiNhanVien = string.Empty,
                Address = string.Empty,
                Phone = string.Empty,
                DateBorn = DateTime.Now,
                NgayVaoLam = DateTime.Now,
                TenTaiKhoan = string.Empty,
                MatKhau = string.Empty,
                LuongCoBan=0,
                LuongTheoGio = 0
            };


            System.Diagnostics.Debug.WriteLine("Created a new employee instance.");
        }
        #endregion
        #region Load
        private void LoadEmployeeData()
        {
            try
            {

                var employeeList = _context.Nhanviens
                    .Include(nv => nv.Taikhoans)
                    .Where(nv => nv.IsDeleted.HasValue && nv.IsDeleted == false) 
                    .ToList()
                    .Select(e => ConvertToEmployeeDisplayModel(e))
                    .OrderBy(e => e.HoTen)
                    .ToList();

                _originalList = new ObservableCollection<EmployeeDisplayModel>(employeeList);
                Employees = new ObservableCollection<EmployeeDisplayModel>(_originalList);
                System.Diagnostics.Debug.WriteLine($"Loaded {Employees.Count} employees.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading employees: {ex.Message}");
            }
        }

        private EmployeeDisplayModel ConvertToEmployeeDisplayModel(Nhanvien employee)
        {
            var account = employee.Taikhoans.FirstOrDefault();

            return new EmployeeDisplayModel
            {
                Id = employee.Id,
                HoTen = employee.HoTen,
                CongViec = employee.CongViec,
                LoaiNhanVien = employee.LoaiNhanVien,
                Address = employee.DiaChi,
                Phone = employee.Sdt,
                DateBorn = employee.NgaySinh,
                NgayVaoLam = employee.NgayVaoLam,
                LuongCoBan = employee.LuongThang ?? 0m, 
                LuongTheoGio = employee.LuongTheoGio ?? 0m, 
                TenTaiKhoan = account != null ? account.TenTaiKhoan : string.Empty,
                MatKhau = account != null ? account.MatKhau : string.Empty
            };

        }
        #endregion
        #region Add
        private bool CanAddEmployee()
        {
            if (SelectedEmployee == null || SelectedEmployee.Id != 0)
            {
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"HoTen: {SelectedEmployee.HoTen}, CongViec: {SelectedEmployee.CongViec}");
            return !string.IsNullOrWhiteSpace(SelectedEmployee.HoTen) &&
                   !string.IsNullOrWhiteSpace(SelectedEmployee.CongViec);
        }


        private void AddEmployee()
        {
            try
            {
                if (SelectedEmployee == null) return;

                if(CheckValiddate()==false) return;
                var newEmployee = new Nhanvien
                {
                    HoTen = SelectedEmployee.HoTen,
                    CongViec = SelectedEmployee.CongViec,
                    LoaiNhanVien = SelectedEmployee.LoaiNhanVien,
                    NgayVaoLam = SelectedEmployee.NgayVaoLam,
                    DiaChi = SelectedEmployee.Address,
                    Sdt = SelectedEmployee.Phone,
                    NgaySinh = SelectedEmployee.DateBorn,
                    IsDeleted = false ,
                    LuongThang=SelectedEmployee.LuongCoBan,
                    LuongTheoGio=SelectedEmployee.LuongTheoGio,
                };

                _context.Nhanviens.Add(newEmployee);
                _context.SaveChanges(); 

                if (!string.IsNullOrEmpty(SelectedEmployee.TenTaiKhoan) && !string.IsNullOrEmpty(SelectedEmployee.MatKhau))
                {
                    var newAccount = new Taikhoan
                    {
                        IdnhanVien = newEmployee.Id, 
                        TenTaiKhoan = SelectedEmployee.TenTaiKhoan,
                        MatKhau = SelectedEmployee.MatKhau,
                        PhanQuyen = 1,  // SỬA LẠI PHÂN QUYỀN LÀ 1 
                        IsDeleted = false 
                    };

                    _context.Taikhoans.Add(newAccount);
                    _context.SaveChanges();
                }

                Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding employee: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }

        #endregion
        #region Update
        private void UpdateEmployee()
        {
            try
            {
                if (SelectedEmployee == null) return;
                if (CheckValiddate() == false) return;

                var employee = _context.Nhanviens.FirstOrDefault(e => e.Id == SelectedEmployee.Id);
                if (employee == null) return;

                employee.HoTen = SelectedEmployee.HoTen;
                employee.CongViec = SelectedEmployee.CongViec;
                employee.LoaiNhanVien = SelectedEmployee.LoaiNhanVien;
                employee.NgayVaoLam = SelectedEmployee.NgayVaoLam;
                employee.DiaChi = SelectedEmployee.Address;
                employee.Sdt = SelectedEmployee.Phone;
                employee.LuongTheoGio=SelectedEmployee.LuongTheoGio;
                employee.LuongThang=SelectedEmployee.LuongCoBan;
                
                _context.Nhanviens.Update(employee);
                _context.SaveChanges();

                var account = _context.Taikhoans.FirstOrDefault(a => a.IdnhanVien == employee.Id);
                if (account != null)
                {
                    account.TenTaiKhoan = SelectedEmployee.TenTaiKhoan;
                    account.MatKhau = SelectedEmployee.MatKhau;
                    _context.Taikhoans.Update(account);
                }

                _context.SaveChanges();

                MessageBox.Show("CẬP NHẬT THÔNG TIN NHÂN VIÊN THÀNH CÔNG");

                Refresh();
                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating employee: {ex.Message}");
            }
        }
        #endregion
        #region delete
        private void DeleteEmployee()
        {
            try
            {
                if (SelectedEmployee == null) return;

                
                var employee = _context.Nhanviens.FirstOrDefault(e => e.Id == SelectedEmployee.Id);
                if (employee == null) return;

               
                employee.IsDeleted = true;

               
                var account = _context.Taikhoans.FirstOrDefault(a => a.IdnhanVien == employee.Id);
                if (account != null)
                {
                    account.IsDeleted = true;
                }

               
                _context.SaveChanges();

                
                Refresh();
    
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting employee: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }
        #endregion
        #region Search
        private void ApplyFilter()
        {
            if (_originalList == null || !_originalList.Any()) return;

            var filteredList = _originalList.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filteredList = filteredList.Where(b => b.HoTen.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }
            Employees = new ObservableCollection<EmployeeDisplayModel>(filteredList);
        }

            private void Refresh()
        {
            LoadEmployeeData();
            SelectedEmployee = null;
        }
        #endregion

        #region attedance
        public void OpenAttendance()
        {
            AttendanceView attendance = new AttendanceView();
            attendance.ShowDialog();

        }
        #endregion
        public bool CheckValiddate()
        {
            if (!SelectedEmployee.Phone.All(char.IsDigit))
            {
                System.Windows.Forms.MessageBox.Show("Số điện thoại chỉ được chứa các chữ số");
                SelectedEmployee.Phone = "";
                return false;
            }
            if (SelectedEmployee.LuongTheoGio == 0 && SelectedEmployee.LuongCoBan == 0)
            {
                System.Windows.Forms.MessageBox.Show("Vui lòng nhập lương nhân viên");
                return false;
            }
            if (SelectedEmployee.DateBorn > SelectedEmployee.NgayVaoLam)
            {
                System.Windows.Forms.MessageBox.Show("Ngày sinh phải nhỏ hơn ngày vào làm");
                return false;
            }
            return true;
        }
    }


    public class EmployeeDisplayModel
    {
        public int Id { get; set; }
        public string HoTen { get; set; }
        public string CongViec { get; set; }
        public string LoaiNhanVien { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public DateTime DateBorn { get; set; }
        public DateTime NgayVaoLam { get; set; }
        public string TenTaiKhoan { get; set; }
        public string MatKhau { get; set; }
        public decimal LuongCoBan { get; set; }
        public decimal LuongTheoGio { get; set; }
       
    }
}
