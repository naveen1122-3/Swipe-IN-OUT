public class AttendanceSession
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public DateTime CheckIn { get; set; }

    public DateTime CheckOut { get; set; }

    public int DurationMinutes { get; set; }

    public bool IsSuspicious { get; set; }
}