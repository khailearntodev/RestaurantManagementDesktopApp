using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using RestaurantManagement.Models;
using System.Windows;
using System.Text.RegularExpressions;
using RestaurantManagement.ViewModels;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace RestaurantManagement.ViewModels
{
    public class LoginWindowViewModel : BaseViewModel
    {
        private string _username;
        private string _password;
        private string _errorMessage;
        public string MANV { get; set; }
        private readonly QlnhContext _context;
        private int _role;

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
                (LoginCommand as RelayCommand)?.RaiseCanExecuteChanged();  // Use the non-generic RelayCommand
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
                (LoginCommand as RelayCommand)?.RaiseCanExecuteChanged();  // Use the non-generic RelayCommand
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public int Role
        {
            get => _role;
            set
            {
                _role = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoginCommand { get; set; }

        public LoginWindowViewModel()
        {
            _context = new QlnhContext();

            Username = "";
            Password = "";
            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
        }

        private bool CanExecuteLogin(object? parameter)
        {
            return !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);
        }

        private void ExecuteLogin(object? parameter)
        {
            if (!IsValidInput(Username) || !IsValidInput(Password))
            {
                ErrorMessage = "Tên đăng nhập và mật khẩu không bao gồm ký tự đặc biệt";
                OnPropertyChanged();
                return;
            }
            try
            {
                var user = _context.Taikhoans.Include(t => t.IdnhanVienNavigation)
                    .FirstOrDefault(t => t.TenTaiKhoan == Username && t.MatKhau == Password);
                if (user != null)
                {
                    Role = (user.PhanQuyen == 0) ? 0 : 1;
                    ErrorMessage = "";
                    MANV = user.IdnhanVienNavigation.MaNhanVien;


                    // Đóng window login và set DialogResult
                    if (parameter is Window window)
                    {
                        window.DialogResult = true;
                        window.Close();
                    }
                }
                else
                {
                    ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng!";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Đã xảy ra lỗi: {ex.Message}";
            }
        }

        private bool IsValidInput(string input)
        {
            var regex = new Regex("^[a-zA-Z0-9_-]+$");
            return regex.IsMatch(input);
        }
    }
}