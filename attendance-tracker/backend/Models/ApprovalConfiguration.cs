namespace backend.Models
{
    public class ApprovalConfiguration
    {
        public int Id { get; set; }

        public decimal RequiredSwipeHours { get; set; }

        public decimal AllowedTopUpHours { get; set; }

        public decimal RequiredTotalHours { get; set; }

        public bool IsActive { get; set; }
    }
}