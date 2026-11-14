using backend.DTOs;
using backend.Models;

namespace backend.Services
{
    public class AttendanceValidationService
    {
        public AttendanceResultDto CalculateAttendance(
            List<SwipeLog> swipeLogs)
        {
            int totalMinutes = 0;

            swipeLogs = swipeLogs
                .OrderBy(x => x.SwipeTime)
                .ToList();

            for (int i = 0; i < swipeLogs.Count - 1; i += 2)
            {
                var swipeIn = swipeLogs[i];
                var swipeOut = swipeLogs[i + 1];

                if (swipeIn.EmployeeId != swipeOut.EmployeeId)
                {
                    return new AttendanceResultDto
                    {
                        EmployeeId = swipeIn.EmployeeId,
                        TotalOfficeHours = 0,
                        AttendanceStatus = "INVALID_EMPLOYEE_SEQUENCE"
                    };
                }

                if (swipeIn.SwipeType != "IN")
                {
                    return new AttendanceResultDto
                    {
                        EmployeeId = swipeIn.EmployeeId,
                        TotalOfficeHours = 0,
                        AttendanceStatus = "INVALID_IN_SWIPE"
                    };
                }

                if (swipeOut.SwipeType != "OUT")
                {
                    return new AttendanceResultDto
                    {
                        EmployeeId = swipeIn.EmployeeId,
                        TotalOfficeHours = 0,
                        AttendanceStatus = "INVALID_OUT_SWIPE"
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
                EmployeeId = swipeLogs.First().EmployeeId,
                TotalOfficeHours = totalHours,
                AttendanceStatus = status
            };
        }
    }
}