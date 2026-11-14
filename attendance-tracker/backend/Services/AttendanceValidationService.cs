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
                if (swipeLogs[i].SwipeType == "IN" &&
                    swipeLogs[i + 1].SwipeType == "OUT")
                {
                    totalMinutes +=
                        (int)(swipeLogs[i + 1].SwipeTime -
                              swipeLogs[i].SwipeTime)
                        .TotalMinutes;
                }
            }

            double totalHours = Math.Round(totalMinutes / 60.0, 2);

            string status = totalHours < 1
                ? "SHORT_PRESENCE"
                : "VALID";

            return new AttendanceResultDto
            {
                EmployeeId = swipeLogs.First().EmployeeId,
                TotalOfficeHours = totalHours,
                AttendanceStatus = status
            };
        }
    }
}
