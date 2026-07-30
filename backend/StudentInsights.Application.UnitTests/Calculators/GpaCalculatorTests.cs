using StudentInsights.Application.Common.Academics;

namespace StudentInsights.UnitTests.Calculators;

public class GpaCalculatorTests
{
    [Fact]
    public void CalculateCreditWeighted_NoGradedCourses_ReturnsNull()
    {
        var result = GpaCalculator.CalculateCreditWeighted(Array.Empty<(decimal, int)>());

        result.Should().BeNull();
    }

    [Fact]
    public void CalculateCreditWeighted_ZeroTotalCredits_ReturnsNull()
    {
        var result = GpaCalculator.CalculateCreditWeighted(new[] { (FinalGrade: 15m, Credits: 0) });

        result.Should().BeNull();
    }

    [Fact]
    public void CalculateCreditWeighted_WeightsByCredits_NotSimpleAverage()
    {
        // A 1-credit 20 and a 4-credit 10 must weight toward the 4-credit
        // course: (20*1 + 10*4) / 5 = 12, not the unweighted average of 15.
        var courses = new[]
        {
            (FinalGrade: 20m, Credits: 1),
            (FinalGrade: 10m, Credits: 4)
        };

        var result = GpaCalculator.CalculateCreditWeighted(courses);

        result.Should().Be(12m);
    }

    [Fact]
    public void CalculateCreditWeighted_SingleCourse_ReturnsItsGrade()
    {
        var courses = new[] { (FinalGrade: 17.5m, Credits: 3) };

        var result = GpaCalculator.CalculateCreditWeighted(courses);

        result.Should().Be(17.5m);
    }
}