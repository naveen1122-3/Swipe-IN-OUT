using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class ApprovalConfigurationDto
    {
        [Range(1, int.MaxValue)]
        public int ManagerId { get; set; }

        [Range(1, int.MaxValue)]
        public int ProjectId { get; set; }

        [Range(0, 24)]
        public decimal RequiredSwipeHours { get; set; } = 5;

        [Range(0, 24)]
        public decimal RequiredOfficeHours { get; set; } = 5;

        [Range(0, 7)]
        public int RequiredOfficeDaysPerWeek { get; set; } = 5;
    }
}