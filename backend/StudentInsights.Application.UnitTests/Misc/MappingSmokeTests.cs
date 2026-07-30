using StudentInsights.Application.Common.Models;
using StudentInsights.Application.Common.Security;
using StudentInsights.Application.Common.Time;
using StudentInsights.Application.Features.Courses.Mappings;
using StudentInsights.Application.Features.Goals.Enums;
using StudentInsights.Application.Features.Goals.Mappings;
using StudentInsights.Application.Features.Goals.Services;
using StudentInsights.Application.Features.LearningActivities.Mappings;
using StudentInsights.Domain.Common;
using StudentInsights.Domain.Entities;
using StudentInsights.Domain.Enums;

namespace StudentInsights.UnitTests.Misc;

public class MappingSmokeTests
{
    private static User ValidUser() =>
        User.Create("Ali", "Rezaei", $"{Guid.NewGuid():N}@example.com", "hash");

    [Fact]
    public void Course_ToDto_MapsEveryField()
    {
        var user = ValidUser();
        var course = Course.Create(user, "Database Systems", 3, "Fall 2026", "Dr. Ahmadi");
        course.SetFinalGrade(18.5m);

        var dto = course.ToDto();

        dto.Id.Should().Be(course.Id);
        dto.Name.Should().Be(course.Name);
        dto.Semester.Should().Be(course.Semester);
        dto.Credits.Should().Be(course.Credits);
        dto.InstructorName.Should().Be(course.InstructorName);
        dto.FinalGrade.Should().Be(18.5m);
        dto.CreatedAtUtc.Should().Be(course.CreatedAtUtc);
        dto.UpdatedAtUtc.Should().Be(course.UpdatedAtUtc);
    }

    [Fact]
    public void Goal_ToDto_MapsEveryFieldIncludingProgress()
    {
        var goal = Goal.Create(ValidUser(), GoalType.ChapterCount, 10m, currentValue: 4m);
        var progress = new GoalProgressResult(GoalProgressStatus.Available, 40m);

        var dto = goal.ToDto(progress);

        dto.Id.Should().Be(goal.Id);
        dto.Type.Should().Be(goal.Type);
        dto.TargetValue.Should().Be(goal.TargetValue);
        dto.CurrentValue.Should().Be(goal.CurrentValue);
        dto.TargetDateUtc.Should().Be(goal.TargetDateUtc);
        dto.RelatedActivityId.Should().Be(goal.RelatedActivityId);
        dto.ProgressStatus.Should().Be(progress.Status);
        dto.ProgressPercentage.Should().Be(progress.ProgressPercentage);
    }

    [Fact]
    public void LearningActivity_ToDto_MapsEveryFieldIncludingSuppliedCourseName()
    {
        var user = ValidUser();
        var course = Course.Create(user, "Database Systems", 3, "Fall 2026");
        var activity = LearningActivity.Create(
            course, "Assignment 1", ActivityType.Assignment, DateTime.UtcNow.AddDays(7),
            ActivityPriority.High, "Description", "https://example.com");

        var dto = activity.ToDto(course.Name);

        dto.Id.Should().Be(activity.Id);
        dto.CourseId.Should().Be(activity.CourseId);
        dto.CourseName.Should().Be(course.Name);
        dto.Title.Should().Be(activity.Title);
        dto.Type.Should().Be(activity.Type);
        dto.DueDateUtc.Should().Be(activity.DueDateUtc);
        dto.Priority.Should().Be(activity.Priority);
        dto.Status.Should().Be(activity.Status);
        dto.Description.Should().Be(activity.Description);
        dto.ResourceLink.Should().Be(activity.ResourceLink);
        dto.CompletedAtUtc.Should().Be(activity.CompletedAtUtc);
    }
}

public class SharedUtilitiesTests
{
    // --- PaginatedResult<T>.Map ---

    [Fact]
    public void PaginatedResult_Map_TransformsItemsAndPreservesPagingMetadata()
    {
        var source = new PaginatedResult<int>(new[] { 1, 2, 3 }, pageNumber: 2, pageSize: 3, totalCount: 9);

        var mapped = source.Map(x => x.ToString());

        mapped.Items.Should().Equal("1", "2", "3");
        mapped.PageNumber.Should().Be(2);
        mapped.PageSize.Should().Be(3);
        mapped.TotalCount.Should().Be(9);
    }

    [Fact]
    public void PaginatedResult_TotalPages_RoundsUp()
    {
        var result = new PaginatedResult<int>(Array.Empty<int>(), pageNumber: 1, pageSize: 10, totalCount: 21);

        result.TotalPages.Should().Be(3);
    }

    // --- PasswordPolicy ---

    [Fact]
    public void PasswordPolicy_EnsureValid_TooShort_Throws()
    {
        var act = () => PasswordPolicy.EnsureValid("short1!");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void PasswordPolicy_EnsureValid_NoLetter_Throws()
    {
        var act = () => PasswordPolicy.EnsureValid("12345678!");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void PasswordPolicy_EnsureValid_NoDigit_Throws()
    {
        var act = () => PasswordPolicy.EnsureValid("NoDigitsHere!");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void PasswordPolicy_EnsureValid_NoSpecialCharacter_Throws()
    {
        var act = () => PasswordPolicy.EnsureValid("NoSpecial123");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void PasswordPolicy_EnsureValid_MeetsAllRules_DoesNotThrow()
    {
        var act = () => PasswordPolicy.EnsureValid("Str0ng!Pass");

        act.Should().NotThrow();
    }

    // --- SecureTokenGenerator ---

    [Fact]
    public void SecureTokenGenerator_GenerateToken_ProducesUniqueTokens()
    {
        var first = SecureTokenGenerator.GenerateToken();
        var second = SecureTokenGenerator.GenerateToken();

        first.Should().NotBe(second);
    }

    [Fact]
    public void SecureTokenGenerator_Hash_IsDeterministic()
    {
        var hash1 = SecureTokenGenerator.Hash("same-token");
        var hash2 = SecureTokenGenerator.Hash("same-token");

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void SecureTokenGenerator_Hash_DifferentInputsProduceDifferentHashes()
    {
        var hash1 = SecureTokenGenerator.Hash("token-a");
        var hash2 = SecureTokenGenerator.Hash("token-b");

        hash1.Should().NotBe(hash2);
    }

    // --- WeekBoundary ---

    [Fact]
    public void WeekBoundary_GetUtcWeekStart_ReturnsSaturdayStartOfDay()
    {
        // Wednesday, July 29, 2026 -> the week's Saturday is July 25, 2026.
        var wednesday = new DateTime(2026, 7, 29, 14, 30, 0, DateTimeKind.Utc);

        var weekStart = WeekBoundary.GetUtcWeekStart(wednesday);

        weekStart.Should().Be(new DateTime(2026, 7, 25, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void WeekBoundary_GetUtcWeekStart_OnTheBoundaryItself_ReturnsSameDayStartOfDay()
    {
        var saturday = new DateTime(2026, 7, 25, 9, 15, 0, DateTimeKind.Utc);

        var weekStart = WeekBoundary.GetUtcWeekStart(saturday);

        weekStart.Should().Be(new DateTime(2026, 7, 25, 0, 0, 0, DateTimeKind.Utc));
    }
}