
using RestaurantManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RestaurantManagement.Views
{
    /// <summary>
    /// Interaction logic for BillHistory.xaml
    /// </summary>
    public partial class BillHistory : UserControl
    {
        public BillHistory()
        {
            InitializeComponent();
            this.DataContext = new BillHistoryViewModel();

        }
    }
}
