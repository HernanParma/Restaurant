using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public sealed class OrderItemPatchOpDto
    {
        public string Op { get; set; } = default!;
        public long? OrderItemId { get; set; }
        public Guid? DishId { get; set; }
        public int? Quantity { get; set; }
        public string? Notes { get; set; }
    }
}
