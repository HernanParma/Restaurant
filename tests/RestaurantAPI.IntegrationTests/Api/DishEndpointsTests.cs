using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

public class DishEndpointsTests : IClassFixture<CustomWebAppFactory>
{
    private readonly HttpClient _c;
    public DishEndpointsTests(CustomWebAppFactory f) => _c = f.CreateClient();

    [Fact]
    public async Task Create_ThenDuplicateName_Returns409()
    {
        var dto = new { name = "Milanesa Napolitana", description = "Con papas", price = 5000m, category = 3, isActive = true };
        var r1 = await _c.PostAsJsonAsync("/api/v1/Dish", dto);
        r1.StatusCode.Should().Be(HttpStatusCode.Created);

        var r2 = await _c.PostAsJsonAsync("/api/v1/Dish", dto);
        r2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Get_List_WithSortAscDesc_Works()
    {
        await _c.PostAsJsonAsync("/api/v1/Dish", new { name = "Pizza", price = 3000m, category = 7 });
        await _c.PostAsJsonAsync("/api/v1/Dish", new { name = "Ravioles", price = 4500m, category = 5 });

        var asc = await _c.GetFromJsonAsync<List<DishResponseTest>>("/api/v1/Dish?sortByPrice=asc");
        asc!.First().Price.Should().BeLessThanOrEqualTo(asc.Last().Price);

        var desc = await _c.GetFromJsonAsync<List<DishResponseTest>>("/api/v1/Dish?sortByPrice=desc");
        desc!.First().Price.Should().BeGreaterThanOrEqualTo(desc.Last().Price);
    }

    private sealed record DishResponseTest(Guid Id, string Name, string? Description, decimal Price, int Category, bool IsActive);
}

