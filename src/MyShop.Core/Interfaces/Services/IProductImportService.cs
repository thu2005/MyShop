using MyShop.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyShop.Core.Interfaces.Services
{
    public interface IProductImportService
    {
        /// <summary>
        /// Parses an Excel file and returns a list of ProductImportRow objects
        /// </summary>
        Task<List<ProductImportRow>> ParseExcelAsync(string filePath);

        /// <summary>
        /// Validates import rows against existing data
        /// </summary>
        void ValidateRows(List<ProductImportRow> rows, List<Category> existingCategories, HashSet<string> existingSkus, HashSet<string> existingBarcodes);
    }
}
