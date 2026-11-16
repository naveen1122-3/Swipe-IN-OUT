namespace backend.DTOs
{
    public class TopUpResponseDto
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public int ManagerId { get; set; }

        public int ProjectId { get; set; }

        public DateTimeOffset TopUpStart { get; set; }

        public DateTimeOffset TopUpEnd { get; set; }

        public decimal SwipeHours { get; set; }

        public decimal RequestedTopUpHours { get; set; }

        public decimal DailyTopUpHours { get; set; }

        public decimal TotalHours { get; set; }

        public decimal RequiredSwipeHours { get; set; }

        public decimal RequiredOfficeHours { get; set; }

        public decimal RequiredTotalHours { get; set; }

        public int RequiredOfficeDaysPerWeek { get; set; }

        public int AttendedDaysThisWeek { get; set; }

        public string ApprovalStatus { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;
    }
}
