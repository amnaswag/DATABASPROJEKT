using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATABASPROJEKT.Models
{
    /// <summary>
    /// Represents a customer order in the shop.
    /// </summary>
    public class Order
    {
        // PK
        public int OrderId { get; set; }

        // FK -> Customer
        public int CustomerId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required, MaxLength(100)]
        public string Status { get; set; } = string.Empty;

        // Navigation - Referencing to the Customer that *owns* the Order 
        public Customer? Customer { get; set; }

        // Navigation - One Order can have several OrderRows
        public List<OrderRow> OrderRows { get; set; } = new();
    }
}