using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
namespace Application.Dtos
{
    public class DishResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public CategoryLiteDto Category { get; set; } = default!;

        [JsonPropertyName("image")]
        public string? ImageUrl { get; set; }

        [JsonPropertyName("isActive")]
        public bool Available { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}