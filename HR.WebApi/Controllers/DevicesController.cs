using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Threading.Tasks;
using HR.Application.Attendance.ZKPython.Devices.Queries;
using HR.Application.Attendance.ZKPython.Devices.Commands;

namespace HR.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DevicesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DevicesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Retrieves all registered devices from the database.
        /// </summary>
        [HttpGet]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetAll()
        {
            var devices = await _mediator.Send(new GetAllDevicesQuery());
            return Ok(devices);
        }

        /// <summary>
        /// Retrieves all registered devices along with their real-time network connection status.
        /// </summary>
        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            var statuses = await _mediator.Send(new GetDevicesWithStatusQuery());
            return Ok(statuses);
        }

        /// <summary>
        /// Retrieves the list of devices a specific user is authorized to use.
        /// </summary>
        /// <param name="userId">The internal user ID</param>
        [HttpGet("user/{userId}")]
        [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "userId" })]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var devices = await _mediator.Send(new GetUserDevicesQuery(userId));
            return Ok(devices);
        }

        /// <summary>
        /// Updates the list of devices a user is authorized for, synchronizing their profile and fingerprints to the selected devices.
        /// </summary>
        [HttpPost("update-user-devices")]
        public async Task<IActionResult> UpdateUserDevices([FromBody] UpdateUserDevicesRequest request)
        {
            var result = await _mediator.Send(new UpdateUserDevicesCommand { UserId = request.UserId, DeviceIds = request.DeviceIds });
            if (result.Success) return Ok(result);
            return BadRequest(result);
        }

        public class UpdateUserDevicesRequest
        {
            public int UserId { get; set; }
            public System.Collections.Generic.List<int> DeviceIds { get; set; } = new();
        }
    }
}
