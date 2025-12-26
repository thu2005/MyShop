using CommunityToolkit.Mvvm.ComponentModel;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Models;
using MyShop.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyShop.App.ViewModels
{
    public partial class AddProductViewModel : ObservableObject
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IImageUploadService _imageUploadService;

        [ObservableProperty]
        private ObservableCollection<Category> _categories;

        [ObservableProperty]
        private bool _isBusy;

        public AddProductViewModel(
            IProductRepository productRepository, 
            ICategoryRepository categoryRepository,
            IImageUploadService imageUploadService)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _imageUploadService = imageUploadService;
            _categories = new ObservableCollection<Category>();
        }

        public async Task LoadCategoriesAsync()
        {
            try
            {
                var cats = await _categoryRepository.GetAllAsync();
                Categories.Clear();
                foreach (var cat in cats)
                {
                    Categories.Add(cat);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load categories: {ex.Message}");
            }
        }

        public async Task<string> UploadImageAsync(StorageFile file)
        {
            try
            {
                IsBusy = true;
                return await _imageUploadService.UploadProductImageAsync(file);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Image upload failed: {ex.Message}");
                throw;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task<Product> AddProductAsync(Product product)
        {
            try
            {
                IsBusy = true;
                return await _productRepository.AddAsync(product);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to add product: {ex.Message}");
                throw;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
