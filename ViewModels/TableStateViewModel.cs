using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using OfficeOpenXml;
//using iText.Kernel.Colors;
//using iText.Kernel.Font;
//using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.Diagnostics;
using RestaurantManagement.Models;
using RestaurantManagement.Views;
//using iText.IO.Font;
using Org.BouncyCastle.Crypto;
using System.Windows.Controls;
//using iText.IO.Font.Constants;
//using iText.Commons.Actions.Contexts;
using PdfSharp.Pdf;
using PdfSharp.Drawing;

namespace RestaurantManagement.ViewModels
{
    internal class TableStateViewModel : BaseViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<cTable> _tables;
        public ObservableCollection<cTable> Tables
        {
            get => _tables;
            set
            {
                _tables = value;
                OnPropertyChanged();
            }
        }

        private string _titleOfBill;
        public string TitleOfBill
        {
            get => _titleOfBill;
            set
            {
                _titleOfBill = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<BillItem> _billItems;
        public ObservableCollection<BillItem> BillItems
        {
            get => _billItems;
            set
            {
                _billItems = value;
                CalculateTotalAmount();
                OnPropertyChanged();
            }
        }

        private decimal _totalAmount;
        public decimal TotalAmount
        {
            get => _totalAmount;
            set
            {
                _totalAmount = value;
                OnPropertyChanged();
            }
        }

        private cTable _selectedTable;

        public cTable SelectedTable
        {
            get { return _selectedTable; }
            set
            {
                if (_selectedTable != value)
                {
                    _selectedTable = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<cTable> _emptyTables;
        public ObservableCollection<cTable> EmptyTables
        {
            get => _emptyTables;
            set
            {
                _emptyTables = value;
                OnPropertyChanged();
            }
        }

        private cTable _targetTable;
        public cTable TargetTable
        {
            get => _targetTable;
            set
            {
                _targetTable = value;
                OnPropertyChanged();
                Debug.WriteLine($"TargetTable changed: {TargetTable?.ID}");
            }
        }

        // Commands
        public ICommand ProcessPaymentCommand { get; }
        public ICommand TransferTableCommand { get; }
        public ICommand ShowMenuCommand { get; }

        private readonly QlnhContext _dbContext;

        // Constructor
        public TableStateViewModel()
        {
            _dbContext = new QlnhContext();
            TitleOfBill = SelectedTable != null ? "Hóa đơn bàn " + SelectedTable.ID : "CHỌN 1 BÀN";
            BillItems = new ObservableCollection<BillItem>();
            Tables = new ObservableCollection<cTable>();
            EmptyTables = new ObservableCollection<cTable>();
            LoadTablesFromDatabase();
            ProcessPaymentCommand = new RelayCommand(ProcessPayment, CanExecuteCommands);
            TransferTableCommand = new RelayCommand(TransferTable, CanExecuteCommands);
            ShowMenuCommand = new RelayCommand(ShowMenu, CanShowMenu);
        }

        private void ShowMenu(object parameter)
        {
            var table = parameter as cTable;
            if (table != null && table.TrangThai==true)
            {
                SelectedTable = table;
                LoadBillForSelectedTable(table);
                TitleOfBill = $"Hóa đơn bàn {table.ID}";
            }
        }


        private bool CanShowMenu(object parameter)
        {
            var table = parameter as cTable;
            return table != null && table.TrangThai==true;
        }


        private void LoadTablesFromDatabase()
        {
            try
            {
                var tables = _dbContext.Bans.Select(b => new cTable
                {
                    ID = b.Id,
                    TrangThai = b.TrangThai
                }).ToList();

                Tables.Clear();
                EmptyTables.Clear();
                foreach (var table in tables)
                {
                    Tables.Add(table);
                    if (!table.TrangThai??false)
                    {
                        EmptyTables.Add(table);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu bàn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void LoadBillForSelectedTable(cTable selectedTable)
        {
            var bill = _dbContext.Hoadons
                                  .Where(h => h.Idban == selectedTable.ID && h.IsDeleted == false)
                                  .FirstOrDefault();
            //MessageBox.Show(bill.Idban.ToString() + SelectedTable.ID);
            if (bill != null)
            {
                TitleOfBill = $"Hóa đơn bàn {selectedTable.ID}";
                var billItems = _dbContext.Cthds
                                          .Where(c => c.IdhoaDon == bill.Id)
                                          .Join(_dbContext.Doanuongs, c => c.IddoAnUong, d => d.Id,
                                                (c, d) => new BillItem
                                                {
                                                    Name = d.TenDoAnUong,
                                                    Quantity = c.SoLuong,
                                                    Price = c.GiaMon
                                                }).ToList();

                BillItems.Clear();
                foreach (var item in billItems)
                {
                    BillItems.Add(item);
                }
                CalculateTotalAmount();
            }
            else
            {
                MessageBox.Show("Bàn chưa có hóa đơn.");
                BillItems.Clear();
                TotalAmount = 0;
            }
        }


        private void CalculateTotalAmount()
        {
            TotalAmount = BillItems.Sum(item => item.Quantity * item.Price)??0;
        }

        private void ProcessPayment(object parameter)
        {
            if (SelectedTable == null)
            {
                MessageBox.Show("Vui lòng chọn bàn để thanh toán!");
                return;
            }

            try
            {
                //var result = MessageBox.Show("Bạn có muốn xuất hóa đơn không?", "Xuất hóa đơn", MessageBoxButton.YesNo, MessageBoxImage.Question);
                //if (result == MessageBoxResult.Yes)
                //{
                //    //var saveFileDialog = new SaveFileDialog
                //    //{
                //    //    Filter = "Pdf Files (*.pdf)|*.pdf",
                //    //    FileName = $"HoaDon_Ban{SelectedTable.ID}_Time_{DateTime.Now:yyyyMMddHHmmss}.pdf"
                //    //};

                //    //if ((saveFileDialog.ShowDialog()) == true)
                //    //{
                //    //     Tiến hành xuất hóa đơn ra file Pdf
                //    //    ExportBillToPdf(saveFileDialog.FileName);
                //    //}
                //}
                // Tiến hành thanh toán
                var paybill = _dbContext.Hoadons
                                         .Where(h => h.Idban == SelectedTable.ID && h.IsDeleted == false)
                                         .FirstOrDefault();
                var exbill = _dbContext.Hoadons     
                                         .Where(h => h.Idban == SelectedTable.ID)
                                         .FirstOrDefault();

                if (exbill == null)
                {
                    MessageBox.Show("Bàn chưa có hóa đơn.");
                    BillItems.Clear();
                    TotalAmount = 0;
                    return;
                }

                if (exbill != null)
                {
                    if (paybill != null)
                    {
                        SelectedTable.TrangThai = false;
                        paybill.IsDeleted = true;
                        _dbContext.SaveChanges();

                        var ban = _dbContext.Bans.FirstOrDefault(b => b.Id == SelectedTable.ID);
                        if (ban != null)
                        {
                            ban.TrangThai = false;
                            _dbContext.SaveChanges();
                            SelectedTable.TrangThai = false;
                            OnPropertyChanged(nameof(SelectedTable));
                        }

                        
                        var billDetailWindow = new BillDetail
                        {
                            DataContext = new BillDetailViewModel(paybill.Id)
                        };
                        
                        billDetailWindow.ShowDialog();
                        LoadTablesFromDatabase();
                        BillItems.Clear();
                        TotalAmount = 0;
                        MessageBox.Show("Thanh toán thành công!");
                    }
                }
                else
                {
                    MessageBox.Show("Không có hóa đơn chưa thanh toán.");
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thanh toán: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportBillToPdf(string filePath)
        {
            //var bill = _dbContext.Hoadons
            //                     .Where(h => h.Idban == SelectedTable.ID && h.IsDeleted == false)
            //                     .FirstOrDefault();

            //if (bill != null)
            //{
            //    var billItems = _dbContext.Cthds
            //                              .Where(c => c.IdhoaDon == bill.Id)
            //                              .Join(_dbContext.Doanuongs, c => c.IddoAnUong, d => d.Id,
            //                                    (c, d) => new
            //                                    {
            //                                        Name = d.TenDoAnUong,
            //                                        Quantity = c.SoLuong,
            //                                        Price = c.GiaMon
            //                                    }).ToList();

            //    // Create a new PDF document
            //    PdfDocument pdf = new PdfDocument();
            //    pdf.Info.Title = "Hóa Đơn";

            //    // Create a new page
            //    PdfPage page = pdf.AddPage();
            //    XGraphics gfx = XGraphics.FromPdfPage(page);

            //    // Define font (Make sure the font supports Vietnamese characters, e.g., Arial or Tahoma)
            //    XFont vietnameseFont = new XFont("Arial", 12);
            //    XFont boldFont = new XFont("Arial", 12);

            //    // Set currency format
            //    var vietnameseCurrencyFormat = new NumberFormatInfo
            //    {
            //        CurrencySymbol = "₫",
            //        CurrencyDecimalDigits = 0,
            //        CurrencyGroupSeparator = ".",
            //        CurrencyDecimalSeparator = ","
            //    };

            //    // Draw the title
            //    gfx.DrawString($"HÓA ĐƠN BÀN {SelectedTable.ID}", boldFont, XBrushes.Black,
            //        new XRect(0, 50, page.Width, page.Height), XStringFormats.Center);

            //    // Draw the current date
            //    gfx.DrawString($"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", vietnameseFont, XBrushes.Black,
            //        new XRect(50, 100, page.Width, page.Height), XStringFormats.TopLeft);

            //    // Draw a table-like structure
            //    double yPosition = 150;
            //    gfx.DrawString("Tên món", boldFont, XBrushes.Black, new XRect(50, yPosition, page.Width, page.Height), XStringFormats.TopLeft);
            //    gfx.DrawString("Số lượng", boldFont, XBrushes.Black, new XRect(200, yPosition, page.Width, page.Height), XStringFormats.TopLeft);
            //    gfx.DrawString("Giá", boldFont, XBrushes.Black, new XRect(300, yPosition, page.Width, page.Height), XStringFormats.TopLeft);
            //    gfx.DrawString("Tổng", boldFont, XBrushes.Black, new XRect(400, yPosition, page.Width, page.Height), XStringFormats.TopLeft);

            //    yPosition += 20;

            //    foreach (var item in billItems)
            //    {
            //        gfx.DrawString(item.Name, vietnameseFont, XBrushes.Black, new XRect(50, yPosition, page.Width, page.Height), XStringFormats.TopLeft);
            //        gfx.DrawString(item.Quantity.ToString(), vietnameseFont, XBrushes.Black, new XRect(200, yPosition, page.Width, page.Height), XStringFormats.TopLeft);
            //        //gfx.DrawString(item.Price.ToString("C", vietnameseCurrencyFormat), vietnameseFont, XBrushes.Black, new XRect(300, yPosition, page.Width, page.Height), XStringFormats.TopLeft);
            //        gfx.DrawString((item.Quantity * item.Price).ToString("C", vietnameseCurrencyFormat), vietnameseFont, XBrushes.Black, new XRect(400, yPosition, page.Width, page.Height), XStringFormats.TopLeft);

            //        yPosition += 20;
            //    }

            //    // Draw total amount
            //    decimal totalAmount = billItems.Sum(item => item.Quantity * item.Price);
            //    gfx.DrawString($"Tổng tiền: {totalAmount.ToString("C", vietnameseCurrencyFormat)}", boldFont, XBrushes.Black,
            //        new XRect(50, yPosition, page.Width, page.Height), XStringFormats.TopLeft);

            //    // Save the PDF to file
            //    pdf.Save(filePath);

            //    // Open the file
                
            //    MessageBox.Show($"Hóa đơn đã được lưu vào: {filePath}", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            //}

            //else
            //{
            //    // Handle case where bill is not found
            //    MessageBox.Show("Không tìm thấy hóa đơn.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
        }



        private void TransferTable(object parameter)
        {
            if (parameter is cTable targetTable && SelectedTable != null)
            {
                var bill = _dbContext.Hoadons
                                     .FirstOrDefault(h => h.Idban == SelectedTable.ID && h.IsDeleted == false);

                if (bill != null && targetTable.TrangThai == false)
                {
                    bill.Idban = targetTable.ID;
                    var oldTable = _dbContext.Bans.FirstOrDefault(b => b.Id == SelectedTable.ID);
                    if (oldTable != null)
                    {
                        oldTable.TrangThai = false;
                        _dbContext.Bans.Update(oldTable);
                    }

                    var targetBan = _dbContext.Bans.FirstOrDefault(b => b.Id == targetTable.ID);
                    if (targetBan != null)
                    {
                        targetBan.TrangThai = true;
                        _dbContext.Bans.Update(targetBan);
                    }

                    _dbContext.SaveChanges();

                    LoadTablesFromDatabase();
                    MessageBox.Show($"Chuyển bàn thành công từ bàn {SelectedTable.ID} sang bàn {targetTable.ID}!");
                }
                else
                {
                    MessageBox.Show("Bàn mục tiêu không trống hoặc không có hóa đơn để chuyển.");
                }
            }
            else
            {
                MessageBox.Show("No data!");
            }
        }




        private bool CanExecuteCommands(object parameter)
        {
            return SelectedTable != null;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class BillItem
    {
        public int IDDoAnUong { get; set; }
        public string? Name { get; set; }
        public int? Quantity { get; set; }
        public decimal? Price { get; set; }
    }

    public class cTable
    {
        public int ID { get; set; }
        public bool? TrangThai { get; set; } // Trạng thái: true = đã đặt, false = chưa đặt
    }
}