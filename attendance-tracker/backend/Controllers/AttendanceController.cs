using backend.Data;
using backend.DTOs;
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
        private readonly ApplicationDbContext _context;

        public AttendanceController(
            AttendanceValidationService service,
            ApplicationDbContext context)
        {
            _service = service;
            _context = context;
        }

        [HttpPost("swipe")]
        public IActionResult RecordSwipe(
            [FromBody] SwipeLog swipe)
        {
            if (swipe.EmployeeId <= 0
                || swipe.ManagerId <= 0
                || swipe.ProjectId <= 0
                || swipe.SwipeTime == default
                || (!string.Equals(
                    swipe.SwipeType,
                    "IN",
                    StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(
                        swipe.SwipeType,
                        "OUT",
                        StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(
                    "Positive employee, manager, and project IDs and a swipeType of IN or OUT are required.");
            }

            swipe.SwipeType = swipe.SwipeType.ToUpperInvariant();
            swipe.SwipeTime = swipe.SwipeTime.ToUniversalTime();

            var lastSwipe = _context.SwipeLogs
                .Where(log =>
                    log.EmployeeId == swipe.EmployeeId
                    && log.ManagerId == swipe.ManagerId
                    && log.ProjectId == swipe.ProjectId)
                .OrderByDescending(log => log.SwipeTime)
                .FirstOrDefault();

            if (lastSwipe != null
                && swipe.SwipeTime <= lastSwipe.SwipeTime)
            {
                return BadRequest("Swipe times must be later than the previous swipe.");
            }

            var previousSwipeWasIn = string.Equals(
                lastSwipe?.SwipeType,
                "IN",
                StringComparison.OrdinalIgnoreCase);

            if (swipe.SwipeType == "IN" && previousSwipeWasIn)
            {
                return BadRequest(
                    "A swipe-out is required before the next swipe-in.");
            }

            if (swipe.SwipeType == "OUT"
                && (lastSwipe == null
                    || !previousSwipeWasIn
                    || lastSwipe.SwipeTime.Date
                        != swipe.SwipeTime.Date))
            {
                return BadRequest(
                    "A same-day swipe-in is required before a swipe-out.");
            }

            _context.SwipeLogs.Add(swipe);
            _context.SaveChanges();

            if (swipe.SwipeType == "IN")
            {
                return Ok(new AttendanceResultDto
                {
                    EmployeeId = swipe.EmployeeId,
                    AttendanceStatus = "SWIPE_IN_RECORDED"
                });
            }

            var dayStart = swipe.SwipeTime.Date;
            var dayEnd = dayStart.AddDays(1);
            var dayLogs = _context.SwipeLogs
                .Where(log =>
                    log.EmployeeId == swipe.EmployeeId
                    && log.ManagerId == swipe.ManagerId
                    && log.ProjectId == swipe.ProjectId
                    && log.SwipeTime >= dayStart
                    && log.SwipeTime < dayEnd)
                .ToList();

            var result = _service.CalculateAttendance(dayLogs);
            var config = _context.ApprovalConfigurations
                .FirstOrDefault(configuration =>
                    configuration.ManagerId == swipe.ManagerId
                    && configuration.ProjectId == swipe.ProjectId
                    && configuration.IsActive);
            var requiredSwipeHours = config?.RequiredSwipeHours ?? 5;

            result.AttendanceStatus =
                (decimal)result.TotalOfficeHours >= requiredSwipeHours
                    ? "ELIGIBLE_FOR_TOPUP"
                    : "INSUFFICIENT_PRESENCE";

            return Ok(result);
        }

        [HttpGet("status")]
        public IActionResult GetAttendanceStatus(
            [FromQuery] int employeeId,
            [FromQuery] int managerId,
            [FromQuery] int projectId)
        {
            if (employeeId <= 0 || managerId <= 0 || projectId <= 0)
            {
                return BadRequest(
                    "Employee, manager, and project IDs must be positive.");
            }

            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);
            var logs = _context.SwipeLogs
                .Where(log =>
                    log.EmployeeId == employeeId
                    && log.ManagerId == managerId
                    && log.ProjectId == projectId
                    && log.SwipeTime >= today
                    && log.SwipeTime < tomorrow)
                .OrderBy(log => log.SwipeTime)
                .ToList();
            var lastSwipe = logs.LastOrDefault();
            var attendance = _service.CalculateAttendance(logs);

            return Ok(new
            {
                employeeId,
                managerId,
                projectId,
                attendance.TotalOfficeHours,
                attendance.AttendanceStatus,
                HasOpenSwipe = string.Equals(
                    lastSwipe?.SwipeType,
                    "IN",
                    StringComparison.OrdinalIgnoreCase),
                NextSwipeType = string.Equals(
                    lastSwipe?.SwipeType,
                    "IN",
                    StringComparison.OrdinalIgnoreCase)
                    ? "OUT"
                    : "IN",
                SwipeCount = logs.Count
            });
        }
    }
}