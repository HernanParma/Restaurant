using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

public class LookupsEndpointsTests : IClassFixture<CustomWebAppFactory>
{
    private readonly HttpClient _c;
    public LookupsEndpointsTests(CustomWebAppFactory f) => _c = f.CreateClient();

    [Fact]
    public async Task Get_Categories_ShouldReturnSeededData()
    {
        var r = await _c.GetAsync("/api/v1/Category");
        r.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = await r.Content.ReadFromJsonAsync<List<CategoryDtoTest>>();
        data.Should().NotBeNull().And.NotBeEmpty();
        data!.Should().Contain(c => c.Name == "Entradas");
        data.Should().Contain(c => c.Name == "Pastas");
    }

    [Fact]
    public async Task Get_DeliveryTypes_ShouldReturnSeededData()
    {
        var r = await _c.GetAsync("/api/v1/DeliveryTypes");
        r.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = await r.Content.ReadFromJsonAsync<List<DeliveryTypeDtoTest>>();
        data.Should().NotBeNull().And.NotBeEmpty();
        data!.Select(x => x.Name).Should().Contain(new[] { "Delivery", "Take away", "Dine in" });
    }

    [Fact]
    public async Task Get_Status_ShouldReturnSeededData()
    {
        var r = await _c.GetAsync("/api/v1/Status");
        r.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = await r.Content.ReadFromJsonAsync<List<StatusDtoTest>>();
        data.Should().NotBeNull().And.NotBeEmpty();
        data!.Select(x => x.Name).Should().Contain(new[] { "Pending", "In progress", "Ready" });
    }

    // DTOs minimalistas para mapear las respuestas
    private sealed record CategoryDtoTest(int Id, string Name, string? Description, int Order);
    private sealed record DeliveryTypeDtoTest(int Id, string Name);
    private sealed record StatusDtoTest(int Id, string Name);
}
