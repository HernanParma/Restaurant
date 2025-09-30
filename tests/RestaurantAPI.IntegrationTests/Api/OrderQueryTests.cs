using System.Net.Http.Json;
using FluentAssertions;

public class OrderQueryTests : IClassFixture<CustomWebAppFactory>
{
    private readonly HttpClient _c;
    public OrderQueryTests(CustomWebAppFactory f) => _c = f.CreateClient();

    [Fact]
    public async Task GetOrders_ByDate_AndStatus_ShouldFilter()
    {
        // crear plato
        var d = await (await _c.PostAsJsonAsync("/api/v1/Dish", new { name = "Napolitana" + Guid.NewGuid().ToString("N"), price = 2000m, category = 3 }))
            .Content.ReadFromJsonAsync<Dish>();

        // crear dos órdenes hoy
        await _c.PostAsJsonAsync("/api/v1/Order", new { deliveryTypeId = 2, deliveryTo = "Hernán", items = new[] { new { dishId = d!.Id, quantity = 1 } } });
        await _c.PostAsJsonAsync("/api/v1/Order", new { deliveryTypeId = 2, deliveryTo = "Hernán", items = new[] { new { dishId = d!.Id, quantity = 2 } } });

        var today = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
        var list = await _c.GetFromJsonAsync<List<OrderResp>>($"/api/v1/Order?date={today}&status=pending");
        list.Should().NotBeNull().And.NotBeEmpty();
        list!.All(o => o.OverallStatusId == 1).Should().BeTrue();
    }

    private sealed record Dish(Guid Id);
    private sealed record OrderResp(Guid Id, decimal Price, int OverallStatusId);
}
