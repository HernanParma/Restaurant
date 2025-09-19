using System;

namespace Application.Dtos
{
    public class OrderCreatedResponseDto
    {
        public long OrderNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
