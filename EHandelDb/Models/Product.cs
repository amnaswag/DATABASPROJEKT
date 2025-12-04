using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EHandelDb.Models
{
    public class Product
    {
        // PK
        public int ProductId { get; set; }
 
        // FK
        [Required]
        public int CategoryId { get; set; }
 
        [Required, MaxLength(100)]
        public string ProductName { get; set; } = string.Empty;
 
        [Required]
        public decimal Price { get; set; }
 
        public List<OrderRow> OrderRows { get; set; } = new();
        public Category? Category { get; set; }
    }
}