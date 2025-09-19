using System;
using System.Collections.Generic;

namespace Application.Dtos
{
    public sealed class OrderListDto
    {
        public long OrderNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public string DeliveryTo { get; set; } = "";
        public string? Notes { get; set; }
        public StatusLiteDto Status { get; set; } = new();
        public DeliveryTypeLiteDto DeliveryType { get; set; } = new();
        public List<OrderItemListDto> Items { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public sealed class OrderItemListDto
    {
        public long Id { get; set; }            
        public int Quantity { get; set; }
        public string? Notes { get; set; }
        public StatusLiteDto Status { get; set; } = new();
        public DishLiteDto Dish { get; set; } = new();
    }

    public sealed class StatusLiteDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public sealed class DeliveryTypeLiteDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public sealed class DishLiteDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string? Image { get; set; }
    }
}
