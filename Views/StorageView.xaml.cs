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
using RestaurantManagement.ViewModels;

namespace RestaurantManagement.Views
{
    /// <summary>
    /// Interaction logic for StorageView.xaml
    /// </summary>
    public partial class StorageView : UserControl
    {
        public StorageView()
        {
            InitializeComponent();
            DataContext = new StorageViewModel();
        }

        private void TabItem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TabControl tabControl)
            {
                var selectedTab = tabControl.SelectedItem as TabItem;
                if (selectedTab?.Header?.ToString() == "Nguyên liệu thô")
                {
                    var drinkListView = FindName("DrinkIngredientsListView") as ListView;
                    if (drinkListView != null)
                    {
                        drinkListView.SelectedItem = null;
                    }
                }
                else if (selectedTab?.Header?.ToString() == "Nước uống")
                {
                    var rawListView = FindName("RawIngredientsListView") as ListView;
                    if (rawListView != null)
                    {
                        rawListView.SelectedItem = null;
                    }
                }
            }
        }
    }
}
