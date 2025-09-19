using System;

namespace Application.Dtos
{
    public sealed class OrderUpdatedResponseDto
    {
        public long OrderNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime UpdateAt { get; set; }
    }
}
