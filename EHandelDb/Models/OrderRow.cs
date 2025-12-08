using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATABASPROJEKT.Models
{
    /// <summary>
    /// Represents a single item line within an order, linking an order to a product.
    /// </summary>
    public class OrderRow
    {
        // PK
        public int OrderRowId { get; set; }

        // FK --> Order
        public int OrderId { get; set; }

        // FK --> Product
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        // Navigation - Referencing to the Order-object this OrderRow belongs to 
        public Order? Order { get; set; }

        // Navigation - Referencing this Product-object for this OrderRow belongs to
        public Product? Product { get; set; }
    }
}