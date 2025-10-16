
namespace Application.Queries
{
    public class OrderFilterQuery
    {
        public DateTimeOffset? From { get; set; }
        public DateTimeOffset? To { get; set; }
        public int? Status { get; set; }
        public string? DeliveryTo { get; set; }
    }
}


