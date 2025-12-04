using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EHandelDb.Models
{
    public class Order
    {
        // PK
        public int OrderId { get; set; }

        // FK
        public int CustomerId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required, MaxLength(100)]
        public string Status { get; set; } = string.Empty;
        
        public Customer? Customer { get; set; }
        public List<OrderRow> OrderRows { get; set; } = new();
    }
}