using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EHandelDb.Models 
{
    public class Customer
    {
        // PK
        public int CustomerId { get; set; }
 
        [Required, MaxLength(100)] 
        public string CustomerName { get; set; } = string.Empty;
 
        [Required, MaxLength(100)]
        public string Email { get; set; } = string.Empty;
 
        [MaxLength(100)]
        public string? City { get; set; } = string.Empty; 
        public List<Order> Orders { get; set; } = new();
    }
}