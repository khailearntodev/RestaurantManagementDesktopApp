using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using RestaurantManagement.ViewModels;

namespace RestaurantManagement.State.Navigator
{
    public enum ViewType 
    {
        Employee,
        Storage,
        OrderMenu,      // Use for employee
        FoodMenu,       // Use for admin
        TableHistory, 
        TableStatus,
        Settings,
        Logout,
        Statistics,
        Kitchen
    }

    public interface INavigator
    {
        BaseViewModel CurrentViewModel { get; set; }
        string CurrentTitle { get; set; }
        Visibility AdminView { get; set; }
        Visibility EmployeeView { get; set; }
        ICommand SelectViewModelCommand { get; }
    }
}
