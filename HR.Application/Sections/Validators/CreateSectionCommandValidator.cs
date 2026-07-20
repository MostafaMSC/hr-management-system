using FluentValidation;
using HR.Application.Sections.Commands;

namespace HR.Application.Sections.Validators;

public class CreateSectionCommandValidator : AbstractValidator<CreateSectionCommand>
{
    public CreateSectionCommandValidator()
    {
        RuleFor(v => v.Name)
            .MaximumLength(100)
            .NotEmpty();

        RuleFor(v => v.DepartmentId)
            .GreaterThan(0);
    }
}
