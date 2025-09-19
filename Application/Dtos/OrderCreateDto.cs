using System;
using System.Collections.Generic;

namespace Application.Dtos
{
    public class OrderCreateDto
    {
        public List<OrderItemCreateDto> Items { get; set; } = new();
        public OrderDeliveryCreateDto Delivery { get; set; } = new();
        public string? Notes { get; set; }
    }

    public class OrderItemCreateDto
    {
        public Guid Id { get; set; }          
        public int Quantity { get; set; }
        public string? Notes { get; set; }
    }

    public class OrderDeliveryCreateDto
    {
        public int Id { get; set; }           
        public string To { get; set; } = "";  
    }
}
