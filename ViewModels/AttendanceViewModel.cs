using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using RestaurantManagement.Models;


namespace RestaurantManagement.ViewModels
{
    public class AttendanceViewModel : BaseViewModel
    { 
        private readonly QlnhContext _context;
        private ObservableCollection<Chamcong> _attendanceRecords;
        private ObservableCollection<Chamcong> _originalAttendanceRecords;
        private DateTime? _selectedDate;
        private string _filterText;
        private ObservableCollection<Nhanvien> _filteredEmployees;
        private string _monthlyFilterText;
        private int _selectedMonthIndex;
        private ObservableCollection<AttendanceRecord> _originalEmployeeList;

        #region Tab1
        public ObservableCollection<Chamcong> AttendanceRecords
        {
            get => _attendanceRecords;
            set
            {
                _attendanceRecords = value;
                OnPropertyChanged();
            }
        }

        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
                LoadAttendanceRecords();
                MessageBox.Show("Nhập giờ vào và giờ ra theo dạng HH:mm", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public string FilterText
        {
            get => _filterText;
            set
            {
                _filterText = value;
                OnPropertyChanged();
                ApplyDateFilter();
            }
        }
        public ObservableCollection<Nhanvien> FilteredEmployees
        {
            get => _filteredEmployees;
            set
            {
                _filteredEmployees = value;
                OnPropertyChanged();
            }
        }


        private ObservableCollection<AttendanceRecord> _employeeList;
        public ObservableCollection<AttendanceRecord> EmployeeList
        {
            get { return _employeeList; }
            set
            {
                _employeeList = value;
                OnPropertyChanged(nameof(EmployeeList));
            }
        }

        // Commands
        public ICommand SaveAttendanceCommand { get; }
        public ICommand SearchAttendanceCommand { get; }
        public ICommand ExportToExcelCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand SearchMonthlyAttendanceCommand { get; }

        public AttendanceViewModel()
        {
            _context = new QlnhContext();
            _originalAttendanceRecords = new ObservableCollection<Chamcong>();
            AttendanceRecords = new ObservableCollection<Chamcong>();
            FilteredEmployees = new ObservableCollection<Nhanvien>();

            
            SaveAttendanceCommand = new RelayCommand<object>(
                execute: _ => SaveAttendance(),
                canExecute: _ => EmployeeList.Any(a => a.GioVao != TimeOnly.MinValue && a.GioRa != TimeOnly.MinValue));  // Ensure that save is allowed only when attendance data is available

            SearchAttendanceCommand = new RelayCommand<object>(
                execute: _ => ApplyDateFilter(),
                canExecute: _ => true);

            ExportToExcelCommand = new RelayCommand<object>(
                execute: _ => ExportToExcel(),
                canExecute: _ => true);

            RefreshCommand = new RelayCommand<object>(
                execute: _ => Refresh(),
                canExecute: _ => true);

            SearchMonthlyAttendanceCommand = new RelayCommand<object>(
                execute: _ => ApplyMonthlyFilter(),
                canExecute: _ => true);

            LoadAttendanceRecords();
        }
        private void SaveAttendance()
        {
            try
            {
                foreach (var record in EmployeeList)
                {
                    

                    
                    TimeSpan gioVaoSpan = record.GioVao.ToTimeSpan();
                    TimeSpan gioRaSpan = record.GioRa.ToTimeSpan();

                   
                    if ((record.GioVao!=TimeOnly.MinValue&& record.GioRa != TimeOnly.MinValue))
                    {
                        if (gioRaSpan <= gioVaoSpan)
                        {
                            MessageBox.Show($"Giờ ra phải lớn hơn giờ vào.\n" +
                                $"Nhân viên: {record.HoTen}\n" +
                                $"Ngày: {record.NgayChamCong:dd/MM/yyyy}\n" +
                                $"Giờ vào: {record.GioVao}\n" +
                                $"Giờ ra: {record.GioRa}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                            record.GioVao = TimeOnly.MinValue;
                            record.GioRa = TimeOnly.MinValue;
                            LoadAttendanceRecords();
                            continue;
                        }
                    }

                    if (record.GioVao != TimeOnly.MinValue || record.GioRa != TimeOnly.MinValue)
                    {
                        var attendance = _context.Chamcongs
                            .FirstOrDefault(a => a.IdnhanVien == record.MaNhanVien && a.NgayChamCong == record.NgayChamCong);

                        if (attendance != null)
                        {
                            attendance.GioVao = record.GioVao;
                            attendance.GioRa = record.GioRa;
                            attendance.GhiChu = record.GhiChu;
                        }
                        else
                        {
                            _context.Chamcongs.Add(new Chamcong
                            {
                                IdnhanVien = record.MaNhanVien,
                                NgayChamCong = record.NgayChamCong,
                                GioVao = record.GioVao,
                                GioRa = record.GioRa,
                                GhiChu = record.GhiChu
                            });
                        }
                    }
                }
                _context.SaveChanges();
                LoadAttendanceRecords();
                MessageBox.Show("Lưu chấm công thành công.", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving attendance data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }





        private void LoadAttendanceRecords()
        {
            try
            {
                
                var allEmployees = _context.Nhanviens
                    .Where(nv => nv.IsDeleted.HasValue && nv.IsDeleted == false && nv.LoaiNhanVien=="Part-time")
                    .ToList();

                DateOnly selectedDate = SelectedDate.HasValue ? DateOnly.FromDateTime(SelectedDate.Value) : DateOnly.FromDateTime(DateTime.Now);

                var attendanceRecords = _context.Chamcongs
                    .Include(a => a.IdnhanVienNavigation)
                    .Where(a => a.NgayChamCong == selectedDate)
                    .ToList();

                var employeeList = allEmployees.Select(employee =>
                {
                    var attendance = attendanceRecords.FirstOrDefault(record => record.IdnhanVien == employee.Id);
                    if (attendance == null)
                    {
                        return new AttendanceRecord
                        {
                            MaNhanVien = employee.Id,
                            HoTen = employee.HoTen,
                            ChucVu = employee.CongViec,
                            GioVao = TimeOnly.MinValue,
                            GioRa = TimeOnly.MinValue,
                            GhiChu = "",
                            NgayChamCong = selectedDate
                        };
                    }
                    else
                    {
                        return new AttendanceRecord
                        {
                            MaNhanVien = employee.Id,
                            HoTen = employee.HoTen,
                            ChucVu = employee.CongViec,
                            GioVao = attendance.GioVao,
                            GioRa = attendance.GioRa,
                            GhiChu = attendance.GhiChu,
                            NgayChamCong = attendance.NgayChamCong
                        };
                    }
                }).ToList();

                _originalEmployeeList = new ObservableCollection<AttendanceRecord>(employeeList);
                EmployeeList = new ObservableCollection<AttendanceRecord>(employeeList);
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading attendance data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void ApplyDateFilter()
        {
            try
            {
                
                if (_originalEmployeeList == null || !_originalEmployeeList.Any()) return;

                DateOnly selectedDate = SelectedDate.HasValue ? DateOnly.FromDateTime(SelectedDate.Value) : DateOnly.FromDateTime(DateTime.Now);

                var filteredRecords = _originalEmployeeList.AsEnumerable();


                filteredRecords = filteredRecords.Where(a => a.NgayChamCong == selectedDate);

                if (!string.IsNullOrEmpty(FilterText))
                {
                    filteredRecords = filteredRecords.Where(a =>
                        a.HoTen.Contains(FilterText, StringComparison.OrdinalIgnoreCase));
                }

                EmployeeList = new ObservableCollection<AttendanceRecord>(filteredRecords);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lọc dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        #endregion
        private void ExportToExcel()
        {
            try
            {
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                if (MonthlyAttendanceRecords == null || !MonthlyAttendanceRecords.Any())
                {
                    MessageBox.Show("Không có dữ liệu để xuất.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string defaultFileName = $"ChamCongThang_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    FileName = defaultFileName
                };

                if (saveFileDialog.ShowDialog() != true)
                {
                    return;
                }

                string savedPath = saveFileDialog.FileName;

                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Monthly Attendance");

                    // Dòng tiêu đề bảng
                    worksheet.Cells[1, 1].Value = $"Bảng chấm công tháng {DateTime.Now:MM/yyyy}";
                    worksheet.Cells[1, 1, 1, 5].Merge = true; // Gộp ô
                    worksheet.Cells[1, 1].Style.Font.Size = 14;
                    worksheet.Cells[1, 1].Style.Font.Bold = true;
                    worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    // Tiêu đề cột
                    worksheet.Cells[2, 1].Value = "Mã Nhân Viên";
                    worksheet.Cells[2, 2].Value = "Họ Tên";
                    worksheet.Cells[2, 3].Value = "Chức Vụ";
                    worksheet.Cells[2, 4].Value = "Tổng Số Giờ Làm Việc";
                    worksheet.Cells[2, 5].Value = "Lương Tạm Tính";

                    using (var range = worksheet.Cells[2, 1, 2, 5])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    // Dữ liệu bảng
                    int row = 3;
                    foreach (var record in MonthlyAttendanceRecords)
                    {
                        worksheet.Cells[row, 1].Value = record.MaNhanVien;
                        worksheet.Cells[row, 2].Value = record.HoTen;
                        worksheet.Cells[row, 3].Value = record.ChucVu;

                        // Tổng số giờ làm việc định dạng xx.xxh
                        worksheet.Cells[row, 4].Value = record.TongSoGioLamViec;
                        worksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0.00\"h\"";

                        // Lương tạm tính định dạng xx,xxx.xxđ
                        worksheet.Cells[row, 5].Value = record.LuongTamtinh;
                        worksheet.Cells[row, 5].Style.Numberformat.Format = "#,##0.00\"đ\"";

                        row++;
                    }

                    worksheet.Cells.AutoFitColumns();

                    package.SaveAs(new System.IO.FileInfo(savedPath));
                }

                MessageBox.Show($"Xuất dữ liệu thành công!\nĐã lưu tại: {savedPath}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Đã xảy ra lỗi khi xuất dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void Refresh()
        {
            LoadAttendanceRecords();
            ApplyDateFilter();
        }
        #region Tab2
        public string MonthlyFilterText
        {
            get => _monthlyFilterText;
            set
            {
                _monthlyFilterText = value;
                OnPropertyChanged();
                ApplyMonthlyFilter();
            }
        }

        public int SelectedMonthIndex
        {
            get => _selectedMonthIndex;
            set
            {
                _selectedMonthIndex = value + 1;
                OnPropertyChanged();
                LoadAttendanceMonthRecords();
            }
        }
        private void ApplyMonthlyFilter()
        {
            try
            {
                if (!string.IsNullOrEmpty(MonthlyFilterText))
                {
                    var filteredRecords = MonthlyAttendanceRecords
                        .Where(record => record.HoTen.Contains(MonthlyFilterText, StringComparison.OrdinalIgnoreCase))
                        .ToList();


                    MonthlyAttendanceRecords = new ObservableCollection<AttedaneceMonthRecord>(filteredRecords);
                }
                else
                {

                    LoadAttendanceMonthRecords();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lọc dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private ObservableCollection<AttedaneceMonthRecord> _monthlyAttendanceRecords;
        public ObservableCollection<AttedaneceMonthRecord> MonthlyAttendanceRecords
        {
            get => _monthlyAttendanceRecords;
            set
            {
                _monthlyAttendanceRecords = value;
                OnPropertyChanged();
            }
        }

        private void LoadAttendanceMonthRecords()
        {
            try
            {
                if (SelectedMonthIndex == 0) return; 

                
                var records = _context.Chamcongs
                    .Include(a => a.IdnhanVienNavigation)
                    .Where(a => a.NgayChamCong.Month == SelectedMonthIndex && a.IdnhanVienNavigation.IsDeleted == false)
                    .ToList();

                
                //if (records == null || !records.Any())
                //{
                //    throw new Exception("Không tìm thấy bản ghi nào cho tháng đã chọn.");
                //}

               
                var monthlyRecords = records
                    
                    .GroupBy(a => a.IdnhanVien)
                    .Select(group =>
                    {
                        var firstRecord = group.First();

                       
                        var employee = firstRecord.IdnhanVienNavigation;
                        if (employee == null)
                        {
                            throw new Exception($"Không tìm thấy thông tin nhân viên cho mã nhân viên: {firstRecord.IdnhanVien}");
                        }


                        foreach (var record in group)
                        {
                            if (!record.SoGioLam.HasValue)
                            {
                                throw new Exception($"Bản ghi điểm danh ngày {record.NgayChamCong} của nhân viên {employee.HoTen} (Mã: {employee.Id}) không có giá trị SoGioLam.");
                            }
                        }


                        var totalWorkHours = group.Sum(r => r.SoGioLam.Value);
                        var temporarySalary = (decimal)employee.LuongThang;
                        if (employee.LoaiNhanVien == "Part-time") 
                        { 
                             temporarySalary += totalWorkHours * (decimal)employee.LuongTheoGio; 
                        }


                        return new AttedaneceMonthRecord
                        {
                            MaNhanVien = employee.Id,
                            HoTen = employee.HoTen,
                            ChucVu = employee.CongViec,
                            TongSoGioLamViec = totalWorkHours,
                            LuongTamtinh = temporarySalary
                        };
                    }).ToList();

                
                MonthlyAttendanceRecords = new ObservableCollection<AttedaneceMonthRecord>(monthlyRecords);
            }
            catch (Exception ex)
            {

                MessageBox.Show($"Lỗi khi tải dữ liệu tháng:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        #endregion

        public class AttendanceRecord
        {
            public int MaNhanVien { get; set; }
            public string HoTen { get; set; }
            public string ChucVu { get; set; }
            public TimeOnly GioVao { get; set; }
            public TimeOnly GioRa { get; set; }
            public string GhiChu { get; set; }
            public DateOnly NgayChamCong { get; set; }
        }
        public class AttedaneceMonthRecord
        {
            public string HoTen { get; set; }
            public int MaNhanVien { get; set; }
            public string ChucVu { get; set; }
            public decimal TongSoGioLamViec { get; set; }
            public decimal LuongTamtinh { get; set; }

        }
    }
}
