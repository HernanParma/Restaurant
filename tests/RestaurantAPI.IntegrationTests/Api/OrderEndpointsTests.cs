    using System.Net;
    using System.Net.Http.Json;
    using System.Text.Json;
    using FluentAssertions;

    public class OrderEndpointsTests : IClassFixture<CustomWebAppFactory>
    {
        private readonly HttpClient _c;
        public OrderEndpointsTests(CustomWebAppFactory f) => _c = f.CreateClient();

        [Fact]
        public async Task CreateOrder_ComputesTotal_AndStartsPending()
        {
            var rd = await _c.PostAsJsonAsync("/api/v1/Dish", new { name = "Pizza Muzza", price = 3000m, category = 7 });
            rd.StatusCode.Should().Be(HttpStatusCode.Created);
            var dish = await rd.Content.ReadFromJsonAsync<DishResponseTest>();
            dish.Should().NotBeNull();

            var order = new
            {
                deliveryTypeId = 1, // Delivery
                deliveryTo = "Av. Siempre Viva 742",
                items = new[]
                {
                new { dishId = dish!.Id, quantity = 2, notes = "Sin orégano" },
                new { dishId = dish!.Id, quantity = 1, notes = (string?)null }
                }
            };

            var ro = await _c.PostAsJsonAsync("/api/v1/Order", order);
            ro.StatusCode.Should().Be(HttpStatusCode.Created);

            var json = await ro.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var price = root.GetProperty("price").GetDecimal();
            price.Should().Be(3 * 3000m);

            if (root.TryGetProperty("overallStatusId", out var st))
                st.GetInt32().Should().Be(1); // Pending
        }

        private sealed record DishResponseTest(Guid Id, string Name, decimal Price, int Category);
    }
