using FluentAssertions;
using Microsoft.OpenApi.Readers;

public class OpenApiContractTests : IClassFixture<CustomWebAppFactory>
{
    private readonly HttpClient _c;
    public OpenApiContractTests(CustomWebAppFactory f) => _c = f.CreateClient();

    [Fact]
    public async Task Swagger_HasRequiredPaths()
    {
        var json = await _c.GetStringAsync("/swagger/v1/swagger.json");
        var doc = new OpenApiStringReader().Read(json, out _);

        doc.Paths.Should().ContainKey("/api/v1/Category");
        doc.Paths.Should().ContainKey("/api/v1/DeliveryTypes");
        doc.Paths.Should().ContainKey("/api/v1/Status");
        doc.Paths.Should().ContainKey("/api/v1/Dish");
        doc.Paths.Should().ContainKey("/api/v1/Dish/{id}");
        doc.Paths.Should().ContainKey("/api/v1/Order");
        doc.Paths.Should().ContainKey("/api/v1/Order/{id}");
        doc.Paths.Should().ContainKey("/api/v1/Order/{id}/item/{itemId}");
    }
}
