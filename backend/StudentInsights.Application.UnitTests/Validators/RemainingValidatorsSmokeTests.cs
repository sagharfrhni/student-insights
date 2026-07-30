using FluentValidation.TestHelper;
using StudentInsights.Application.Common.Models;
using StudentInsights.Application.Features.Admin.Settings.Commands.CreateSystemSetting;
using StudentInsights.Application.Features.Admin.Settings.Commands.UpdateSystemSettingValue;
using StudentInsights.Application.Features.Admin.Users.Commands.ChangeUserRole;
using StudentInsights.Application.Features.Admin.Users.Queries.GetUsers;
using StudentInsights.Application.Features.Courses.Commands.RecordCourseGrade;
using StudentInsights.Application.Features.Courses.Commands.UpdateCourse;
using StudentInsights.Application.Features.Exams.Commands.CreateExam;
using StudentInsights.Application.Features.Exams.Commands.RecordExamGrade;
using StudentInsights.Application.Features.Exams.Commands.UpdateExam;
using StudentInsights.Application.Features.Goals.Commands.CreateGoal;
using StudentInsights.Application.Features.Goals.Commands.UpdateGoal;
using StudentInsights.Application.Features.LearningActivities.Commands.CreateLearningActivity;
using StudentInsights.Application.Features.LearningActivities.Commands.UpdateLearningActivity;
using StudentInsights.Application.Features.LearningActivities.Commands.UpdateLearningActivityStatus;
using StudentInsights.Application.Features.Notifications.Queries.GetNotifications;
using StudentInsights.Application.Features.PersonalEvents.Commands.CreatePersonalEvent;
using StudentInsights.Application.Features.PersonalEvents.Commands.UpdatePersonalEvent;
using StudentInsights.Application.Features.StudyLogs.Commands.CreateStudyLog;
using StudentInsights.Application.Features.StudyLogs.Commands.UpdateStudyLog;
using StudentInsights.Application.Features.Users.Commands.UpdateMyProfile;
using StudentInsights.Domain.Enums;

namespace StudentInsights.UnitTests.Validators;

public class RemainingValidatorsSmokeTests
{
    [Fact]
    public void UpdateCourseCommandValidator_ValidCommand_HasNoErrors()
    {
        var validator = new UpdateCourseCommandValidator();
        var command = new UpdateCourseCommand(Guid.NewGuid(), "Database Systems", 3, "Dr. Ahmadi");

        validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateCourseCommandValidator_NonPositiveCredits_HasError()
    {
        var validator = new UpdateCourseCommandValidator();
        var command = new UpdateCourseCommand(Guid.NewGuid(), "Database Systems", 0, null);

        validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Credits);
    }

    [Fact]
    public void RecordCourseGradeCommandValidator_ValidGrade_HasNoErrors()
    {
        var validator = new RecordCourseGradeCommandValidator();
        var command = new RecordCourseGradeCommand(Guid.NewGuid(), 18.5m);

        validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RecordCourseGradeCommandValidator_GradeOutOfRange_HasError()
    {
        var validator = new RecordCourseGradeCommandValidator();
        var command = new RecordCourseGradeCommand(Guid.NewGuid(), 21m);

        validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Grade);
    }

    [Fact]
    public void CreateExamCommandValidator_ValidCommand_HasNoErrors()
    {
        var validator = new CreateExamCommandValidator();
        var command = new CreateExamCommand(Guid.NewGuid(), "Midterm", DateTime.UtcNow.AddDays(10), "Chapters 1-5");

        validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateExamCommandValidator_BlankTitle_HasError()
    {
        var validator = new CreateExamCommandValidator();
        var command = new CreateExamCommand(Guid.NewGuid(), "", DateTime.UtcNow.AddDays(10), null);

        validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void UpdateExamCommandValidator_ValidCommand_HasNoErrors()
    {
        var validator = new UpdateExamCommandValidator();
        var command = new UpdateExamCommand(Guid.NewGuid(), "Midterm", DateTime.UtcNow.AddDays(10), null);

        validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateExamCommandValidator_TitleExceedsMaxLength_HasError()
    {
        var validator = new UpdateExamCommandValidator();
        var command = new UpdateExamCommand(Guid.NewGuid(), new string('a', 201), DateTime.UtcNow, null);

        validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void RecordExamGradeCommandValidator_ValidGrade_HasNoErrors()
    {
        var validator = new RecordExamGradeCommandValidator();
        var command = new RecordExamGradeCommand(Guid.NewGuid(), 15m);

        validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RecordExamGradeCommandValidator_TooManyDecimalPlaces_HasError()
    {
        var validator = new RecordExamGradeCommandValidator();
        var command = new RecordExamGradeCommand(Guid.NewGuid(), 15.555m);

        validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Grade);
    }

    [Fact]
    public void CreateLearningActivityCommandValidator_ValidCommand_HasNoErrors()
    {
        var validator = new CreateLearningActivityCommandValidator();
        var command = new CreateLearningActivityCommand(
            Guid.NewGuid(), "Assignment 1", ActivityType.Assignment, DateTime.UtcNow.AddDays(7),
            ActivityPriority.Medium, null, null);

        validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateLearningActivityCommandValidator_BlankTitle_HasError()
    {
        var validator = new CreateLearningActivityCommandValidator();
        var command = new CreateLearningActivityCommand(
            Guid.NewGuid(), "", ActivityType.Assignment, DateTime.UtcNow.AddDays(7),
            ActivityPriority.Medium, null, null);

        validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void UpdateLearningActivityCommandValidator_ValidCommand_HasNoErrors()
    {
        var validator = new UpdateLearningActivityCommandValidator();
        var command = new UpdateLearningActivityCommand(
            Guid.NewGuid(), "Assignment 1", DateTime.UtcNow.AddDays(-3), ActivityPriority.High, null, null);

        // Past due dates ARE allowed on update (see the validator's own remarks).
        validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateLearningActivityCommandValidator_BlankTitle_HasError()
    {
        var validator = new UpdateLearningActivityCommandValidator();
        var command = new UpdateLearningActivityCommand(
            Guid.NewGuid(), "  ", DateTime.UtcNow, ActivityPriority.Medium, null, null);

        validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void UpdateLearningActivityStatusCommandValidator_ValidCommand_HasNoErrors()
    {
        var validator = new UpdateLearningActivityStatusCommandValidator();
        var command = new UpdateLearningActivityStatusCommand(Guid.NewGuid(), ActivityStatus.InProgress);

        validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateLearningActivityStatusCommandValidator_EmptyId_HasError()
    {
        var validator = new UpdateLearningActivityStatusCommandValidator();
        var command = new UpdateLearningActivityStatusCommand(Guid.Empty, ActivityStatus.Completed);

        validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.LearningActivityId);
    }

    [Fact]
    public void CreatePersonalEventCommandValidator_ValidCommand_HasNoErrors()
    {
        var validator = new CreatePersonalEventCommandValidator();
        var now = DateTime.UtcNow;
        var command = new CreatePersonalEventCommand("Study group", now, now.AddHours(2), false, null);

        validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreatePersonalEventCommandValidator_EndBeforeStart_HasError()
    {
        var validator = new CreatePersonalEventCommandValidator();
        var now = DateTime.UtcNow;
        var command = new CreatePersonalEventCommand("Study group", now, now.AddHours(-1), false, null);

        validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.EndAtUtc);
    }

    [Fact]
    public void UpdatePersonalEventCommandValidator_ValidCommand_HasNoErrors()
    {
        var validator = new UpdatePersonalEventCommandValidator();
        var now = DateTime.UtcNow;
        var command = new UpdatePersonalEventCommand(Guid.NewGuid(), "Study group", now, now.AddHours(2), null);

        validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdatePersonalEventCommandValidator_EndBeforeStart_HasError()
    {
        var validator = new UpdatePersonalEventCommandValidator();
        var now = DateTime.UtcNow;
        var command = new UpdatePersonalEventCommand(Guid.NewGuid(), "Study group", now, now.AddHours(-1), null);

        validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.EndAtUtc);
    }

    [Fact]
    public void CreateStudyLogCommandValidator_ValidCommand_HasNoErrors()
    {
        var validator = new CreateStudyLogCommandValidator();
        var command = new CreateStudyLogCommand(Guid.NewGuid(), DateTime.UtcNow.AddHours(-1), 60, "Chapter 3");

        validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateStudyLogCommandValidator_FutureStudyDate_HasError()
    {
        var validator = new CreateStudyLogCommandValidator();
        var command = new CreateStudyLogCommand(Guid.NewGuid(), DateTime.UtcNow.AddDays(1), 60, null);

        validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.StudyDateUtc);
    }

    [Fact]
    public void CreateStudyLogCommandValidator_NonPositiveDuration_HasError()
    {
        var validator = new CreateStudyLogCommandValidator();
        var command = new CreateStudyLogCommand(Guid.NewGuid(), DateTime.UtcNow.AddHours(-1), 0, null);

        validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.DurationMinutes);
    }

    [Fact]
    public void UpdateStudyLogCommandValidator_ValidCommand_HasNoErrors()
    {
        var validator = new UpdateStudyLogCommandValidator();
        var command = new UpdateStudyLogCommand(Guid.NewGuid(), DateTime.UtcNow, 45, null);

        validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateStudyLogCommandValidator_NonPositiveDuration_HasError()
    {
        var validator = new UpdateStudyLogCommandValidator();
        var command = new UpdateStudyLogCommand(Guid.NewGuid(), DateTime.UtcNow, -1, null);

        validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.DurationMinutes);
    }

    [Fact]
    public void CreateGoalCommandValidator_ProjectDeadlineWithoutRelatedActivity_HasError()
    {
        var validator = new CreateGoalCommandValidator();
        var command = new CreateGoalCommand(GoalType.ProjectDeadline, 100m, null, null);

        validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.RelatedActivityId);
    }

    [Fact]
    public void CreateGoalCommandValidator_NonProjectDeadlineWithRelatedActivity_HasError()
    {
        var validator = new CreateGoalCommandValidator();
        var command = new CreateGoalCommand(GoalType.ChapterCount, 10m, null, Guid.NewGuid());

        validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.RelatedActivityId);
    }

    [Fact]
    public void CreateGoalCommandValidator_ValidProjectDeadlineCommand_HasNoErrors()
    {
        var validator = new CreateGoalCommandValidator();
        var command = new CreateGoalCommand(GoalType.ProjectDeadline, 100m, null, Guid.NewGuid());

        validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateGoalCommandValidator_ValidCommand_HasNoErrors()
    {
        var validator = new UpdateGoalCommandValidator();
        var command = new UpdateGoalCommand(Guid.NewGuid(), 10m, null);

        validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateGoalCommandValidator_NonPositiveTargetValue_HasError()
    {
        var validator = new UpdateGoalCommandValidator();
        var command = new UpdateGoalCommand(Guid.NewGuid(), 0m, null);

        validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.TargetValue);
    }

    [Fact]
    public void CreateSystemSettingCommandValidator_ValidCommand_HasNoErrors()
    {
        var validator = new CreateSystemSettingCommandValidator();
        var command = new CreateSystemSettingCommand("MaxUploadSize", "10", "Max upload size in MB");

        validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateSystemSettingCommandValidator_BlankKey_HasError()
    {
        var validator = new CreateSystemSettingCommandValidator();
        var command = new CreateSystemSettingCommand("", "10", null);

        validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Key);
    }

    [Fact]
    public void UpdateSystemSettingValueCommandValidator_ValidCommand_HasNoErrors()
    {
        var validator = new UpdateSystemSettingValueCommandValidator();
        var command = new UpdateSystemSettingValueCommand("MaxUploadSize", "20");

        validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateSystemSettingValueCommandValidator_ValueExceedsMaxLength_HasError()
    {
        var validator = new UpdateSystemSettingValueCommandValidator();
        var command = new UpdateSystemSettingValueCommand("MaxUploadSize", new string('a', 1001));

        validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Value);
    }

    [Fact]
    public void ChangeUserRoleCommandValidator_ValidCommand_HasNoErrors()
    {
        var validator = new ChangeUserRoleCommandValidator();
        var command = new ChangeUserRoleCommand(Guid.NewGuid(), UserRole.Admin);

        validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ChangeUserRoleCommandValidator_EmptyUserId_HasError()
    {
        var validator = new ChangeUserRoleCommandValidator();
        var command = new ChangeUserRoleCommand(Guid.Empty, UserRole.Admin);

        validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void GetUsersQueryValidator_ValidQuery_HasNoErrors()
    {
        var validator = new GetUsersQueryValidator();
        var query = new GetUsersQuery("ali", UserRole.Student, true, new PaginationParams());

        validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void GetUsersQueryValidator_SearchExceedsMaxLength_HasError()
    {
        var validator = new GetUsersQueryValidator();
        var query = new GetUsersQuery(new string('a', 201), null, null, new PaginationParams());

        validator.TestValidate(query).ShouldHaveValidationErrorFor(x => x.Search);
    }

    [Fact]
    public void GetNotificationsQueryValidator_NoTypeSupplied_HasNoErrors()
    {
        var validator = new GetNotificationsQueryValidator();
        var query = new GetNotificationsQuery(new PaginationParams());

        validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void GetNotificationsQueryValidator_ValidType_HasNoErrors()
    {
        var validator = new GetNotificationsQueryValidator();
        var query = new GetNotificationsQuery(new PaginationParams(), Type: NotificationType.ExamTomorrow);

        validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateMyProfileCommandValidator_ValidCommand_HasNoErrors()
    {
        var validator = new UpdateMyProfileCommandValidator();
        var command = new UpdateMyProfileCommand("Ali", "Rezaei", "ali.rezaei@example.com");

        validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateMyProfileCommandValidator_InvalidEmail_HasError()
    {
        var validator = new UpdateMyProfileCommandValidator();
        var command = new UpdateMyProfileCommand("Ali", "Rezaei", "not-an-email");

        validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Email);
    }
}