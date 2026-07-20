using FluentValidation;
using HR.Application.Sections.Commands;

namespace HR.Application.Sections.Validators;

public class UpdateSectionCommandValidator : AbstractValidator<UpdateSectionCommand>
{
    public UpdateSectionCommandValidator()
    {
        RuleFor(v => v.Id)
            .GreaterThan(0);

        RuleFor(v => v.Name)
            .MaximumLength(100)
            .NotEmpty();

        RuleFor(v => v.DepartmentId)
            .GreaterThan(0);
    }
}
