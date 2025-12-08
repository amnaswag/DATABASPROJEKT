using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATABASPROJEKT.Models
{
    /// <summary>
    /// Represents a category for products in the shop.
    /// </summary>
    public class Category
    {
        // PK
        public int CategoryId { get; set; }

        [Required, MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string CategoryDescription { get; set; } = string.Empty;

        // Navigation - One Category can have many Products
        public List<Product> Products { get; set; } = new();
    }
}