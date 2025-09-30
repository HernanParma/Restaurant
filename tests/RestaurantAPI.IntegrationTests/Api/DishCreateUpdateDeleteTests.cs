using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

public class DishCreateUpdateDeleteTests : IClassFixture<CustomWebAppFactory>
{
    private readonly HttpClient _c;
    public DishCreateUpdateDeleteTests(CustomWebAppFactory f) => _c = f.CreateClient();

    [Fact]
    public async Task CreateDish_ShouldReturn201_AndLocation()
    {
        var dto = new { name = "Ravioles Caseros", description = "Con tuco", price = 4200m, category = 5, isActive = true };
        var r = await _c.PostAsJsonAsync("/api/v1/Dish", dto);
        r.StatusCode.Should().Be(HttpStatusCode.Created);
        r.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateDish_DuplicateName_ShouldReturn409()
    {
        var dto = new { name = "Milanesa", price = 5000m, category = 3 };
        (await _c.PostAsJsonAsync("/api/v1/Dish", dto)).StatusCode.Should().Be(HttpStatusCode.Created);
        (await _c.PostAsJsonAsync("/api/v1/Dish", dto)).StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateDish_InvalidPrice_ShouldReturn400(decimal price)
    {
        var dto = new { name = $"PrecioInvalido{price}", price, category = 3 };
        var r = await _c.PostAsJsonAsync("/api/v1/Dish", dto);
        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateDish_ShouldReturn200_AndPersistChanges()
    {
        var created = await (await _c.PostAsJsonAsync("/api/v1/Dish", new { name = "Lomo", price = 6000m, category = 4 }))
            .Content.ReadFromJsonAsync<DishResp>();

        var r = await _c.PutAsJsonAsync($"/api/v1/Dish/{created!.Id}",
            new { name = "Lomo Completo", price = 6500m, category = 4 });

        r.StatusCode.Should().Be(HttpStatusCode.OK);

        var get = await _c.GetFromJsonAsync<DishResp>($"/api/v1/Dish/{created!.Id}");
        get!.Name.Should().Be("Lomo Completo");
        get.Price.Should().Be(6500m);
    }

    private sealed record DishResp(Guid Id, string Name, string? Description, decimal Price, int Category, bool IsActive);
}
