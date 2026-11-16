using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigurationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ConfigurationController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult SaveConfiguration(
            ApprovalConfigurationDto dto)
        {
            var existing =
                _context.ApprovalConfigurations
                        .FirstOrDefault(configuration =>
                            configuration.ManagerId == dto.ManagerId
                            && configuration.ProjectId == dto.ProjectId);

            if (existing == null)
            {
                existing = new ApprovalConfiguration
                {
                    ManagerId = dto.ManagerId,
                    ProjectId = dto.ProjectId
                };

                _context.ApprovalConfigurations
                    .Add(existing);
            }

            existing.RequiredSwipeHours =
                dto.RequiredSwipeHours;

            existing.RequiredOfficeHours =
                dto.RequiredOfficeHours;

            existing.RequiredTotalHours =
                dto.RequiredSwipeHours + dto.RequiredOfficeHours;

            existing.RequiredOfficeDaysPerWeek =
                dto.RequiredOfficeDaysPerWeek;

            existing.IsActive = true;

            _context.SaveChanges();

            return Ok(existing);
        }

    }
}