using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class TopUpReviewDto
    {
        [Range(1, int.MaxValue)]
        public int ManagerId { get; set; }

        public bool? Approved { get; set; }

        public string Comment { get; set; } = string.Empty;
    }
}
