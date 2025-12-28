using System;

namespace MyShop.App.Models
{
    public class ProductDraft
    {
        public string Name { get; set; }
        public string Sku { get; set; }
        public string Description { get; set; }
        public string Barcode { get; set; }
        public string Price { get; set; }
        public string CostPrice { get; set; }
        public string Stock { get; set; }
        public string MinStock { get; set; }
        public string ImageUrl { get; set; }
        public int? CategoryId { get; set; }
        public DateTime SavedAt { get; set; }
    }
}
