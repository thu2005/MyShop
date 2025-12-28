namespace MyShop.Core.Models
{
    public class ProductImportRow
    {
        // Row metadata
        public int RowNumber { get; set; }
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();

        // Product fields (matching CreateProductInput)
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public decimal Price { get; set; }
        public decimal? CostPrice { get; set; }
        public int Stock { get; set; }
        public int MinStock { get; set; } = 5;
        public string? ImageUrl { get; set; }
        
        // Category can be ID or Name
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }

        // Display helpers
        public string ValidationStatus => IsValid ? "✅ Valid" : "❌ Invalid";
        public string ErrorSummary => Errors.Count > 0 ? string.Join(", ", Errors) : string.Empty;
    }
}
