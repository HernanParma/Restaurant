using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Order
    {
        public long OrderId { get; set; }
        public int DeliveryTypeId { get; set; }
        public DeliveryType DeliveryType { get; set; } = default!;
        public int OverallStatusId { get; set; }
        public Status OverallStatus { get; set; } = default!;
        public string DeliveryTo { get; set; } = default!;   
        public string? Notes { get; set; }                   
        public decimal Price { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }

        // Relacion
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}