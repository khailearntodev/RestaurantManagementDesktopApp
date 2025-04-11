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
using System.DirectoryServices;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Windows.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace RestaurantManagement.ViewModels
{
    public class EditIngredientViewModel : BaseViewModel
    {
        private readonly QlnhContext dbContext;
        private readonly Action closeWindowCallback;

        public class Ingredient
        {
            public int importID { get; set; }
            public int ingredientID { get; set; }
            public string? ingredientName { get; set; }
            public DateTime supplyDate { get; set; }
            public string? unit { get; set; }
            public decimal unitPrice { get; set; }
            public int quantity { get; set; }
            public bool type { get; set; } // 0: raw, 1: drink
            public string? supplySource { get; set; }
            public string? phoneNumber { get; set; }
            public int remainQuantity { get; set; }
        }

        private StorageViewModel.Ingredient ingredientToChange;
        public StorageViewModel.Ingredient IngredientToChange
        {
            get => ingredientToChange;
            set
            {
                ingredientToChange = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveIngredientChangeCommand { get; set; }
        public ICommand CancelCommand { get; set; } 

        public EditIngredientViewModel()
        {
            
        }

        public EditIngredientViewModel(QlnhContext _context, StorageViewModel.Ingredient selectedIngredient, Action closeWindow)
        {
            dbContext = _context;
            IngredientToChange = new StorageViewModel.Ingredient
            {
                importID = selectedIngredient.importID,
                ingredientID = selectedIngredient.ingredientID,
                ingredientName = selectedIngredient.ingredientName,
                supplyDate = selectedIngredient.supplyDate,
                unit = selectedIngredient.unit,
                unitPrice = selectedIngredient.unitPrice,
                quantity = selectedIngredient.quantity,
                type = selectedIngredient.type,
                supplySource = selectedIngredient.supplySource, 
                phoneNumber = selectedIngredient.phoneNumber,
                remainQuantity = selectedIngredient.remainQuantity
            };

            SaveIngredientChangeCommand = new RelayCommand(SaveIngredientChange);
            CancelCommand = new RelayCommand(CloseWindow);
            closeWindowCallback = closeWindow;
        }

        private void SaveIngredientChange(object? parameter)
        {
            try
            {
                // Cập nhật bảng NGUYENLIEU
                var ingredientOriginal = dbContext.Nguyenlieus.FirstOrDefault(nl => nl.Id == IngredientToChange.ingredientID);
                if (ingredientOriginal != null)
                {
                    ingredientOriginal.TenNguyenLieu = IngredientToChange.ingredientName;
                    ingredientOriginal.DonVi = IngredientToChange.unit;
                    ingredientOriginal.DonGia = IngredientToChange.unitPrice;
                    ingredientOriginal.Loai = IngredientToChange.type;
                    // Cập nhật CTKHO ---------------
                    dbContext.SaveChanges();
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Không tìm thấy nguyên liệu khi sửa thông tin nguyên liệu");
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Lỗi khi sửa thông tin NGUYÊN LIỆU: {ex.Message}\nChi tiết: {ex.InnerException?.Message}");
            }


            try
            {
                // Cập nhật bảng CTNHAPKHO, với target là hàng thêm vào mới nhất
                var importDetailOriginal = dbContext.Ctnhapkhos.Where(ctnk => ctnk.IdnhapKho == IngredientToChange.importID
                                                                               && ctnk.IdnguyenLieu == IngredientToChange.ingredientID)
                                                                .OrderByDescending(ctnk => ctnk.IdnhapKhoNavigation.NgayNhap)
                                                                .FirstOrDefault();
                if (importDetailOriginal != null)
                {
                    importDetailOriginal.SoLuongNguyenLieu = IngredientToChange.quantity;
                    importDetailOriginal.GiaNguyenLieu = importDetailOriginal.IdnguyenLieuNavigation.DonGia * importDetailOriginal.SoLuongNguyenLieu;
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Không tìm thấy không tìm thấy chi tiết nhập kho khi sửa thông tin nguyên liệu");
                }
            }
            catch(Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Lỗi khi sửa thông tin CTNHAPKHO: {ex.Message}\nChi tiết: {ex.InnerException?.Message}");
            }

            try
            {
                // Cập nhật bảng NHAPKHO
                var importOriginal = dbContext.Nhapkhos.FirstOrDefault(nk => nk.Id == IngredientToChange.importID);
                if (importOriginal != null)
                {
                    importOriginal.NguonNhap = IngredientToChange.supplySource;
                    importOriginal.NgayNhap = IngredientToChange.supplyDate;
                    importOriginal.SdtlienLac = IngredientToChange.phoneNumber;

                    // Cập nhật lại cột GiaNhap trong bảng NHAPKHO thông qua SLNL trong bảng CTNHAPKHO và DonGia trong bảng NGUYENLIEU 
                    var ctnk = dbContext.Ctnhapkhos.Where(ctnk => ctnk.IdnhapKho == IngredientToChange.importID
                                                                   && ctnk.IdnguyenLieu == IngredientToChange.ingredientID)
                                                    .OrderByDescending(ctnk => ctnk.IdnhapKhoNavigation.NgayNhap)
                                                    .FirstOrDefault();
                    if (ctnk != null)
                    {
                        importOriginal.GiaNhap = ctnk.SoLuongNguyenLieu * ctnk.IdnguyenLieuNavigation.DonGia;
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Không tìm thấy chi tiết nhập kho để cập nhật GIANHAP trong NHAPKHO khi sửa thông tin nguyên liệu");
                    }
                    dbContext.SaveChanges();
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Không tìm thấy nhập kho khi sửa thông tin nguyên liệu");
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Lỗi khi sửa thông tin NHAPKHO: {ex.Message}\nChi tiết: {ex.InnerException?.Message}");
            }


            try
            {
                // Cập nhật CTKHO
                var storageDetailOriginal = dbContext.Ctkhos.FirstOrDefault(ctk => ctk.IdnguyenLieu == IngredientToChange.ingredientID);
                if (storageDetailOriginal != null)
                {
                    var calcSumQuantity = dbContext.Ctnhapkhos.GroupBy(ctnk => ctnk.IdnguyenLieu)
                                                              .Select(group => new
                                                              {
                                                                  IDNguyenLieu = group.Key,
                                                                  TongSoLuong = group.Sum(ctnk => ctnk.SoLuongNguyenLieu),
                                                              }).ToDictionary(x => x.IDNguyenLieu, x => x.TongSoLuong);
                    // Cập nhật tồn dư qua số lượng nguyên liệu nhập của lần nhập mới nhất
                    storageDetailOriginal.SoLuongTonDu = calcSumQuantity.ContainsKey(storageDetailOriginal.IdnguyenLieu) ? calcSumQuantity[storageDetailOriginal.IdnguyenLieu] : 0;
                    dbContext.SaveChanges();
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Không tìm thấy CT Kho để cập nhật tồn dư cho nguyên liệu");
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Lỗi khi sửa thông tin CTKHO: {ex.Message}\nChi tiết: {ex.InnerException?.Message}");
            }

            System.Windows.Forms.MessageBox.Show("Lưu thông tin nguyên liệu thành công");
        }

        private void CloseWindow(object? parameter) => closeWindowCallback();
    }
}