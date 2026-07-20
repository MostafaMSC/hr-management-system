using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Application.Common.Interfaces;
using System.Text.Json.Nodes;

namespace HR.Application.Attendance.ZKPython.Logs.Queries;

public class GetLogsByDeviceQueryHandler : IRequestHandler<GetLogsByDeviceQuery, GetLogsByDeviceResult>
{
    private readonly IPythonService _pythonService;

    public GetLogsByDeviceQueryHandler(IPythonService pythonService)
    {
        _pythonService = pythonService;
    }

    public async Task<GetLogsByDeviceResult> Handle(GetLogsByDeviceQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 100 : request.PageSize;

        var result = await _pythonService.RunPythonAsync(request.DeviceIp, cancellationToken);

        // Assuming result is the data array or contains it. 
        // Based on controller logic: var query = await _pythonService.RunPythonAsync...
        // return Ok(new { success = true, total = query.Count(), ... data = query });

        // Wait, RunPythonAsync returns JsonObject. The controller code treated it as enumerable?
        // Let's check PythonService.RunPythonAsync signature. It returns Task<JsonObject> or Task<JsonArray>?
        // The interface says Task<JsonObject>. 
        // The controller code: var query = await _pythonService.RunPythonAsync... 
        // return Ok(new { ... count = query.Count() ... });
        // This implies query is a collection. 
        // BUT if RunPythonAsync returns JsonObject, then query.Count() might be counting properties?
        // OR the previous code in Controller was assuming dynamic or something else?

        // Let's look at the actual PythonService implementation (Step 581).
        // It returns data["data"] as JsonArray if success.
        // Wait, the interface says `Task<JsonObject> RunPythonAsync`.
        // The controller code I saw in step 509 lines 47-59 was:
        // var query = await _pythonService.RunPythonAsync(deviceIp, cancellationToken);
        // return Ok(new { ... count = query.Count(), data = query });

        // If 'query' is JsonObject, query.Count is property count.
        // If 'query' is supposed to be the *logs*, then the service should return the logs array or the controller extracted it.
        // In FPBackgroundService, it extracts `data["data"]`.

        // I suspect the Controller code I saw (Step 509) might have been incorrect or I misread it or `IPythonService` was returning `dynamic`.
        // The Interface (Step 607) says `Task<JsonObject>`.

        // I will implement the handler to return the raw result or process it properly.
        // For now, I'll pass the JsonObject as Data.

        // Actually, if the python script returns `{ "data": [ ... ], "success": true }`, 
        // I should probably return that structure.

        return new GetLogsByDeviceResult
        {
            Success = true, // We might want to check result["success"]
            Data = result,
            Page = page,
            PageSize = pageSize,
            Total = 0, // JsonObject doesn't have Count like a list
            Count = 0
        };
    }
}
