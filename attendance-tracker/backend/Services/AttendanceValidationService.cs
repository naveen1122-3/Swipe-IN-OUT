using backend.DTOs;
using backend.Models;

namespace backend.Services
{
    public class AttendanceValidationService
    {
        public AttendanceResultDto CalculateAttendance(
            List<SwipeLog>? swipeLogs)
        {
            int totalMinutes = 0;

            if (swipeLogs == null || swipeLogs.Count == 0)
            {
                return new AttendanceResultDto
                {
                    AttendanceStatus = "NO_SWIPE_LOGS"
                };
            }

            swipeLogs = swipeLogs
                .OrderBy(x => x.SwipeTime)
                .ToList();

            var firstLog = swipeLogs[0];
            if (swipeLogs.Any(log =>
                log.EmployeeId != firstLog.EmployeeId
                || log.ManagerId != firstLog.ManagerId
                || log.ProjectId != firstLog.ProjectId))
            {
                return new AttendanceResultDto
                {
                    EmployeeId = firstLog.EmployeeId,
                    AttendanceStatus = "MIXED_ATTENDANCE_IDENTIFIERS"
                };
            }

            if (swipeLogs.Count % 2 != 0)
            {
                return new AttendanceResultDto
                {
                    EmployeeId = firstLog.EmployeeId,
                    AttendanceStatus = "INCOMPLETE_SWIPE_PAIR"
                };
            }

            for (int i = 0; i < swipeLogs.Count; i += 2)
            {
                var swipeIn = swipeLogs[i];
                var swipeOut = swipeLogs[i + 1];

                if (swipeIn.EmployeeId != swipeOut.EmployeeId
                    || swipeIn.ManagerId != swipeOut.ManagerId
                    || swipeIn.ProjectId != swipeOut.ProjectId)
                {
                    return new AttendanceResultDto
                    {
                        EmployeeId = swipeIn.EmployeeId,
                        TotalOfficeHours = 0,
                        AttendanceStatus = "INVALID_EMPLOYEE_SEQUENCE"
                    };
                }

                if (!string.Equals(
                    swipeIn.SwipeType,
                    "IN",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return new AttendanceResultDto
                    {
                        EmployeeId = swipeIn.EmployeeId,
                        TotalOfficeHours = 0,
                        AttendanceStatus = "INVALID_IN_SWIPE"
                    };
                }

                if (!string.Equals(
                    swipeOut.SwipeType,
                    "OUT",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return new AttendanceResultDto
                    {
                        EmployeeId = swipeIn.EmployeeId,
                        TotalOfficeHours = 0,
                        AttendanceStatus = "INVALID_OUT_SWIPE"
                    };
                }

                if (swipeIn.SwipeTime.Date
                        != swipeOut.SwipeTime.Date
                    || swipeOut.SwipeTime <= swipeIn.SwipeTime)
                {
                    return new AttendanceResultDto
                    {
                        EmployeeId = swipeIn.EmployeeId,
                        AttendanceStatus = "INVALID_SWIPE_TIME"
                    };
                }

                totalMinutes +=
                    (int)(swipeOut.SwipeTime -
                          swipeIn.SwipeTime)
                    .TotalMinutes;
            }

            double totalHours =
                Math.Round(totalMinutes / 60.0, 2);

            string status =
                totalHours >= 5
                ? "ELIGIBLE_FOR_TOPUP"
                : "INSUFFICIENT_PRESENCE";

            return new AttendanceResultDto
            {
                EmployeeId = firstLog.EmployeeId,
                TotalOfficeHours = totalHours,
                AttendanceStatus = status
            };
        }
    }
}