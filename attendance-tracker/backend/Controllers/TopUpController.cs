using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TopUpController : ControllerBase
    {
        private readonly TopUpApprovalService _service;
        private readonly AttendanceValidationService _attendanceService;
        private readonly ApplicationDbContext _context;

        public TopUpController(
            TopUpApprovalService service,
            AttendanceValidationService attendanceService,
            ApplicationDbContext context)
        {
            _service = service;
            _attendanceService = attendanceService;
            _context = context;
        }

        [HttpPost("approve")]
        public IActionResult Approve(
            [FromBody] List<TopUpRequestDto> requests)
        {
            if (requests == null || requests.Count == 0)
            {
                return BadRequest("At least one top-up interval is required.");
            }

            var normalizedRequests = requests
                .Select(request => new
                {
                    Request = request,
                    Start = request.TopUpStart.UtcDateTime,
                    End = request.TopUpEnd.UtcDateTime
                })
                .ToList();

            if (normalizedRequests.Any(interval =>
                interval.Request.EmployeeId <= 0
                || interval.Request.ManagerId <= 0
                || interval.Request.ProjectId <= 0
                || interval.End <= interval.Start
                || interval.Start.Date != interval.End.Date))
            {
                return BadRequest(
                    "Each interval needs positive employee, manager, and project IDs, a valid start/end, and must stay within one UTC day.");
            }

            foreach (var dayGroup in normalizedRequests.GroupBy(interval => new
            {
                interval.Request.EmployeeId,
                interval.Request.ManagerId,
                interval.Request.ProjectId,
                Day = interval.Start.Date
            }))
            {
                var orderedIntervals = dayGroup
                    .OrderBy(interval => interval.Start)
                    .ToList();

                for (var index = 1; index < orderedIntervals.Count; index++)
                {
                    if (orderedIntervals[index].Start
                        < orderedIntervals[index - 1].End)
                    {
                        return BadRequest(
                            "Top-up intervals for the same employee, manager, project, and day cannot overlap.");
                    }
                }
            }

            if (normalizedRequests.Any(interval =>
                _context.TopUpRequests.Any(existing =>
                    existing.EmployeeId == interval.Request.EmployeeId
                    && existing.ManagerId == interval.Request.ManagerId
                    && existing.ProjectId == interval.Request.ProjectId
                    && existing.TopUpStart.HasValue
                    && existing.TopUpEnd.HasValue
                    && interval.Start < existing.TopUpEnd.Value
                    && interval.End > existing.TopUpStart.Value)))
            {
                return BadRequest(
                    "A top-up interval overlaps an interval already submitted for that employee, manager, and project.");
            }

            var results = new List<(TopUpRequest Entity, TopUpResponseDto Response)>();

            foreach (var dayGroup in normalizedRequests.GroupBy(interval => new
            {
                interval.Request.EmployeeId,
                interval.Request.ManagerId,
                interval.Request.ProjectId,
                Day = interval.Start.Date
            }))
            {
                var dayStart = dayGroup.Key.Day;
                var dayEnd = dayStart.AddDays(1);
                var weekStart = dayStart.AddDays(
                    -(((int)dayStart.DayOfWeek + 6) % 7));
                var weekEnd = weekStart.AddDays(7);

                var config = _context.ApprovalConfigurations
                    .FirstOrDefault(configuration =>
                        configuration.ManagerId == dayGroup.Key.ManagerId
                        && configuration.ProjectId == dayGroup.Key.ProjectId
                        && configuration.IsActive)
                    ?? new ApprovalConfiguration
                    {
                        ManagerId = dayGroup.Key.ManagerId,
                        ProjectId = dayGroup.Key.ProjectId,
                        RequiredSwipeHours = 5,
                        RequiredOfficeHours = 5,
                        RequiredTotalHours = 10,
                        RequiredOfficeDaysPerWeek = 5,
                        IsActive = true
                    };

                var identitySwipeLogs = _context.SwipeLogs
                    .Where(log =>
                        log.EmployeeId == dayGroup.Key.EmployeeId
                        && log.ManagerId == dayGroup.Key.ManagerId
                        && log.ProjectId == dayGroup.Key.ProjectId
                        && log.SwipeTime >= weekStart
                        && log.SwipeTime < weekEnd)
                    .ToList();

                var daySwipeLogs = identitySwipeLogs
                    .Where(log => log.SwipeTime >= dayStart
                        && log.SwipeTime < dayEnd)
                    .ToList();

                var attendance = _attendanceService
                    .CalculateAttendance(daySwipeLogs);
                var swipeHours = attendance.AttendanceStatus is
                    "ELIGIBLE_FOR_TOPUP" or "INSUFFICIENT_PRESENCE"
                    ? (decimal)attendance.TotalOfficeHours
                    : 0;

                var attendedDays = identitySwipeLogs
                    .GroupBy(log => log.SwipeTime.Date)
                    .Count(logs =>
                    {
                        var result = _attendanceService
                            .CalculateAttendance(logs.ToList());

                        return result.AttendanceStatus is
                            "ELIGIBLE_FOR_TOPUP" or "INSUFFICIENT_PRESENCE";
                    });

                var dailyTopUpHours = Math.Round(
                    dayGroup.Sum(interval =>
                        (decimal)(interval.End - interval.Start).TotalHours),
                    2);

                var decision = _service.ProcessRequest(
                    swipeHours,
                    dailyTopUpHours,
                    attendedDays,
                    config);

                foreach (var interval in dayGroup)
                {
                    var requestedHours = Math.Round(
                        (decimal)(interval.End - interval.Start).TotalHours,
                        2);

                    var entity = new TopUpRequest
                    {
                        EmployeeId = dayGroup.Key.EmployeeId,
                        ManagerId = dayGroup.Key.ManagerId,
                        ProjectId = dayGroup.Key.ProjectId,
                        TopUpStart = interval.Start,
                        TopUpEnd = interval.End,
                        OfficeHours = swipeHours,
                        RequestedHours = requestedHours,
                        Status = decision.ApprovalStatus,
                        Reason = decision.Reason,
                        RequestDate = DateTime.UtcNow
                    };

                    _context.TopUpRequests.Add(entity);

                    var response = new TopUpResponseDto
                    {
                        EmployeeId = entity.EmployeeId,
                        ManagerId = entity.ManagerId,
                        ProjectId = entity.ProjectId,
                        TopUpStart = new DateTimeOffset(
                            interval.Start,
                            TimeSpan.Zero),
                        TopUpEnd = new DateTimeOffset(
                            interval.End,
                            TimeSpan.Zero),
                        SwipeHours = decision.SwipeHours,
                        RequestedTopUpHours = requestedHours,
                        DailyTopUpHours = decision.DailyTopUpHours,
                        TotalHours = decision.TotalHours,
                        RequiredSwipeHours = decision.RequiredSwipeHours,
                        RequiredOfficeHours = decision.RequiredOfficeHours,
                        RequiredTotalHours = decision.RequiredTotalHours,
                        RequiredOfficeDaysPerWeek =
                            decision.RequiredOfficeDaysPerWeek,
                        AttendedDaysThisWeek = decision.AttendedDaysThisWeek,
                        ApprovalStatus = decision.ApprovalStatus,
                        Reason = decision.Reason
                    };

                    results.Add((entity, response));
                }
            }

            _context.SaveChanges();

            foreach (var result in results)
            {
                result.Response.Id = result.Entity.Id;
            }

            return Ok(results.Select(result => result.Response));
        }

        [HttpGet("manager/{managerId:int}")]
        public IActionResult GetManagerRequests(
            int managerId,
            [FromQuery] int? projectId)
        {
            if (managerId <= 0 || projectId is <= 0)
            {
                return BadRequest("Manager and project IDs must be positive.");
            }

            var requests = _context.TopUpRequests
                .AsNoTracking()
                .Where(request => request.ManagerId == managerId);

            if (projectId.HasValue)
            {
                requests = requests.Where(request =>
                    request.ProjectId == projectId.Value);
            }

            return Ok(requests
                .OrderByDescending(request => request.RequestDate)
                .ToList());
        }

        [HttpGet("employee/{employeeId:int}")]
        public IActionResult GetEmployeeRequests(
            int employeeId,
            [FromQuery] int managerId,
            [FromQuery] int projectId)
        {
            if (employeeId <= 0 || managerId <= 0 || projectId <= 0)
            {
                return BadRequest(
                    "Employee, manager, and project IDs must be positive.");
            }

            return Ok(_context.TopUpRequests
                .AsNoTracking()
                .Where(request =>
                    request.EmployeeId == employeeId
                    && request.ManagerId == managerId
                    && request.ProjectId == projectId)
                .OrderByDescending(request => request.RequestDate)
                .ToList());
        }

        [HttpPost("manager/{managerId:int}/requests/{requestId:int}/review")]
        public IActionResult ReviewRequest(
            int managerId,
            int requestId,
            [FromBody] TopUpReviewDto review)
        {
            if (managerId <= 0 || requestId <= 0
                || review.ManagerId != managerId
                || review.Approved is null)
            {
                return BadRequest(
                    "A matching manager ID and an approval decision are required.");
            }

            var request = _context.TopUpRequests.FirstOrDefault(item =>
                item.Id == requestId && item.ManagerId == managerId);

            if (request == null)
            {
                return NotFound("Top-up request was not found for this manager.");
            }

            if (request.Status != "PENDING_MANAGER_REVIEW")
            {
                return Conflict("Only requests pending manager review can be decided.");
            }

            request.Status = review.Approved.Value
                ? "MANUALLY_APPROVED"
                : "MANUALLY_REJECTED";
            request.Reason = string.IsNullOrWhiteSpace(review.Comment)
                ? review.Approved.Value
                    ? "Approved by manager."
                    : "Rejected by manager."
                : review.Comment.Trim();

            _context.SaveChanges();

            return Ok(request);
        }
    }
}
