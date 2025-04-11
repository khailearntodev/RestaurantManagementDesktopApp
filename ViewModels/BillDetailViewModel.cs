using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using RestaurantManagement.Models;
using RestaurantManagement.Views;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Font;
using iText.Kernel.Font;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto;
using iText.Kernel.Colors;
using System.Windows.Controls;
using iText.IO.Image;
using VietQRHelper;

namespace RestaurantManagement.ViewModels
{
    public class BillDetailViewModel : BaseViewModel
    {
        private QlnhContext _context;
        private string _employee;
        public string Employee
        {
            get => _employee;
            set
            {
                _employee = value;
                OnPropertyChanged();
            }
        }
        private int _billId;
        public int BillId
        {
            get => _billId;
            set
            {
                _billId = value;
                OnPropertyChanged();
            }
        }

        private string _customerName;
        public string CustomerName
        {
            get => _customerName;
            set
            {
                _customerName = value;
                OnPropertyChanged();
            }
        }

        private DateTime _billDate;
        public DateTime BillDate
        {
            get => _billDate;
            set
            {
                _billDate = value;
                OnPropertyChanged();
            }
        }

        private decimal _totalPrice;
        public decimal TotalPrice
        {
            get => _totalPrice;
            set
            {
                _totalPrice = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<BillDetailModel> BillDetails { get; set; }

        public ICommand PrintCommand { get; set; }
        public ICommand FinishCommand { get; set; }
        public BillDetailViewModel(int billId)
        {
            _context = new QlnhContext();
            BillDetails = new ObservableCollection<BillDetailModel>();
            BillId = billId;
            PrintCommand = new RelayCommand<object>(canExecute: _ => true, execute: ExecutePrint);
            FinishCommand = new RelayCommand<object>(canExecute: _ => true, execute: Finish);
            LoadBillDetails();
        }
        private void Finish(object parameter)
        {
            if (parameter is Window window)
            {
                window.Close();
            }
        }
        private void LoadBillDetails()
        {
            var bill = _context.Hoadons
                    .Include(b => b.Cthds)
                    .ThenInclude(c => c.IddoAnUongNavigation)
                    .Include(b => b.IdbanNavigation)
                    .Include(b => b.IdnhanVienNavigation)
                    .FirstOrDefault(b => b.Id == BillId);

            if (bill != null)
            {
                CustomerName = bill.IdbanNavigation?.MaBan ?? "Không rõ khách hàng";
                BillDate = bill.NgayHoaDon;
                TotalPrice = bill.TongGia ?? 0m;
                Employee = bill.IdnhanVienNavigation?.HoTen ?? "Không rõ nhân viên";
                foreach (var cthd in bill.Cthds)
                {
                    var billDetail = new BillDetailModel
                    {
                        ItemName = cthd.IddoAnUongNavigation?.TenDoAnUong ?? "Chưa có tên món",
                        Quantity = cthd.SoLuong,
                        UnitPrice = cthd.GiaMon,
                        Price = cthd.SoLuong * cthd.GiaMon
                    };

                    BillDetails.Add(billDetail);
                }
            }
        }

        private void ExecutePrint(object parameter)
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"HoaDon_{BillId}.pdf");

            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                PdfWriter writer = new PdfWriter(fs);
                PdfDocument pdfDoc = new PdfDocument(writer);
                Document document = new Document(pdfDoc);

                PdfFont vietnameseFont = PdfFontFactory.CreateFont("c:/windows/fonts/arial.ttf", PdfEncodings.IDENTITY_H);
                PdfFont boldFont = PdfFontFactory.CreateFont("C:/Windows/Fonts/arialbd.ttf", PdfEncodings.IDENTITY_H);

                var vietnameseCurrencyFormat = new System.Globalization.CultureInfo("vi-VN");

                // Tiêu đề
                document.Add(new Paragraph("HÓA ĐƠN")
                    .SetFont(boldFont)
                    .SetFontSize(24)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetMarginBottom(20));

                // Thông tin hóa đơn
                document.Add(new Paragraph($"Mã hóa đơn: {BillId}")
                    .SetFont(vietnameseFont).SetFontSize(12).SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT));

                document.Add(new Paragraph($"Mã số bàn: {CustomerName}")
                    .SetFont(vietnameseFont).SetFontSize(12).SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT));

                document.Add(new Paragraph($"Ngày lập hóa đơn: {BillDate.ToString("dd/MM/yyyy")}")
                    .SetFont(vietnameseFont).SetFontSize(12).SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT));

                // Tạo bảng
                Table table = new Table(4).UseAllAvailableWidth();

                // Header bảng
                table.AddHeaderCell(new Cell().Add(new Paragraph("Tên món").SetFont(boldFont).SetFontSize(12).SetFontColor(ColorConstants.WHITE))
                    .SetBackgroundColor(ColorConstants.DARK_GRAY).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Số lượng").SetFont(boldFont).SetFontSize(12).SetFontColor(ColorConstants.WHITE))
                    .SetBackgroundColor(ColorConstants.DARK_GRAY).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Đơn giá").SetFont(boldFont).SetFontSize(12).SetFontColor(ColorConstants.WHITE))
                    .SetBackgroundColor(ColorConstants.DARK_GRAY).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Thành tiền").SetFont(boldFont).SetFontSize(12).SetFontColor(ColorConstants.WHITE))
                    .SetBackgroundColor(ColorConstants.DARK_GRAY).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                // Thêm dữ liệu vào bảng
                foreach (var detail in BillDetails)
                {
                    table.AddCell(new Cell().Add(new Paragraph(detail.ItemName).SetFont(vietnameseFont).SetFontSize(12))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT));
                    table.AddCell(new Cell().Add(new Paragraph(detail.Quantity.ToString()).SetFont(vietnameseFont).SetFontSize(12))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));
                    table.AddCell(new Cell().Add(new Paragraph(string.Format(vietnameseCurrencyFormat, "{0:C0}", detail.UnitPrice)).SetFont(vietnameseFont).SetFontSize(12))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));
                    table.AddCell(new Cell().Add(new Paragraph(string.Format(vietnameseCurrencyFormat, "{0:C0}", detail.Price)).SetFont(vietnameseFont).SetFontSize(12))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));
                }

                // Thêm bảng vào tài liệu
                document.Add(table.SetMarginBottom(20));

                // Tổng giá
                document.Add(new Paragraph($"Tổng giá: {string.Format(vietnameseCurrencyFormat, "{0:C0}", TotalPrice)}")
                    .SetFont(boldFont).SetFontSize(14).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetPadding(10));

                // Thêm mã QR
                var qrPay = QRPay.InitVietQR(
                    bankBin: BankApp.BanksObject[BankKey.VIETCOMBANK].bin, //ngan hang nhan tien
                    bankNumber: "1038172542", //tai khoan nhan tien
                    amount: TotalPrice.ToString(),
                    purpose: $"Thanh toan ban {CustomerName}"
                );

                var content = qrPay.Build();
                var imageQR = QRCodeHelper.TaoVietQRCodeImage(content);

                // Convert Bitmap to ImageData
                using (var ms = new MemoryStream())
                {
                    imageQR.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ImageData qrImageData = ImageDataFactory.Create(ms.ToArray());
                    iText.Layout.Element.Image qrImage = new iText.Layout.Element.Image(qrImageData).ScaleToFit(100, 100);

                    // Add QR Code to the document
                    document.Add(new Paragraph("Quét mã QR để thanh toán:")
                        .SetFont(vietnameseFont)
                        .SetFontSize(12)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetMarginTop(20));

                    document.Add(qrImage.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER));
                }

                // Lời cảm ơn
                document.Add(new Paragraph("\n***** Rất hân hạnh được phục vụ quý khách. *****")
                    .SetFont(boldFont)
                    .SetFontSize(12)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetMarginTop(30));

                document.Close();
            }

            // Thông báo khi hoàn thành
            MessageBox.Show($"Hóa đơn đã được xuất ra file PDF tại: {filePath}", "Xuất Hóa Đơn", MessageBoxButton.OK, MessageBoxImage.Information);
        }


    }

    public class BillDetailModel
    {
        public string? ItemName { get; set; }
        public int? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? Price { get; set; }
    }
}