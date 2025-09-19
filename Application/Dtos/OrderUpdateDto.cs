using System;
using System.Collections.Generic;

namespace Application.Dtos
{
    public sealed class OrderUpdateDto
    {
        public List<OrderUpdateItemDto> Items { get; set; } = new();
    }

    public sealed class OrderUpdateItemDto
    {
        public Guid Id { get; set; }      
        public int Quantity { get; set; }
        public string? Notes { get; set; }
    }
}
