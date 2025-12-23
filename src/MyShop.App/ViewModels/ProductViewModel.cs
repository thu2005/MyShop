using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using MyShop.App.ViewModels.Base;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Models;

namespace MyShop.App.ViewModels
{
    public class ProductViewModel : ViewModelBase
    {
        private readonly IProductRepository _productRepository;
        private ObservableCollection<Product> _products;

        public ProductViewModel(IProductRepository productRepository)
        {
            _productRepository = productRepository;
            _products = new ObservableCollection<Product>();

            // Load data when ViewModel is created
            LoadProductsCommand = new RelayCommand(async _ => await LoadProductsAsync());
            _ = LoadProductsAsync();
        }

        // The list bound to the UI
        public ObservableCollection<Product> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        public ICommand LoadProductsCommand { get; }

        public async Task LoadProductsAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty; // Clear previous errors

                var productList = await _productRepository.GetAllAsync();

                Products.Clear();
                foreach (var p in productList)
                {
                    Products.Add(p);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Could not load products. Is the backend running?";
                Debug.WriteLine($"[ProductViewModel Error] {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}