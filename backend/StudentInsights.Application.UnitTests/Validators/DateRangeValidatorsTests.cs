using FluentValidation.TestHelper;
using StudentInsights.Application.Common.Models;
using StudentInsights.Application.Features.Analytics.Enums;
using StudentInsights.Application.Features.Analytics.Queries.GetAssignmentProgress;
using StudentInsights.Application.Features.Analytics.Queries.GetStudyTime;
using StudentInsights.Application.Features.Calendar.Queries.GetCalendarEvents;
using StudentInsights.Application.Features.Exams.Queries.GetExams;
using StudentInsights.Application.Features.PersonalEvents.Queries.GetPersonalEvents;
using StudentInsights.Application.Features.StudyLogs.Queries.GetStudyLogs;

namespace StudentInsights.UnitTests.Validators;

public class DateRangeValidatorsTests
{
    // --- GetCalendarEventsQuery: both bounds required + max-range guard ---

    [Fact]
    public void Calendar_FromAfterTo_HasError()
    {
        var validator = new GetCalendarEventsQueryValidator();
        var query = new GetCalendarEventsQuery(DateTime.UtcNow, DateTime.UtcNow.AddDays(-1));

        var result = validator.TestValidate(query);

        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public void Calendar_RangeExceeds400Days_HasError()
    {
        var validator = new GetCalendarEventsQueryValidator();
        var from = DateTime.UtcNow;
        var query = new GetCalendarEventsQuery(from, from.AddDays(401));

        var result = validator.TestValidate(query);

        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public void Calendar_ValidRange_HasNoErrors()
    {
        var validator = new GetCalendarEventsQueryValidator();
        var from = DateTime.UtcNow;
        var query = new GetCalendarEventsQuery(from, from.AddDays(30));

        var result = validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // --- GetExamsQuery: optional From/To ---

    [Fact]
    public void Exams_FromAfterTo_HasError()
    {
        var validator = new GetExamsQueryValidator();
        var query = new GetExamsQuery(new PaginationParams(), From: DateTime.UtcNow, To: DateTime.UtcNow.AddDays(-1));

        var result = validator.TestValidate(query);

        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public void Exams_NoDatesSupplied_HasNoErrors()
    {
        var validator = new GetExamsQueryValidator();
        var query = new GetExamsQuery(new PaginationParams());

        var result = validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // --- GetStudyLogsQuery: optional From/To ---

    [Fact]
    public void StudyLogs_FromAfterTo_HasError()
    {
        var validator = new GetStudyLogsQueryValidator();
        var query = new GetStudyLogsQuery(new PaginationParams(), From: DateTime.UtcNow, To: DateTime.UtcNow.AddDays(-1));

        var result = validator.TestValidate(query);

        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public void StudyLogs_ValidRange_HasNoErrors()
    {
        var validator = new GetStudyLogsQueryValidator();
        var query = new GetStudyLogsQuery(new PaginationParams(), From: DateTime.UtcNow.AddDays(-7), To: DateTime.UtcNow);

        var result = validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // --- GetPersonalEventsQuery: optional From/To ---

    [Fact]
    public void PersonalEvents_FromAfterTo_HasError()
    {
        var validator = new GetPersonalEventsQueryValidator();
        var query = new GetPersonalEventsQuery(new PaginationParams(), From: DateTime.UtcNow, To: DateTime.UtcNow.AddDays(-1));

        var result = validator.TestValidate(query);

        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public void PersonalEvents_NoDatesSupplied_HasNoErrors()
    {
        var validator = new GetPersonalEventsQueryValidator();
        var query = new GetPersonalEventsQuery(new PaginationParams());

        var result = validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // --- GetAssignmentProgressQuery: optional From/To ---

    [Fact]
    public void AssignmentProgress_FromAfterTo_HasError()
    {
        var validator = new GetAssignmentProgressQueryValidator();
        var query = new GetAssignmentProgressQuery(DateTime.UtcNow, DateTime.UtcNow.AddDays(-1));

        var result = validator.TestValidate(query);

        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public void AssignmentProgress_NoDatesSupplied_HasNoErrors()
    {
        var validator = new GetAssignmentProgressQueryValidator();
        var query = new GetAssignmentProgressQuery();

        var result = validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // --- GetStudyTimeQuery: required Granularity + optional From/To ---

    [Fact]
    public void StudyTime_MissingGranularity_HasError()
    {
        var validator = new GetStudyTimeQueryValidator();
        var query = new GetStudyTimeQuery(Granularity: null);

        var result = validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Granularity);
    }

    [Theory]
    [InlineData(StudyTimeGranularity.Daily)]
    [InlineData(StudyTimeGranularity.Weekly)]
    [InlineData(StudyTimeGranularity.Monthly)]
    public void StudyTime_ValidGranularity_HasNoGranularityError(StudyTimeGranularity granularity)
    {
        var validator = new GetStudyTimeQueryValidator();
        var query = new GetStudyTimeQuery(granularity);

        var result = validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(x => x.Granularity);
    }

    [Fact]
    public void StudyTime_FromAfterTo_HasError()
    {
        var validator = new GetStudyTimeQueryValidator();
        var query = new GetStudyTimeQuery(StudyTimeGranularity.Daily, DateTime.UtcNow, DateTime.UtcNow.AddDays(-1));

        var result = validator.TestValidate(query);

        result.ShouldHaveAnyValidationError();
    }
}