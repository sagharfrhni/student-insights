using StudentInsights.Domain.Common;
using StudentInsights.Domain.ValueObjects;

namespace StudentInsights.UnitTests.Domain;

public class GradeTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(20)]
    [InlineData(15.5)]
    public void Constructor_ValueWithinRange_Succeeds(decimal value)
    {
        var grade = new Grade(value);

        grade.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(20.01)]
    public void Constructor_ValueOutOfRange_ThrowsDomainException(decimal value)
    {
        var act = () => new Grade(value);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_MoreThanTwoDecimalPlaces_ThrowsDomainException()
    {
        var act = () => new Grade(15.555m);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ImplicitConversion_ToDecimal_ReturnsValue()
    {
        var grade = new Grade(18.5m);

        decimal value = grade;

        value.Should().Be(18.5m);
    }
}