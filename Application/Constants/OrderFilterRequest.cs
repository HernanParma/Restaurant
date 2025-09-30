namespace RestaurantAPI.Contracts
{
    public class OrderFilterRequest
    {
        public DateTimeOffset? From { get; set; }
        public DateTimeOffset? To { get; set; }
        public int? Status { get; set; }
    }
}