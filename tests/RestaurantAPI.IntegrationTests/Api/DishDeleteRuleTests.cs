using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

public class DishDeleteRuleTests : IClassFixture<CustomWebAppFactory>
{
    private readonly HttpClient _c;
    public DishDeleteRuleTests(CustomWebAppFactory f) => _c = f.CreateClient();

    [Fact]
    public async Task DeleteDish_WithExistingOrder_ShouldReturn409()
    {
        var d = await (await _c.PostAsJsonAsync("/api/v1/Dish", new { name = "Tarta" + Guid.NewGuid().ToString("N"), price = 2000m, category = 3 }))
            .Content.ReadFromJsonAsync<Dish>();

        await _c.PostAsJsonAsync("/api/v1/Order", new { deliveryTypeId = 1, deliveryTo = "Calle 123", items = new[] { new { dishId = d!.Id, quantity = 1 } } });

        var del = await _c.DeleteAsync($"/api/v1/Dish/{d!.Id}");
        del.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    private sealed record Dish(Guid Id);
}
