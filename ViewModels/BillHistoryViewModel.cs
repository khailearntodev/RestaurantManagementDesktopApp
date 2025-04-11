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
    public class BillHistoryViewModel : BaseViewModel
    {
       private readonly QlnhContext _context;
        private ObservableCollection<Hoadon> _listProduct;
        private ObservableCollection<Hoadon> _originalList;
        private string _filterText;
        private int _filterSelectedIndex;
        private DateTime? _selectedDate;
        private int? _selectedMonth;
        private Hoadon _selectedBill;
        public Hoadon SelectedBill
        {
            get => _selectedBill;
            set
            {
                _selectedBill = value;
                OnPropertyChanged();
                if (_selectedBill != null)
                    ShowBillDetailCommand.Execute(_selectedBill);
            }
        }
        public ICommand ShowBillDetailCommand { get; }
        public ICommand Filterbox_SelectionChanged { get; }

        public ObservableCollection<Hoadon> ListProduct
        {
            get => _listProduct;
            set
            {
                _listProduct = value;
                OnPropertyChanged();
            }
        }

        public string FilterText
        {
            get => _filterText;
            set
            {
                _filterText = value;
                ApplyFilter();
                OnPropertyChanged();
            }
        }

        public int FilterSelectedIndex
        {
            get => _filterSelectedIndex;
            set
            {
                if (_filterSelectedIndex != value)
                {
                    _filterSelectedIndex = value;
                    OnPropertyChanged();
                    ApplyFilter();
                }
            }
        }

        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate != value)
                {
                    _selectedDate = value;
                    OnPropertyChanged();
                    ApplyFilter();
                }
            }
        }


        public int? SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                if (_selectedMonth != value)
                {
                    _selectedMonth = value;
                    OnPropertyChanged();
                    ApplyFilter();

                }
            }
        }


        public ICommand RefreshCommand { get; }

        public BillHistoryViewModel()
        {
            _context = new QlnhContext();
            _originalList = new ObservableCollection<Hoadon>();
            ListProduct = new ObservableCollection<Hoadon>();

            RefreshCommand = new RelayCommand<object>(
                canExecute: _ => true,
                execute: _ => Refresh());

            LoadBills();
            ShowBillDetailCommand = new RelayCommand<Hoadon>(
        execute: (selectedBill) =>
        {
            if (selectedBill == null) return;
            var billDetailWindow = new BillDetail
            {
                DataContext = new BillDetailViewModel(selectedBill.Id)
            };

            billDetailWindow.ShowDialog();
        },
        canExecute: (selectedBill) => selectedBill != null);
        }
        private void ShowBillDetail(Hoadon bill)
        {
            System.Diagnostics.Debug.WriteLine($"Selected Bill: {bill.MaHoaDon}");
        }
        private void LoadBills()
        {
            try
            {

                var bills = _context.Hoadons
                    .Include(h => h.IdbanNavigation)
                    .Include(h => h.IdnhanVienNavigation)
                    .OrderByDescending(h => h.NgayHoaDon)
                    .ToList();
                _originalList = new ObservableCollection<Hoadon>(bills);
                ListProduct = new ObservableCollection<Hoadon>(_originalList);
                System.Diagnostics.Debug.WriteLine($"Loaded {ListProduct.Count} bills");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading bills: {ex.Message}");
            }
        }

        private void ApplyFilter()
        {
            if (_originalList == null || !_originalList.Any()) return;

            var filteredList = _originalList.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(FilterText))
            {
                filteredList = filteredList.Where(b => b.MaHoaDon.Contains(FilterText, StringComparison.OrdinalIgnoreCase));
            }

            switch (FilterSelectedIndex)
            {
                case 1:
                    if (SelectedDate.HasValue)
                    {
                        filteredList = filteredList.Where(b => b.NgayHoaDon.Date == SelectedDate.Value.Date);
                    }
                    break;

                case 2:
                    if (SelectedMonth.HasValue)
                    {

                        filteredList = filteredList.Where(b => b.NgayHoaDon.Month == SelectedMonth + 1);
                    }
                    break;

                case 3:
                    filteredList = filteredList.Where(b => b.NgayHoaDon.Year == DateTime.Now.Year);
                    break;
            }


            ListProduct = new ObservableCollection<Hoadon>(filteredList);
        }
        private void Refresh()
        {
            LoadBills();
            ApplyFilter();
        }
    }
}
