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
using System.Windows.Shapes;

namespace RestaurantManagement.Views
{
    /// <summary>
    /// Interaction logic for AddIngredients.xaml
    /// </summary>
    public partial class AddIngredientsView : Window
    {
        public AddIngredientsView()
        {
            InitializeComponent();
        }

        private void TabItem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //FilterText.Text = string.Empty;
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
