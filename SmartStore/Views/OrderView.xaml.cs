using SmartStorePOS.Models;
using SmartStorePOS.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SmartStorePOS.Views
{
    /// <summary>
    /// Interaction logic for OrderView.xaml
    /// </summary>
    public partial class OrderView : UserControl
    {
        OrderViewModel viewModel;
        public OrderView()
        {
            InitializeComponent();
            this.Loaded += OrderView_Loaded;
        }

        private async void OrderView_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel = this.DataContext as OrderViewModel;
            if (viewModel != null)
            {
                await viewModel.LoadProducts();
            }
        }

        private void ProductComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox &&
                comboBox.SelectedItem is ProductDTO selectedProduct &&
                comboBox.DataContext is OrderItem orderItem &&
                selectedProduct.ProductId != orderItem.ProductId &&
                DataContext is OrderViewModel viewModel)
            {
                viewModel.OnProductSelected(selectedProduct, orderItem);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                comboBox.IsDropDownOpen = true;
            }
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // xử lý cộng tổng tiền
            viewModel.CalculateTotalPrice();
        }
    }
}
