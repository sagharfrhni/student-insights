// StudentInsights.Application/Features/PersonalEvents/Commands/UpdatePersonalEvent/UpdatePersonalEventCommandValidator.cs
using FluentValidation;

namespace StudentInsights.Application.Features.PersonalEvents.Commands.UpdatePersonalEvent;

/// <summary>
/// Same field rules as CreatePersonalEventCommandValidator, plus a
/// required PersonalEventId, mirroring UpdateExamCommandValidator's
/// ExamId + Title/Description/date rules.
/// </summary>
public class UpdatePersonalEventCommandValidator : AbstractValidator<UpdatePersonalEventCommand>
{
    public UpdatePersonalEventCommandValidator()
    {
        RuleFor(x => x.PersonalEventId)
            .NotEmpty().WithMessage("PersonalEventId is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.EndAtUtc)
            .GreaterThan(x => x.StartAtUtc).WithMessage("End time must be after the start time.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");
    }
}