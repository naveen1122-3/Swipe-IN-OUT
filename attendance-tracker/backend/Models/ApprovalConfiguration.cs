namespace backend.Models
{
    public class ApprovalConfiguration
    {
        public int Id { get; set; }

        public int ManagerId { get; set; }

        public int ProjectId { get; set; }

        public decimal RequiredSwipeHours { get; set; }

        public decimal RequiredOfficeHours { get; set; }

        public decimal RequiredTotalHours { get; set; }

        public int RequiredOfficeDaysPerWeek { get; set; }

        public bool IsActive { get; set; }
    }
}