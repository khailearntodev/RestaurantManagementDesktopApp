using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;
using System.Windows;

namespace RestaurantManagement.ViewModels
{
    public class StatisticsViewModel : BaseViewModel
    {
        private readonly QlnhContext _context;
        private ObservableCollection<int> _months;
        private ObservableCollection<int> _years;
        private int _selectedMonth;
        private int _selectedYear;
        private SeriesCollection _chartSeries;
        private string[] _xAxisLabels;
        private decimal _totalIncome;
        private decimal _totalExpense;
        private bool _isDailyMode = true;
        public Dictionary<int, decimal> TotalMonthlySalaries { get; set; }

        public StatisticsViewModel()
        {
            _context = new QlnhContext();
            InitializeCollections();
            InitializeCommands();
            InitializeChart();
        }

        public ObservableCollection<int> Months
        {
            get => _months;
            set
            {
                _months = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<int> Years
        {
            get => _years;
            set
            {
                _years = value;
                OnPropertyChanged();
            }
        }

        public int SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                _selectedMonth = value;
                OnPropertyChanged();
                UpdateStatistics();
            }
        }

        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                _selectedYear = value;
                OnPropertyChanged();
                UpdateStatistics();
            }
        }

        public SeriesCollection ChartSeries
        {
            get => _chartSeries;
            set
            {
                _chartSeries = value;
                OnPropertyChanged();
            }
        }

        public string[] XAxisLabels
        {
            get => _xAxisLabels;
            set
            {
                _xAxisLabels = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalIncome
        {
            get => _totalIncome;
            set
            {
                _totalIncome = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalExpense
        {
            get => _totalExpense;
            set
            {
                _totalExpense = value;
                OnPropertyChanged();
            }
        }

        public ICommand ShowDailyStatisticsCommand { get; private set; }
        public ICommand ShowMonthlyStatisticsCommand { get; private set; }

        private void InitializeCollections()
        {
            Months = new ObservableCollection<int>();
            for (int i = 1; i <= 12; i++)
            {
                Months.Add(i);
            }

            Years = new ObservableCollection<int>();
            int currentYear = DateTime.Now.Year;
            for (int i = 0; i < 5; i++)
            {
                Years.Add(currentYear - i);
            }

            SelectedMonth = DateTime.Now.Month;
            SelectedYear = currentYear;
        }

        private void InitializeCommands()
        {
            ShowDailyStatisticsCommand = new RelayCommand<object>(
                (p) => true,
                (p) =>
                {
                    _isDailyMode = true;
                    ShowDailyStatistics();
                }
            );

            ShowMonthlyStatisticsCommand = new RelayCommand<object>(
                (p) => true,
                (p) =>
                {
                    _isDailyMode = false;
                    ShowMonthlyStatistics();
                }
            );
        }

        private void InitializeChart()
        {
            ChartSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Thu",
                    Values = new ChartValues<decimal>(),
                    LineSmoothness = 0
                },
                new LineSeries
                {
                    Title = "Chi",
                    Values = new ChartValues<decimal>(),
                    LineSmoothness = 0
                }
            };

            XAxisLabels = Array.Empty<string>();
        }

        private void UpdateStatistics()
        {
            if (_isDailyMode)
                ShowDailyStatistics();
            else
                ShowMonthlyStatistics();
        }

        private async void ShowDailyStatistics()
        {
            try
            {
                var daysInMonth = DateTime.DaysInMonth(SelectedYear, SelectedMonth);
                var startDate = new DateTime(SelectedYear, SelectedMonth, 1);
                var endDate = startDate.AddMonths(1);

                var dailyIncomes = await _context.Hoadons
                    .Where(h => h.NgayHoaDon >= startDate &&
                               h.NgayHoaDon < endDate)
                    .GroupBy(h => h.NgayHoaDon.Day)
                    .Select(g => new
                    {
                        Day = g.Key,
                        Total = g.Sum(h => h.TongGia)
                  
                    })
                    .ToDictionaryAsync(x => x.Day, x => x.Total);

                var dailyExpenses = await _context.Nhapkhos
                    .Where(n => n.NgayNhap.HasValue && n.NgayNhap.Value >= startDate && n.NgayNhap.Value < endDate)
                    .GroupBy(n => n.NgayNhap.Value.Day)
                    .Select(g => new
                    {
                        Day = g.Key,
                        Total = g.Sum(n => n.GiaNhap ?? 0) // Xử lý nullable
                    })
                    .ToDictionaryAsync(x => x.Day, x => x.Total);


                XAxisLabels = Enumerable.Range(1, daysInMonth)
                    .Select(d => d.ToString())
                    .ToArray();

                var incomeValues = new ChartValues<decimal>();
                var expenseValues = new ChartValues<decimal>();

                for (int day = 1; day <= daysInMonth; day++)
                {
                    incomeValues.Add(dailyIncomes.ContainsKey(day) ? dailyIncomes[day] ?? 0 : 0);
                    expenseValues.Add(dailyExpenses.ContainsKey(day) ? dailyExpenses[day] : 0);
                }

   
                ChartSeries[0].Values = incomeValues;
                ChartSeries[1].Values = expenseValues;


                TotalIncome = incomeValues.Sum();
                TotalExpense = expenseValues.Sum();
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error updating daily statistics: {ex.Message}");
            }
        }


        private async void ShowMonthlyStatistics()
        {
            try
            {
                var startDate = new DateTime(SelectedYear, 1, 1);
                var endDate = startDate.AddYears(1);

                var monthlyIncomes = await _context.Hoadons
                    .Where(h => h.NgayHoaDon >= startDate &&
                                h.NgayHoaDon < endDate)
                    .GroupBy(h => h.NgayHoaDon.Month)
                    .Select(g => new
                    {
                        Month = g.Key,
                        Total = g.Sum(h => h.TongGia)
                    })
                    .ToDictionaryAsync(x => x.Month, x => x.Total);


                var monthlyExpenses = await _context.Nhapkhos
                    .Where(n => n.NgayNhap.HasValue &&
                                n.NgayNhap.Value >= startDate &&
                                n.NgayNhap.Value < endDate)
                    .GroupBy(n => n.NgayNhap.Value.Month)
                    .Select(g => new
                    {
                        Month = g.Key,
                        Total = g.Sum(n => n.GiaNhap)
                    })
                    .ToDictionaryAsync(x => x.Month, x => x.Total);


                Dictionary<int, decimal> employeeSalaries = new Dictionary<int, decimal>();

                try
                {
                    var records = await _context.Chamcongs
                        .Include(a => a.IdnhanVienNavigation)
                        .Where(a => a.NgayChamCong.Year == SelectedYear && a.IdnhanVienNavigation.IsDeleted == false)
                        .ToListAsync();

                    if (records == null || !records.Any())
                    {
                        throw new Exception("Không tìm thấy bản ghi nào cho năm đã chọn.");
                    }

                    employeeSalaries = records
                        .GroupBy(a => a.NgayChamCong.Month)
                        .Select(group =>
                        {
                            var month = group.Key;
                            var totalWorkHours = group.Sum(r => r.SoGioLam ?? 0);

                            var employees = group.Select(r => r.IdnhanVienNavigation).Distinct();
                            decimal totalSalary = 0;

                            foreach (var employee in employees)
                            {
                                if (employee != null)
                                {
                                    var salary = employee.LoaiNhanVien == "Part-time"
                                        ? totalWorkHours * (decimal)employee.LuongTheoGio
                                        : (decimal)employee.LuongThang;

                                    totalSalary += salary;
                                }
                            }

                            return new { Month = month, TotalSalary = totalSalary };
                        })
                        .ToDictionary(x => x.Month, x => x.TotalSalary);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi tính toán lương nhân viên:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }


                TotalMonthlySalaries = employeeSalaries;

                foreach (var month in employeeSalaries.Keys)
                {
                    if (monthlyExpenses.ContainsKey(month))
                    {
                        monthlyExpenses[month] += employeeSalaries[month];
                    }
                    else
                    {
                        monthlyExpenses[month] = employeeSalaries[month];
                    }
                }


                XAxisLabels = new[] { "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12" };


                var incomeValues = new ChartValues<decimal>();
                var expenseValues = new ChartValues<decimal>();

                for (int month = 1; month <= 12; month++)
                {
                    incomeValues.Add(monthlyIncomes.ContainsKey(month) ? monthlyIncomes[month] ?? 0 : 0 );
                    expenseValues.Add(monthlyExpenses.ContainsKey(month) ? monthlyExpenses[month] ?? 0 : 0);

                }


                ChartSeries[0].Values = incomeValues;
                ChartSeries[1].Values = expenseValues;


                TotalIncome = incomeValues.Sum();
                TotalExpense = expenseValues.Sum();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating monthly statistics: {ex.Message}");
            }
        }




    }
}