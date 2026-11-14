namespace backend.DTOs
{
    public class ApprovalConfigurationDto
    {
        public decimal RequiredSwipeHours { get; set; }

        public decimal AllowedTopUpHours { get; set; }

        public decimal RequiredTotalHours { get; set; }
    }
}