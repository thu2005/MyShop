using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MyShop.Core.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string Sku { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Barcode { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? CostPrice { get; set; }

        [Required]
        public int Stock { get; set; }

        public int MinStock { get; set; }

        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }

        public bool IsActive { get; set; } = true;

        public int Popularity { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Images collection
        public List<ProductImage> Images { get; set; } = new();

        // Computed property for main image
        // Computed property for main image
        [NotMapped]
        private string? _mainImage;

        [NotMapped]
        public string? MainImage 
        { 
            get => _mainImage ?? Images?.FirstOrDefault(i => i.IsMain)?.ImageUrl ?? Images?.FirstOrDefault()?.ImageUrl;
            set => _mainImage = value;
        }
    }
}