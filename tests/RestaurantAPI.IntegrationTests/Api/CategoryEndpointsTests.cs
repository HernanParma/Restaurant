using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

public class CategoryEndpointsTests : IClassFixture<CustomWebAppFactory>
{
    private readonly HttpClient _client;
    public CategoryEndpointsTests(CustomWebAppFactory f) => _client = f.CreateClient();

    [Fact]
    public async Task GetAll_ReturnsSeededCategories()
    {
        var resp = await _client.GetAsync("/api/v1/Category");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = await resp.Content.ReadFromJsonAsync<List<CategoryDtoTest>>();
        data.Should().NotBeNull();
        data!.Should().NotBeEmpty();
        data.Should().Contain(c => c.Name == "Entradas");
        data.Should().Contain(c => c.Name == "Pastas");
    }

    private sealed record CategoryDtoTest(int Id, string Name, string? Description, int Order);
}
