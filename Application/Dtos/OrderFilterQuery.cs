using System;

namespace Application.Dtos
{
    public sealed class OrderFilterQuery
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int? Status { get; set; }
    }
}
