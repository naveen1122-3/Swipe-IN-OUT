using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly AttendanceValidationService _service;

        public AttendanceController(
            AttendanceValidationService service)
        {
            _service = service;
        }

        [HttpPost("validate")]
        public IActionResult ValidateAttendance(
            List<SwipeLog> logs)
        {
            var result =
                _service.CalculateAttendance(logs);

            return Ok(result);
        }
    }
}