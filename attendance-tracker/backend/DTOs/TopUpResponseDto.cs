namespace backend.DTOs
{
    public class TopUpResponseDto
    {
        public decimal SwipeHours { get; set; }

        public decimal RequestedTopUpHours { get; set; }

        public decimal TotalHours { get; set; }

        public decimal RequiredSwipeHours { get; set; }

        public decimal AllowedTopUpHours { get; set; }

        public decimal RequiredTotalHours { get; set; }

        public string ApprovalStatus { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;
    }
}
