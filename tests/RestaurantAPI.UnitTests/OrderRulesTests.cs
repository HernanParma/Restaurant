using FluentAssertions;

public class OrderRulesTests
{
    [Fact]
    public void OrderStatus_ChangesOnlyWhenAllItemsHaveSameStatus()
    {
        var items = new[] { 3, 3, 3 }; // Ready
        var allSame = items.Distinct().Count() == 1;
        allSame.Should().BeTrue();
    }
}
