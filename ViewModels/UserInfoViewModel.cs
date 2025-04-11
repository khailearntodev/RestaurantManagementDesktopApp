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
using System.Media;
using System.Data;
using System.Windows.Controls;

namespace RestaurantManagement.ViewModels
{
    public class UserInfoViewModel : BaseViewModel
    {
        private readonly QlnhContext dbContext;

        private Nhanvien nhanVien;
        public Nhanvien NhanVien
        {
            get => nhanVien;
            set
            {
                nhanVien = value;
                OnPropertyChanged();
            }
        }

        public UserInfoViewModel(string MaNV)
        {
            dbContext = new QlnhContext();

            NhanVien = dbContext.Nhanviens.FirstOrDefault(tk => tk.MaNhanVien == MaNV);
        }
    }
}