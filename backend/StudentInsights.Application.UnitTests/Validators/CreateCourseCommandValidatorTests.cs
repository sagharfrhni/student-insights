using FluentValidation.TestHelper;
using StudentInsights.Application.Features.Courses.Commands.CreateCourse;

namespace StudentInsights.UnitTests.Validators;

public class CreateCourseCommandValidatorTests
{
    private readonly CreateCourseCommandValidator _validator = new();

    private static CreateCourseCommand ValidCommand() =>
        new("Database Systems", "Fall 2026", 3, "Dr. Ahmadi");

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_BlankName_HasError(string? name)
    {
        var command = ValidCommand() with { Name = name! };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameExceedsMaxLength_HasError()
    {
        var command = ValidCommand() with { Name = new string('a', 201) };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameAtExactMaxLength_HasNoError()
    {
        var command = ValidCommand() with { Name = new string('a', 200) };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_BlankSemester_HasError()
    {
        var command = ValidCommand() with { Semester = "" };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Semester);
    }

    [Fact]
    public void Validate_SemesterExceedsMaxLength_HasError()
    {
        var command = ValidCommand() with { Semester = new string('a', 101) };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Semester);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_NonPositiveCredits_HasError(int credits)
    {
        var command = ValidCommand() with { Credits = credits };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Credits);
    }

    [Fact]
    public void Validate_InstructorNameExceedsMaxLength_HasError()
    {
        var command = ValidCommand() with { InstructorName = new string('a', 201) };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.InstructorName);
    }

    [Fact]
    public void Validate_NullInstructorName_HasNoError()
    {
        var command = ValidCommand() with { InstructorName = null };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.InstructorName);
    }
}