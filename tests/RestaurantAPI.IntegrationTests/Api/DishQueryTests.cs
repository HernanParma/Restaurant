using System.Net.Http.Json;
using FluentAssertions;

public class DishQueryTests : IClassFixture<CustomWebAppFactory>
{
    private readonly HttpClient _c;
    public DishQueryTests(CustomWebAppFactory f) => _c = f.CreateClient();

    [Fact]
    public async Task List_WithNameFilter_ShouldMatch()
    {
        await _c.PostAsJsonAsync("/api/v1/Dish", new { name = "Pizza Muzza", price = 3000m, category = 7 });
        await _c.PostAsJsonAsync("/api/v1/Dish", new { name = "Pizza Napolitana", price = 3500m, category = 7 });

        var list = await _c.GetFromJsonAsync<List<DishResp>>("/api/v1/Dish?name=napo");
        list!.Should().OnlyContain(d =>
            d.Name.Contains("Napoli", StringComparison.OrdinalIgnoreCase) ||
            d.Name.Contains("Napolitana", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task List_WithCategoryFilter_ShouldMatch()
    {
        await _c.PostAsJsonAsync("/api/v1/Dish", new { name = "Ravioles", price = 4500m, category = 5 }); // pastas
        await _c.PostAsJsonAsync("/api/v1/Dish", new { name = "Bondiola", price = 8000m, category = 6 }); // parrilla

        var list = await _c.GetFromJsonAsync<List<DishResp>>("/api/v1/Dish?category=5");
        list!.Should().OnlyContain(d => d.Category == 5);
    }

    [Fact]
    public async Task List_WithSortAscDesc_ShouldOrderByPrice()
    {
        await _c.PostAsJsonAsync("/api/v1/Dish", new { name = "Empanada", price = 1500m, category = 1 });
        await _c.PostAsJsonAsync("/api/v1/Dish", new { name = "Bife", price = 9000m, category = 6 });

        var asc = await _c.GetFromJsonAsync<List<DishResp>>("/api/v1/Dish?sortByPrice=asc");
        asc!.First().Price.Should().BeLessThanOrEqualTo(asc.Last().Price);

        var desc = await _c.GetFromJsonAsync<List<DishResp>>("/api/v1/Dish?sortByPrice=desc");
        desc!.First().Price.Should().BeGreaterThanOrEqualTo(desc.Last().Price);
    }

    private sealed record DishResp(Guid Id, string Name, string? Description, decimal Price, int Category, bool IsActive);
}
