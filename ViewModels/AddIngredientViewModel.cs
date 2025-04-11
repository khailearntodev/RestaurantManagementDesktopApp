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

namespace RestaurantManagement.ViewModels
{
    public class AddIngredientViewModel : BaseViewModel
    {
        private readonly QlnhContext dbContext; // Manipulating database

        public ICommand PowerOffAddIngredientViewCommand { get; set; }
        public ICommand SaveDishIngredientCommand { get; set; }
        public ICommand AddIngredientsToDishCommand { get; set; }
        public ICommand RemoveIngredientFromDishCommand { get; set; }

        public class IngredientViewModel
        {
            public string? TenNguyenLieu { get; set; }
            public string? DonVi { get; set; }
            public decimal? DonGia { get; set; }
            public int? TonDu { get; set; }
        }

        public class DishDetails 
        {
            public int IdDoAnUong { get; set; }
            public int IdNguyenLieu { get; set; } 
            public string? TenNguyenLieu { get; set; }   
            public int? SoLuongNguyenLieu { get; set; }
            public DishDetails() { }
            public DishDetails(string tenNL, int SL)
            {
                TenNguyenLieu = tenNL;
                SoLuongNguyenLieu = SL;
            }
        }

        public Doanuong SelectedDish_Ingredient { get; set; }

        private string ingredientFilterText;
        public string IngredientFilterText
        {
            get => ingredientFilterText;
            set
            {
                ingredientFilterText = value;
                OnPropertyChanged(nameof(IngredientFilterText));
                RawIngredient_Filter();
                DrinkIngredient_Filter();
            }
        }

        private ObservableCollection<DishDetails> deletedIngredients;
        public ObservableCollection<DishDetails> DeletedIngredients
        {
            get => deletedIngredients;
            set
            {
                deletedIngredients = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<DishDetails> addedIngredients;
        public ObservableCollection<DishDetails> AddedIngredients
        {
            get => addedIngredients;
            set
            {
                addedIngredients = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<DishDetails> ingredientOfDish;
        public ObservableCollection<DishDetails> IngredientOfDish
        {
            get => ingredientOfDish;
            set
            {
                ingredientOfDish = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<IngredientViewModel> rawIngredient;
        public ObservableCollection<IngredientViewModel> RawIngredient
        {
            get => rawIngredient;
            set
            {
                rawIngredient = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<IngredientViewModel> displayRawIngredient;
        public ObservableCollection<IngredientViewModel> DisplayRawIngredient
        {
            get => displayRawIngredient;
            set
            {
                displayRawIngredient = value;
                OnPropertyChanged();
            }
        }

        private IngredientViewModel selectedRawIngredient;
        public IngredientViewModel SelectedRawIngredient
        {
            get => selectedRawIngredient;
            set
            {
                selectedRawIngredient = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<IngredientViewModel> drinkIngredient;
        public ObservableCollection<IngredientViewModel> DrinkIngredient
        {
            get => drinkIngredient;
            set
            {
                drinkIngredient = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<IngredientViewModel> displayDrinkIngredient;
        public ObservableCollection<IngredientViewModel> DisplayDrinkIngredient
        {
            get => displayDrinkIngredient;
            set
            {
                displayDrinkIngredient = value;
                OnPropertyChanged();
            }
        }

        private IngredientViewModel selectedDrinkIngredient;
        public IngredientViewModel SelectedDrinkIngredient
        {
            get => selectedDrinkIngredient;
            set
            {
                selectedDrinkIngredient = value;
                OnPropertyChanged();
            }
        }

        public AddIngredientViewModel()
        {
            dbContext = new QlnhContext();
            try
            {
                RawIngredient = new ObservableCollection<IngredientViewModel>
                (
                    dbContext.Ctkhos
                    .Where(ctk => ctk.Idkho == 1 && ctk.IsDeleted == false)
                    .Select(ctk => new IngredientViewModel
                    {
                        TenNguyenLieu = ctk.IdnguyenLieuNavigation.TenNguyenLieu,
                        DonVi = ctk.IdnguyenLieuNavigation.DonVi,
                        DonGia = ctk.IdnguyenLieuNavigation.DonGia,
                        TonDu = ctk.SoLuongTonDu
                    })
                    .ToList()
                );
                DisplayRawIngredient = new ObservableCollection<IngredientViewModel>(RawIngredient);

                DrinkIngredient = new ObservableCollection<IngredientViewModel>
                (
                    dbContext.Ctkhos
                    .Where(ctk => ctk.Idkho == 2 && ctk.IsDeleted == false)
                    .Select(ctk => new IngredientViewModel
                    {
                        TenNguyenLieu = ctk.IdnguyenLieuNavigation.TenNguyenLieu,
                        DonVi = ctk.IdnguyenLieuNavigation.DonVi,
                        DonGia = ctk.IdnguyenLieuNavigation.DonGia,
                        TonDu = ctk.SoLuongTonDu
                    })
                    .ToList()
                );
                DisplayDrinkIngredient = new ObservableCollection<IngredientViewModel>(DrinkIngredient);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Lỗi khi tải nguyên liệu cho chi tiết món ăn: {ex.Message}\nChi tiết: {ex.InnerException?.Message}");
            }

            PowerOffAddIngredientViewCommand = new RelayCommand(parameter =>
            {
                if (parameter is Window win)
                {
                    PowerOffAddIngredientView(win);
                }
            });

            AddIngredientsToDishCommand = new RelayCommand<object>(CanExecuteAddIngredientsToDish, ExecuteAddIngredientsToDish);
            RemoveIngredientFromDishCommand = new RelayCommand(parameter =>
            {
                if (parameter is DishDetails dt)
                {
                    RemoveIngredientFromDish(dt);
                }
            });

            SaveDishIngredientCommand = new RelayCommand<object>(CanExecuteSaveDishIngredient, ExecuteSaveIngredient);
        }

        private void PowerOffAddIngredientView(Window p)
        {
            p.Close();
            //DeletedIngredients.Clear();
        }

        private bool CanExecuteAddIngredientsToDish(object? parameter)
        {
            if (SelectedDrinkIngredient == null && SelectedRawIngredient == null)
                return false;
            return true;
        }

        private void ExecuteAddIngredientsToDish(object? parameter)
        {
            string? ingredientName = SelectedDrinkIngredient?.TenNguyenLieu
                             ?? SelectedRawIngredient?.TenNguyenLieu;
            if (ingredientName == null)
                return;
            if (IsListedInIngredientList(ingredientName))
            {
                System.Windows.Forms.MessageBox.Show("Nguyên liệu này đã được thêm!");
                return;
            }

            //var newIngredient = new DishDetails(ingredientName, 0);
            //IngredientOfDish.Add(newIngredient);
            var newIngredient = dbContext.Nguyenlieus.FirstOrDefault(d => d.TenNguyenLieu == ingredientName);
            if (newIngredient != null)
            {
                IngredientOfDish.Add(new DishDetails
                {
                    IdDoAnUong = SelectedDish_Ingredient.Id,
                    IdNguyenLieu = newIngredient.Id,
                    TenNguyenLieu = newIngredient.TenNguyenLieu,
                    SoLuongNguyenLieu = 0
                });
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Nguyên liệu mới trả về null");
                return;
            }

            var ingredientInDeleted = DeletedIngredients.FirstOrDefault(d => d.IdNguyenLieu == newIngredient.Id);
            if (ingredientInDeleted != null)
                DeletedIngredients.Remove(ingredientInDeleted);
            else
            {
                var add = dbContext.Nguyenlieus.FirstOrDefault(d => d.TenNguyenLieu == newIngredient.TenNguyenLieu);
                if (add != null)
                {
                    AddedIngredients.Add(new DishDetails
                    {
                        IdDoAnUong = SelectedDish_Ingredient.Id,
                        IdNguyenLieu = add.Id,
                        TenNguyenLieu = add.TenNguyenLieu,
                        SoLuongNguyenLieu = 0
                    });
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Nguyên liệu chưa được thêm đã không tìm thấy trong CSDL!");
                    return;
                }
            }
        }

        private void RemoveIngredientFromDish(DishDetails dt)
        {
            try
            {
                IngredientOfDish.Remove(dt);

                var ingredientInAdded = AddedIngredients.FirstOrDefault(a => a.IdNguyenLieu == dt.IdNguyenLieu);
                if (ingredientInAdded != null)
                    AddedIngredients.Remove(ingredientInAdded);
                else DeletedIngredients.Add(dt);
            }
            catch (Exception ex) 
            {
                System.Windows.Forms.MessageBox.Show($"Lỗi khi xóa {ex.Message}");
            }
        }

        private bool CanExecuteSaveDishIngredient(object? parameter)
        {
            return true;
        }

        private void ExecuteSaveIngredient(object? parameter)
        {
            try
            {
                if (IngredientOfDish.Count == 0)
                {
                    var result = System.Windows.Forms.MessageBox.Show
                    (
                        $"Hiện tại {SelectedDish_Ingredient.TenDoAnUong} chưa có nguyên liệu nào, bạn phải thêm nguyên liệu sau này?",
                        "Xác nhận lưu",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question
                    );

                    if (result == DialogResult.OK)
                    {
                        var ingredientsToDelete = dbContext.Ctmonans
                            .Where(ctma => ctma.IddoAnUong == SelectedDish_Ingredient.Id);
                        dbContext.Ctmonans.RemoveRange(ingredientsToDelete);
                        dbContext.SaveChanges();

                        System.Windows.Forms.MessageBox.Show("Lưu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    foreach (var addedIngredients in IngredientOfDish)
                    {
                        var ingredient = dbContext.Nguyenlieus
                            .FirstOrDefault(nl => nl.Id == addedIngredients.IdNguyenLieu);

                        if (ingredient != null)
                        {
                            var existingCtmonan = dbContext.Ctmonans
                                .FirstOrDefault(ctma => ctma.IddoAnUong == SelectedDish_Ingredient.Id &&
                                                ctma.IdnguyenLieu == ingredient.Id);
                            
                            if (existingCtmonan != null)
                            {
                                var findCtma = IngredientOfDish.FirstOrDefault(ctma => ctma.IdNguyenLieu == existingCtmonan.IdnguyenLieu
                                                                               && ctma.IdDoAnUong == existingCtmonan.IddoAnUong);
                                if (findCtma != null)
                                {
                                    existingCtmonan.SoLuongNguyenLieu = findCtma.SoLuongNguyenLieu;
                                    dbContext.Ctmonans.Update(existingCtmonan);
                                }
                                else
                                {
                                    System.Windows.Forms.MessageBox.Show("Không tìm thấy nguyên liệu khi cập nhật số lượng");
                                    return;
                                }
                            }
                            else
                            {
                                dbContext.Ctmonans.Add(new Ctmonan
                                {
                                    IddoAnUong = SelectedDish_Ingredient.Id,
                                    IdnguyenLieu = ingredient.Id,
                                    SoLuongNguyenLieu = addedIngredients.SoLuongNguyenLieu
                                });
                                //System.Windows.Forms.MessageBox.Show($"Không thực hiện cập nhật, chỉ thêm");
                            }
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show("Ingredient bị null rồi_Added", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }

                    foreach (var deletedIngredients in DeletedIngredients)
                    {
                        var ingredient = dbContext.Nguyenlieus
                            .FirstOrDefault(nl => nl.Id == deletedIngredients.IdNguyenLieu);

                        if (ingredient != null)
                        {
                            var ingredientToRemove = dbContext.Ctmonans
                                .FirstOrDefault(ctma => ctma.IddoAnUong == SelectedDish_Ingredient.Id &&
                                                ctma.IdnguyenLieu == ingredient.Id);
                            if (ingredientToRemove != null)
                            {
                                dbContext.Ctmonans.Remove(ingredientToRemove);
                            }
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show("Ingredient bị null rồi__Deleted", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }

                    dbContext.SaveChanges();
                    AddedIngredients.Clear();
                    DeletedIngredients.Clear();
                    System.Windows.Forms.MessageBox.Show("Lưu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Lỗi khi lưu các nguyên liệu cần thiết cho món ăn {ex.Message}");
            }
        }

        public bool IsListedInIngredientList(string TenNL)
        {
            if (IngredientOfDish.Count == 0)
                return false;

            foreach (var ctm in IngredientOfDish)
            {
                if (ctm.TenNguyenLieu.CompareTo(TenNL) == 0)
                    return true;
            }
            return false;
        }

        public void ResetFilterText()
        {
            IngredientFilterText = string.Empty;
        }

        public void RawIngredient_Filter()
        {
            if (RawIngredient == null || !RawIngredient.Any()) return;
            var rawFiltered = RawIngredient.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(IngredientFilterText))
            {
                rawFiltered = rawFiltered.Where(u => u.TenNguyenLieu.Contains(IngredientFilterText, StringComparison.OrdinalIgnoreCase));
            }

            DisplayRawIngredient = new ObservableCollection<IngredientViewModel>(rawFiltered);
        }

        public void DrinkIngredient_Filter()
        {
            if (DrinkIngredient == null || !DrinkIngredient.Any()) return;
            var drinkFiltered = DrinkIngredient.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(IngredientFilterText))
            {
                drinkFiltered = drinkFiltered.Where(u => u.TenNguyenLieu.Contains(IngredientFilterText, StringComparison.OrdinalIgnoreCase));
            }

            DisplayDrinkIngredient = new ObservableCollection<IngredientViewModel>(drinkFiltered);
        }
    }
}