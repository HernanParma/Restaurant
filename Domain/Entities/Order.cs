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
        public int DeliveryType { get; set; }
        public DeliveryType DeliveryTypes { get; set; } = default!;
        public int OverallStatus { get; set; }
        public Status OverallStatuses { get; set; } = default!;
        public string DeliveryTo { get; set; } = default!;   
        public string? Notes { get; set; }                   
        public decimal Price { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }

        // Relacion
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}