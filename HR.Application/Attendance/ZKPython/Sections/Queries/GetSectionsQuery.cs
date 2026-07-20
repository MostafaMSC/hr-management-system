using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Common.Interfaces;
using HR.Application.Common;
using MediatR;

namespace HR.Application.Attendance.ZKPython.Sections.Queries;

public record GetSectionsQuery(int? DepartmentId = null) : IRequest<List<SectionDto>>;

public class GetSectionsQueryHandler : IRequestHandler<GetSectionsQuery, List<SectionDto>>
{
    private readonly ISectionRepository _repository;
    private readonly ICacheService _cache;

    public GetSectionsQueryHandler(ISectionRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<List<SectionDto>> Handle(GetSectionsQuery request, CancellationToken cancellationToken)
    {
        return new System.Collections.Generic.List<SectionDto>();
    }
}
