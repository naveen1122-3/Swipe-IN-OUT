namespace backend.DTOs
{
    public class TopUpRequestDto
    {
        public decimal SwipeHours { get; set; }

        public decimal RequestedTopUpHours { get; set; }
    }
}