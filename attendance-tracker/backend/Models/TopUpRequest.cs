namespace backend.Models
{
    public class TopUpRequest
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public int ManagerId { get; set; }

        public int ProjectId { get; set; }

        public DateTime? TopUpStart { get; set; }

        public DateTime? TopUpEnd { get; set; }

        public decimal OfficeHours { get; set; }

        public decimal RequestedHours { get; set; }

        public string Status { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;

        public DateTime RequestDate { get; set; }
    }
}