using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

public class OrderItemPatchTests : IClassFixture<CustomWebAppFactory>
{
    private readonly HttpClient _c;
    public OrderItemPatchTests(CustomWebAppFactory f) => _c = f.CreateClient();

    [Fact]
    public async Task PatchItemStatus_ChangesOrderStatusWhenAllMatch()
    {
        var d = await (await _c.PostAsJsonAsync("/api/v1/Dish", new { name = "Faina" + Guid.NewGuid().ToString("N"), price = 1000m, category = 7 }))
            .Content.ReadFromJsonAsync<Dish>();
        var order = new
        {
            deliveryTypeId = 3,
            deliveryTo = "Mesa 5",
            items = new[] { new { dishId = d!.Id, quantity = 1 }, new { dishId = d.Id, quantity = 1 } }
        };
        var ro = await _c.PostAsJsonAsync("/api/v1/Order", order);
        var created = await ro.Content.ReadFromJsonAsync<OrderCreated>();
        var orderId = created!.Id;
        var item1 = created.Items[0].Id;
        var item2 = created.Items[1].Id;

        // item1 -> Ready (3)
        (await _c.PatchAsJsonAsync($"/api/v1/Order/{orderId}/item/{item1}", new { statusId = 3 })).StatusCode.Should().Be(HttpStatusCode.NoContent);

        // la orden sigue pendiente
        var o1 = await _c.GetStringAsync($"/api/v1/Order/{orderId}");
        using (var doc1 = JsonDocument.Parse(o1))
            doc1.RootElement.GetProperty("overallStatusId").GetInt32().Should().Be(1);

        // item2 -> Ready
        (await _c.PatchAsJsonAsync($"/api/v1/Order/{orderId}/item/{item2}", new { statusId = 3 })).StatusCode.Should().Be(HttpStatusCode.NoContent);

        // ahora la orden Ready
        var o2 = await _c.GetStringAsync($"/api/v1/Order/{orderId}");
        using var doc2 = JsonDocument.Parse(o2);
        doc2.RootElement.GetProperty("overallStatusId").GetInt32().Should().Be(3);
    }

    private sealed record Dish(Guid Id);
    private sealed record OrderItem(Guid Id);
    private sealed record OrderCreated(Guid Id, decimal Price, int OverallStatusId, List<OrderItem> Items);
}
