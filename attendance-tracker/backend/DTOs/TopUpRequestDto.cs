using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class TopUpRequestDto
    {
        [Range(0, int.MaxValue)]
        public int Id { get; set; }

        [Range(1, int.MaxValue)]
        public int EmployeeId { get; set; }

        [Range(1, int.MaxValue)]
        public int ManagerId { get; set; }

        [Range(1, int.MaxValue)]
        public int ProjectId { get; set; }

        public DateTimeOffset TopUpStart { get; set; }

        public DateTimeOffset TopUpEnd { get; set; }
    }
}