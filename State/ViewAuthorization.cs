using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RestaurantManagement.ViewModels;

namespace RestaurantManagement.State
{
    public class ViewAuthorization : BaseViewModel
    { 
        public ViewAuthorization() { }

        public ViewAuthorization(int role) // 0 : admin, 1 : employee
        {
            GetRole(role);
        }

        private bool adminRole;
        private Visibility adminView;
        private Visibility employeeView;

        public bool AdminRole
        {
            get
            {
                return adminRole;
            }
            set
            {
                adminRole = value;
                OnPropertyChanged();
            }
        }

        public Visibility AdminView
        {
            get
            {
                return adminView;
            }
            set
            {
                adminView = value;
                OnPropertyChanged();
            }
        }

        public Visibility EmployeeView
        {
            get
            {
                return employeeView;
            }
            set
            {
                employeeView = value; 
                OnPropertyChanged();
            }
        }
        
        public void GetRole(int role)
        {
            if (role == 0) // Admin
                AdminRole = true;
            else           // Employee
                AdminRole = false;

            SetVisibility(AdminRole);
        }

        public void SetVisibility(bool isAdmin)
        {
            if (isAdmin) 
            {
                AdminView = Visibility.Visible;
                EmployeeView = Visibility.Collapsed;
            }
            else
            {
                AdminView = Visibility.Collapsed;
                EmployeeView = Visibility.Visible;
            }
        }
    }
}