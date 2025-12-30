using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.App.ViewModels
{
    public partial class ImportProductsViewModel : ObservableObject
    {
        private readonly IProductImportService _importService;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        // Event to notify when import is completed
        public event EventHandler ImportCompleted;

        [ObservableProperty]
        private ObservableCollection<ProductImportRow> _importRows = new();

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private int _totalRows;

        [ObservableProperty]
        private int _validRows;

        public bool HasValidRows => ValidRows > 0;

        [ObservableProperty]
        private int _invalidRows;

        [ObservableProperty]
        private int _importProgress;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        public ImportProductsViewModel(
            IProductImportService importService,
            IProductRepository productRepository,
            ICategoryRepository categoryRepository)
        {
            _importService = importService;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task LoadFileAsync(string filePath)
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Parsing Excel file...";

                // Parse Excel
                var rows = await _importService.ParseExcelAsync(filePath);
                
                // Get existing data for validation
                var existingProducts = await _productRepository.GetAllAsync();
                var existingSkus = existingProducts.Select(p => p.Sku).ToHashSet();
                var existingBarcodes = existingProducts
                    .Where(p => !string.IsNullOrEmpty(p.Barcode))
                    .Select(p => p.Barcode)
                    .ToHashSet();
                var existingCategories = await _categoryRepository.GetAllAsync();

                // Validate
                StatusMessage = "Validating data...";
                _importService.ValidateRows(rows, existingCategories.ToList(), existingSkus, existingBarcodes);

                // Update UI
                ImportRows.Clear();
                foreach (var row in rows)
                {
                    ImportRows.Add(row);
                }

                TotalRows = rows.Count;
                ValidRows = rows.Count(r => r.IsValid);
                InvalidRows = rows.Count(r => !r.IsValid);
                OnPropertyChanged(nameof(HasValidRows)); // Notify HasValidRows changed

                StatusMessage = $"Ready to import {ValidRows} valid products";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ImportValidRowsAsync()
        {
            try
            {
                IsBusy = true;
                var validRows = ImportRows.Where(r => r.IsValid).ToList();
                var imported = 0;
                var failed = 0;


                foreach (var row in validRows)
                {
                    try
                    {
                        // Create category if doesn't exist
                        if (!row.CategoryId.HasValue && !string.IsNullOrEmpty(row.CategoryName))
                        {
                            var newCategory = new Category { Name = row.CategoryName };
                            var created = await _categoryRepository.AddAsync(newCategory);
                            row.CategoryId = created.Id;
                        }

                        var product = new Product
                        {
                            Name = row.Name,
                            Sku = row.Sku,
                            Description = row.Description,
                            Barcode = row.Barcode,
                            Price = row.Price,
                            CostPrice = row.CostPrice ?? 0,
                            Stock = row.Stock,
                            MinStock = row.MinStock,
                            CategoryId = row.CategoryId!.Value,
                            Images = !string.IsNullOrWhiteSpace(row.ImageUrl) 
                                ? row.ImageUrl.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select((url, index) => new ProductImage 
                                      { 
                                          ImageUrl = url.Trim(), 
                                          DisplayOrder = index, 
                                          IsMain = index == 0 
                                      }).ToList()
                                : new System.Collections.Generic.List<ProductImage>()
                        };

                        var result = await _productRepository.AddAsync(product);
                        imported++;
                        
                        ImportProgress = (int)((double)imported / validRows.Count * 100);
                        StatusMessage = $"Importing... {imported}/{validRows.Count}";
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        row.IsValid = false;
                        var errorMsg = $"Import failed: {ex.Message}";
                        row.Errors.Add(errorMsg);
                    }
                }

                StatusMessage = $"Successfully imported {imported} products! ({failed} failed)";
                
                // Raise event to notify completion (if any products were imported)
                if (imported > 0)
                {
                    ImportCompleted?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Import failed: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
                ImportProgress = 0;
            }
        }
    }
}
