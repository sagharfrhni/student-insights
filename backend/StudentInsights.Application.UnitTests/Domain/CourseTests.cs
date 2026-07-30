using StudentInsights.Domain.Common;
using StudentInsights.Domain.Entities;
using StudentInsights.Domain.Enums;

namespace StudentInsights.UnitTests.Domain;

public class CourseTests
{
    private static User ValidUser() =>
        User.Create("Ali", "Rezaei", $"{Guid.NewGuid():N}@example.com", "hash");

    [Fact]
    public void Create_ValidData_Succeeds()
    {
        var user = ValidUser();

        var course = Course.Create(user, "Database Systems", 3, "Fall 2026", "Dr. Ahmadi");

        course.UserId.Should().Be(user.Id);
        course.Name.Should().Be("Database Systems");
        course.Credits.Should().Be(3);
        course.Semester.Should().Be("Fall 2026");
        course.InstructorName.Should().Be("Dr. Ahmadi");
    }

    [Fact]
    public void Create_NullUser_ThrowsDomainException()
    {
        var act = () => Course.Create(null!, "Database Systems", 3, "Fall 2026");

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_BlankName_ThrowsDomainException(string? name)
    {
        var user = ValidUser();

        var act = () => Course.Create(user, name!, 3, "Fall 2026");

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_NonPositiveCredits_ThrowsDomainException(int credits)
    {
        var user = ValidUser();

        var act = () => Course.Create(user, "Database Systems", credits, "Fall 2026");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateCredits_NonPositiveValue_ThrowsDomainException()
    {
        var course = Course.Create(ValidUser(), "Database Systems", 3, "Fall 2026");

        var act = () => course.UpdateCredits(0);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateCredits_PositiveValue_UpdatesCredits()
    {
        var course = Course.Create(ValidUser(), "Database Systems", 3, "Fall 2026");

        course.UpdateCredits(4);

        course.Credits.Should().Be(4);
    }

    [Fact]
    public void AddClassSchedule_EndTimeBeforeStartTime_ThrowsDomainException()
    {
        var course = Course.Create(ValidUser(), "Database Systems", 3, "Fall 2026");

        var act = () => course.AddClassSchedule(DayOfWeek.Monday, TimeSpan.FromHours(10), TimeSpan.FromHours(9));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddClassSchedule_OverlappingSameDay_ThrowsDomainException()
    {
        var course = Course.Create(ValidUser(), "Database Systems", 3, "Fall 2026");
        course.AddClassSchedule(DayOfWeek.Monday, TimeSpan.FromHours(9), TimeSpan.FromHours(11));

        var act = () => course.AddClassSchedule(DayOfWeek.Monday, TimeSpan.FromHours(10), TimeSpan.FromHours(12));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddClassSchedule_ExactlyTouchingBoundarySameDay_DoesNotThrow()
    {
        // The second schedule starts exactly when the first ends — this must
        // NOT count as an overlap. This is the boundary case the roadmap
        // flags as the exact spot an off-by-one bug would hide (Course.cs:
        // startTime < s.EndTime && endTime > s.StartTime).
        var course = Course.Create(ValidUser(), "Database Systems", 3, "Fall 2026");
        course.AddClassSchedule(DayOfWeek.Monday, TimeSpan.FromHours(9), TimeSpan.FromHours(10));

        var act = () => course.AddClassSchedule(DayOfWeek.Monday, TimeSpan.FromHours(10), TimeSpan.FromHours(11));

        act.Should().NotThrow();
        course.ClassSchedules.Should().HaveCount(2);
    }

    [Fact]
    public void AddClassSchedule_SameTimeDifferentDay_DoesNotThrow()
    {
        var course = Course.Create(ValidUser(), "Database Systems", 3, "Fall 2026");
        course.AddClassSchedule(DayOfWeek.Monday, TimeSpan.FromHours(9), TimeSpan.FromHours(11));

        var act = () => course.AddClassSchedule(DayOfWeek.Tuesday, TimeSpan.FromHours(9), TimeSpan.FromHours(11));

        act.Should().NotThrow();
        course.ClassSchedules.Should().HaveCount(2);
    }

    [Fact]
    public void SetFinalGrade_ValueOutOfRange_ThrowsDomainException()
    {
        var course = Course.Create(ValidUser(), "Database Systems", 3, "Fall 2026");

        var act = () => course.SetFinalGrade(21m);

        act.Should().Throw<DomainException>();
    }
}