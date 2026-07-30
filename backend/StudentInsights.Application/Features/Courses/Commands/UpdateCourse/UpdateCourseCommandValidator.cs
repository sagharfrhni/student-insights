using FluentValidation;

namespace StudentInsights.Application.Features.Courses.Commands.UpdateCourse;

/// <summary>Same field rules as CreateCourseCommandValidator, plus CourseId.</summary>
public class UpdateCourseCommandValidator : AbstractValidator<UpdateCourseCommand>
{
    public UpdateCourseCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("CourseId is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Course name is required.")
            .MaximumLength(200).WithMessage("Course name must not exceed 200 characters.");

        RuleFor(x => x.Credits)
            .GreaterThan(0).WithMessage("Credits must be greater than zero.");

        // No .When(InstructorName is not null) guard needed: FluentValidation's
        // MaximumLength already treats a null value as valid and only checks
        // length when a value is present.
        RuleFor(x => x.InstructorName)
            .MaximumLength(200).WithMessage("Instructor name must not exceed 200 characters.");
    }
}