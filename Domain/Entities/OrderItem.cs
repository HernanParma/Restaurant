using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class OrderItem
    {
        public long OrderItemId { get; set; }
        public int Quantity { get; set; }
        public string? Notes { get; set; }        
        public DateTime CreateDate { get; set; }
        //Relaciones
        public Order Order { get; set; } = default!;
        public long OrderId { get; set; }
        public Dish Dish { get; set; } = default!;
        public Guid DishId { get; set; }
        public Status Status { get; set; } = default!;
        public int StatusId { get; set; }
    }
}