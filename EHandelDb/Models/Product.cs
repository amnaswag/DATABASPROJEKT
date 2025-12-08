using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATABASPROJEKT.Models
{
    public class Product
    {
        // PK
        public int ProductId { get; set; }

        // FK -> Category
        public int CategoryId { get; set; }

        [Required, MaxLength(100)]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        // Navigation - One Product can exist in several OrderRows.
        public List<OrderRow> OrderRows { get; set; } = new();

        // Navigation - Referencing the Category the specific Product belongs to 
        public Category? Category { get; set; }
    }
}