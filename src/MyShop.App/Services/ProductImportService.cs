using MyShop.Core.Interfaces.Services;
using MyShop.Core.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.App.Services
{
    public class ProductImportService : IProductImportService
    {
        public async Task<List<ProductImportRow>> ParseExcelAsync(string filePath)
        {
            var rows = new List<ProductImportRow>();

            await Task.Run(() =>
            {
                using var package = new ExcelPackage(new FileInfo(filePath));
                var worksheet = package.Workbook.Worksheets[0]; // First sheet
                var rowCount = worksheet.Dimension?.Rows ?? 0;

                for (int row = 2; row <= rowCount; row++)
                {
                    var importRow = new ProductImportRow
                    {
                        RowNumber = row,
                        Name = worksheet.Cells[row, 1].Text?.Trim() ?? string.Empty,
                        Sku = worksheet.Cells[row, 2].Text?.Trim() ?? string.Empty,
                        Description = worksheet.Cells[row, 3].Text?.Trim(),
                        Barcode = worksheet.Cells[row, 4].Text?.Trim(),
                        CategoryName = worksheet.Cells[row, 5].Text?.Trim()
                    };

                    // Parse numeric fields
                    if (decimal.TryParse(worksheet.Cells[row, 6].Text, out var price))
                        importRow.Price = price;

                    if (decimal.TryParse(worksheet.Cells[row, 7].Text, out var costPrice))
                        importRow.CostPrice = costPrice;

                    if (int.TryParse(worksheet.Cells[row, 8].Text, out var stock))
                        importRow.Stock = stock;

                    if (int.TryParse(worksheet.Cells[row, 9].Text, out var minStock))
                        importRow.MinStock = minStock;
                    else
                        importRow.MinStock = 5;

                    importRow.ImageUrl = worksheet.Cells[row, 10].Text?.Trim();

                    rows.Add(importRow);
                }
            });

            return rows;
        }

        public void ValidateRows(List<ProductImportRow> rows, List<Category> existingCategories, HashSet<string> existingSkus, HashSet<string> existingBarcodes)
        {
            var skusInFile = new HashSet<string>();
            var barcodesInFile = new HashSet<string>();

            foreach (var row in rows)
            {
                row.Errors.Clear();
                row.IsValid = true;

                // Validate Name
                if (string.IsNullOrWhiteSpace(row.Name))
                {
                    row.Errors.Add("Name is required");
                    row.IsValid = false;
                }
                else if (row.Name.Length > 200)
                {
                    row.Errors.Add("Name too long (max 200 chars)");
                    row.IsValid = false;
                }

                // Validate SKU
                if (string.IsNullOrWhiteSpace(row.Sku))
                {
                    row.Errors.Add("SKU is required");
                    row.IsValid = false;
                }
                else if (row.Sku.Length > 50)
                {
                    row.Errors.Add("SKU too long (max 50 chars)");
                    row.IsValid = false;
                }
                else
                {
                    // Check duplicate in database
                    if (existingSkus.Contains(row.Sku))
                    {
                        row.Errors.Add("SKU already exists in database");
                        row.IsValid = false;
                    }
                    // Check duplicate in file
                    else if (skusInFile.Contains(row.Sku))
                    {
                        row.Errors.Add("Duplicate SKU in file");
                        row.IsValid = false;
                    }
                    else
                    {
                        skusInFile.Add(row.Sku);
                    }
                }

                // Validate Barcode 
                if (!string.IsNullOrWhiteSpace(row.Barcode))
                {
                    if (row.Barcode.Length > 50)
                    {
                        row.Errors.Add("Barcode too long (max 50 chars)");
                        row.IsValid = false;
                    }
                    else
                    {
                        // Check duplicate in database
                        if (existingBarcodes.Contains(row.Barcode))
                        {
                            row.Errors.Add("Barcode already exists in database");
                            row.IsValid = false;
                        }
                        // Check duplicate in file
                        else if (barcodesInFile.Contains(row.Barcode))
                        {
                            row.Errors.Add("Duplicate Barcode in file");
                            row.IsValid = false;
                        }
                        else
                        {
                            barcodesInFile.Add(row.Barcode);
                        }
                    }
                }

                // Validate Price
                if (row.Price <= 0)
                {
                    row.Errors.Add("Price must be greater than 0");
                    row.IsValid = false;
                }

                // Validate Stock
                if (row.Stock < 0)
                {
                    row.Errors.Add("Stock cannot be negative");
                    row.IsValid = false;
                }

                // Validate Category
                if (string.IsNullOrWhiteSpace(row.CategoryName))
                {
                    row.Errors.Add("Category is required");
                    row.IsValid = false;
                }
                else
                {
                    var category = existingCategories.FirstOrDefault(c => 
                        c.Name.Equals(row.CategoryName, StringComparison.OrdinalIgnoreCase));
                    
                    if (category != null)
                    {
                        row.CategoryId = category.Id;
                    }
                }
            }
        }
    }
}
