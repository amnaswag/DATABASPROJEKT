using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATABASPROJEKT.Models
{
    /// <summary>
    /// Represents a customer in the e-commerce system.
    /// </summary>
    public class Customer
    {
        // PK
        public int CustomerId { get; set; }

        [Required, MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        // Navigation - One Customer can have several Orders
        public List<Order> Orders { get; set; } = new();
    }
}