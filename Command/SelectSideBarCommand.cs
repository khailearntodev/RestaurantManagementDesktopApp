using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using RestaurantManagement.Models;
using RestaurantManagement.State.Navigator;
using RestaurantManagement.ViewModels;

namespace RestaurantManagement.Command
{
    public class SelectViewModelCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        private readonly INavigator viewModelNavigator;
        private readonly INavigator titleNavigator;

        // Set the MENU screen as default when logging in for admin or employee via constructor
        public SelectViewModelCommand(INavigator vmNavigator, INavigator _titleNavigator)
        {
            if (vmNavigator.AdminView == System.Windows.Visibility.Visible) // Admin
            {
                vmNavigator.CurrentViewModel = new FoodMenuViewModel();
                _titleNavigator.CurrentTitle = "Danh sách món ăn";
                viewModelNavigator = vmNavigator;
                titleNavigator = _titleNavigator;
            }
            else // Employee
            {
                vmNavigator.CurrentViewModel = new MenuViewModel();
                _titleNavigator.CurrentTitle = "Đặt bàn";
                viewModelNavigator = vmNavigator;
                titleNavigator = _titleNavigator;
            }
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            if (parameter is ViewType)
            {
                ViewType viewType = (ViewType)parameter;

                switch(viewType)
                {
                    case ViewType.Employee:
                    
                        viewModelNavigator.CurrentViewModel = new EmployeeViewModel();
                        titleNavigator.CurrentTitle = "Nhân viên";
                        break;
                    case ViewType.Storage:
                        viewModelNavigator.CurrentViewModel = new StorageViewModel();
                        titleNavigator.CurrentTitle = "Kho nguyên liệu";
                        break;
                    case ViewType.OrderMenu:
                        viewModelNavigator.CurrentViewModel = new MenuViewModel();
                        titleNavigator.CurrentTitle = "Đặt bàn";
                        break;
                    case ViewType.FoodMenu:
                        viewModelNavigator.CurrentViewModel = new FoodMenuViewModel();
                        titleNavigator.CurrentTitle = "Danh sách món ăn";
                        break;
                    case ViewType.TableHistory:
                        viewModelNavigator.CurrentViewModel = new BillHistoryViewModel();
                        titleNavigator.CurrentTitle = "Lịch sử hóa đơn";
                        break;
                    case ViewType.TableStatus:
                        viewModelNavigator.CurrentViewModel = new TableStateViewModel();
                        titleNavigator.CurrentTitle = "Tình trạng bàn";
                        break;
                    case ViewType.Statistics:
                        viewModelNavigator.CurrentViewModel = new StatisticsViewModel();
                        titleNavigator.CurrentTitle = "Thống kê";
                        break;
                    case ViewType.Kitchen:
                        viewModelNavigator.CurrentViewModel = new KitchenViewModel();
                        titleNavigator.CurrentTitle = "Bếp";
                        break;
                    default:
                        break;
                }
            }
        }
    }
}