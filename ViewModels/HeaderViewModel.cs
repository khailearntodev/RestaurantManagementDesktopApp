using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RestaurantManagement.ViewModels
{
    public class HeaderViewModel : BaseViewModel
    {
        public ICommand CloseWindowCommand { get; set; }

        public HeaderViewModel()
        {
            CloseWindowCommand = new RelayCommand<UserControl>((p) => { return p == null ? false : true; }, (p) => {
                FrameworkElement window = GetWindowParent(p);
                var windows = window as Window;
                if (windows != null)
                {
                    windows.Close();
                }
            });
        }

        FrameworkElement GetWindowParent(UserControl p)
        {
            FrameworkElement t = p;

            while (t.Parent != null)
            {
                t = t.Parent as FrameworkElement;
            }

            return t;
        }
    }
}
