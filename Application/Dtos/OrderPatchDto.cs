using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public sealed class OrderPatchDto
    {
        public string? DeliveryTo { get; set; }
        public string? Notes { get; set; }
        public IReadOnlyList<OrderItemPatchOpDto>? Items { get; set; }
    }
}
