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
                        .FirstOrDefault();

            if (existing == null)
            {
                existing = new ApprovalConfiguration();

                _context.ApprovalConfigurations
                    .Add(existing);
            }

            existing.RequiredSwipeHours =
                dto.RequiredSwipeHours;

            existing.AllowedTopUpHours =
                dto.AllowedTopUpHours;

            existing.RequiredTotalHours =
                dto.RequiredTotalHours;

            existing.IsActive = true;

            _context.SaveChanges();

            return Ok(existing);
        }

        [HttpGet]
        public IActionResult GetConfiguration()
        {
            var config =
                _context.ApprovalConfigurations
                        .FirstOrDefault();

            return Ok(config);
        }
    }
}