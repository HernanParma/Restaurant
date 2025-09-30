using FluentAssertions;

public class DishRulesTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Price_MustBePositive(decimal price)
    {
        (price > 0).Should().BeFalse("el precio debe ser > 0");
    }
}
