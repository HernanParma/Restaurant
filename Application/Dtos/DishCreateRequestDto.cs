using System.Text.Json.Serialization;

namespace Application.Dtos
{
    public class DishCreateRequestDto
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public decimal Price { get; set; }

        // clave JSON: "category"
        [JsonPropertyName("category")]
        public int Category { get; set; }

        // clave JSON: "image"
        [JsonPropertyName("image")]
        public string? Image { get; set; }
    }
}
