using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using RestaurantManagement.Command;
using RestaurantManagement.ViewModels;
using RestaurantManagement.State;

namespace RestaurantManagement.State.Navigator
{
    public class Navigator : ObservableObject, INavigator
    {
        private Visibility adminView;
        private Visibility employeeView;
        private ViewAuthorization verifiedAuthorization;
        private BaseViewModel currentViewModel;
        private string currentTitle;

        public Visibility AdminView
        {
            get
            {
                return adminView;
            }
            set
            {
                adminView = value;
                OnPropertyChanged(nameof(adminView));
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
                OnPropertyChanged(nameof(employeeView));
            }
        }

        public ViewAuthorization VerifiedAuthorization
        {
            get
            {
                return verifiedAuthorization; 
            }
            set
            {
                verifiedAuthorization = value;
                OnPropertyChanged(nameof(verifiedAuthorization));
            }
        }

        public BaseViewModel CurrentViewModel
        {
            get
            {
                return currentViewModel;
            }
            set
            {
                currentViewModel = value;
                OnPropertyChanged(nameof(currentViewModel));
            }
        }

        public string CurrentTitle
        {
            get
            {
                return currentTitle;
            }
            set
            {
                currentTitle = value;
                OnPropertyChanged(nameof(currentTitle));
            }
        }

        public Navigator(int role) // 0 : admin, 1 : employee
        {
            VerifiedAuthorization = new ViewAuthorization(role);
            AdminView = VerifiedAuthorization.AdminView; // admin or Admin ???
            EmployeeView = VerifiedAuthorization.EmployeeView; // employee or Employee ???
        }

        public ICommand SelectViewModelCommand => new SelectViewModelCommand(this, this);
    }
}