using HR.Application.Common.Interfaces;
using MediatR;

namespace HR.Application.Sections.Commands;

public record DeleteSectionCommand(int Id) : IRequest<bool>;

public class DeleteSectionCommandHandler : IRequestHandler<DeleteSectionCommand, bool>
{
    private readonly ISectionRepository _sectionRepository;

    public DeleteSectionCommandHandler(ISectionRepository sectionRepository)
    {
        _sectionRepository = sectionRepository;
    }

    public async Task<bool> Handle(DeleteSectionCommand request, CancellationToken cancellationToken)
    {
        var section = await _sectionRepository.GetByIdAsync(request.Id);
        if (section == null)
        {
            throw new KeyNotFoundException($"Section with ID {request.Id} not found.");
        }

        if (section.Employees != null && section.Employees.Any())
        {
            throw new InvalidOperationException("Cannot delete section because it has active employees assigned to it.");
        }

        return await _sectionRepository.DeleteAsync(request.Id);
    }
}
