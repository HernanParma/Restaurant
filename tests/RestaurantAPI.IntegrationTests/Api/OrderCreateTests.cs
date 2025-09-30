using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

public class OrderCreateTests : IClassFixture<CustomWebAppFactory>
{
    private readonly HttpClient _c;
    public OrderCreateTests(CustomWebAppFactory f) => _c = f.CreateClient();

    [Fact]
    public async Task CreateOrder_ComputesTotal_AndStartsPending()
    {
        var d = await (await _c.PostAsJsonAsync("/api/v1/Dish", new { name = "Pizza Muzza", price = 3000m, category = 7 }))
            .Content.ReadFromJsonAsync<Dish>();
        var order = new
        {
            deliveryTypeId = 1, // Delivery
            deliveryTo = "Av. Siempre Viva 742",
            items = new[] { new { dishId = d!.Id, quantity = 2 }, new { dishId = d.Id, quantity = 1 } }
        };

        var ro = await _c.PostAsJsonAsync("/api/v1/Order", order);
        ro.StatusCode.Should().Be(HttpStatusCode.Created);

        var json = await ro.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("price").GetDecimal().Should().Be(9000m);
        if (doc.RootElement.TryGetProperty("overallStatusId", out var st))
            st.GetInt32().Should().Be(1); // Pending
    }

    [Theory]
    [InlineData(3, "Mesa 12")]   // Dine in
    [InlineData(2, "Hernán")]    // Take away
    [InlineData(1, "Av 742")]    // Delivery
    public async Task CreateOrder_RequiresDeliveryToAccordingToType(int deliveryTypeId, string deliveryTo)
    {
        var d = await (await _c.PostAsJsonAsync("/api/v1/Dish", new { name = Guid.NewGuid().ToString("N"), price = 1000m, category = 7 }))
            .Content.ReadFromJsonAsync<Dish>();

        // Falta deliveryTo -> 400
        var bad = await _c.PostAsJsonAsync("/api/v1/Order", new { deliveryTypeId, items = new[] { new { dishId = d!.Id, quantity = 1 } } });
        bad.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Con deliveryTo -> 201
        var ok = await _c.PostAsJsonAsync("/api/v1/Order", new { deliveryTypeId, deliveryTo, items = new[] { new { dishId = d!.Id, quantity = 1 } } });
        ok.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    private sealed record Dish(Guid Id, string Name, decimal Price, int Category);
}
