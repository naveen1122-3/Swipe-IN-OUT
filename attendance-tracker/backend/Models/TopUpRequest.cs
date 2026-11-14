namespace backend.Models
{
    public class TopUpRequest
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public decimal OfficeHours { get; set; }

        public decimal RequestedHours { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime RequestDate { get; set; }
    }
}