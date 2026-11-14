using backend.Data;
using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TopUpController : ControllerBase
    {
        private readonly TopUpApprovalService _service;
        private readonly ApplicationDbContext _context;

        public TopUpController(
            TopUpApprovalService service,
            ApplicationDbContext context)
        {
            _service = service;
            _context = context;
        }

        [HttpPost("approve")]
        public IActionResult Approve(
            TopUpRequestDto request)
        {
            var config =
                _context.ApprovalConfigurations
                .FirstOrDefault();

            if (config == null)
            {
                config = new Models.ApprovalConfiguration
                {
                    RequiredSwipeHours = 5,
                    AllowedTopUpHours = 5,
                    RequiredTotalHours = 10
                };
            }

            var result =
                _service.ProcessRequest(
                    request.SwipeHours,
                    request.RequestedTopUpHours,
                    config);

            return Ok(result);
        }
    }
}