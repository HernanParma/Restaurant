using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

public class GetByIdEndpointsTests : IClassFixture<CustomWebAppFactory>
{
    private readonly HttpClient _c;
    public GetByIdEndpointsTests(CustomWebAppFactory f) => _c = f.CreateClient();

    [Fact]
    public async Task GetDishById_ShouldReturn200()
    {
        var d = await (await _c.PostAsJsonAsync("/api/v1/Dish", new { name = "Flan" + Guid.NewGuid().ToString("N"), price = 2500m, category = 10 }))
            .Content.ReadFromJsonAsync<Dish>();
        var g = await _c.GetAsync($"/api/v1/Dish/{d!.Id}");
        g.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetOrderById_ShouldReturn200_WithItems()
    {
        var dish = await (await _c.PostAsJsonAsync("/api/v1/Dish", new { name = "Lomo" + Guid.NewGuid().ToString("N"), price = 7000m, category = 4 }))
            .Content.ReadFromJsonAsync<Dish>();

        var ro = await _c.PostAsJsonAsync("/api/v1/Order", new
        {
            deliveryTypeId = 2,
            deliveryTo = "Hernán",
            items = new[] { new { dishId = dish!.Id, quantity = 2 } }
        });

        var created = await ro.Content.ReadFromJsonAsync<OrderCreated>();
        var get = await _c.GetAsync($"/api/v1/Order/{created!.Id}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);

        var o = await get.Content.ReadFromJsonAsync<OrderWithItems>();
        o!.Items.Should().NotBeNull().And.HaveCount(1);
    }

    private sealed record Dish(Guid Id);
    private sealed record OrderCreated(Guid Id);
    private sealed record OrderWithItems(Guid Id, decimal Price, int OverallStatusId, List<OrderItem> Items);
    private sealed record OrderItem(Guid Id, Guid DishId, int Quantity, int StatusId);
}
