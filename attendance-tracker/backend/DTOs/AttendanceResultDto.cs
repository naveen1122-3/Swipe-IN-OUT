namespace backend.DTOs
{
    public class AttendanceResultDto
    {
        public int EmployeeId { get; set; }

        public double TotalOfficeHours { get; set; }

        public string AttendanceStatus { get; set; } = string.Empty;
    }
}