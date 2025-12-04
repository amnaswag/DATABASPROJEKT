using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EHandelDb.Models 
{
    public class Category
    {
        // PK
        public int CategoryId { get; set; }
 
        [Required, MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;
 
        [Required, MaxLength(150)] 
        public string CategoryDescription { get; set; } = string.Empty;
 
        // Navigation property
        public List<Product> Products { get; set; } = new List<Product>();
    }
}