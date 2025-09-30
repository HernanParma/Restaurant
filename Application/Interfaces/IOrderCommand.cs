using Application.Dtos;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IOrderCommand
    {
        Task<Application.Dtos.OrderCreatedResponseDto> CreateAsync(CreateOrderCommandModel model, CancellationToken ct = default);
        Task<OrderUpdatedResponseDto> UpdateItemStatusAsync(long orderId, long itemId, int newItemStatusId, int newOverallStatusId, CancellationToken ct = default);
        Task<OrderUpdatedResponseDto> UpdateItemsAsync(
        long orderId,
        IReadOnlyList<OrderItemToPersist> newItems,
        decimal newTotal,
        CancellationToken ct = default);
    }

    public class CreateOrderCommandModel
    {
        public int DeliveryTypeId { get; set; }
        public string DeliveryTo { get; set; } = "";
        public string? Notes { get; set; }
        public System.Collections.Generic.List<OrderItemToPersist> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public int InitialStatusId { get; set; } = 1; 
    }

    public class OrderItemToPersist
    {
        public System.Guid DishId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Notes { get; set; }
    }
}
