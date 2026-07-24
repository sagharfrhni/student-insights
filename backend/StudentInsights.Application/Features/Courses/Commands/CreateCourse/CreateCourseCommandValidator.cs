using FluentValidation;

namespace StudentInsights.Application.Features.Courses.Commands.CreateCourse;

/// <summary>
/// Mirrors both Course's Domain invariants (Create throws on blank name /
/// non-positive credits) and CourseConfiguration's HasMaxLength(200) on
/// Name/InstructorName, so bad input is rejected with a clean 400 here
/// instead of surfacing as a DomainException or a DB truncation error.
/// </summary>
public class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
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