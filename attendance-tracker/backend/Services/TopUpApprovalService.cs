using backend.DTOs;
using backend.Models;

namespace backend.Services
{
    public class TopUpApprovalService
    {
        public TopUpResponseDto ProcessRequest(
            decimal swipeHours,
            decimal requestedTopUpHours,
            ApprovalConfiguration config)
        {
            var response = new TopUpResponseDto
            {
                SwipeHours = swipeHours,
                RequestedTopUpHours = requestedTopUpHours,
                TotalHours = swipeHours + requestedTopUpHours,
                RequiredSwipeHours = config.RequiredSwipeHours,
                AllowedTopUpHours = config.AllowedTopUpHours,
                RequiredTotalHours = config.RequiredTotalHours
            };

            if (swipeHours < config.RequiredSwipeHours)
            {
                response.ApprovalStatus =
                    "PENDING_MANAGER_REVIEW";

                response.Reason =
                    "Required swipe hours not met.";

                return response;
            }

            if (requestedTopUpHours >
                config.AllowedTopUpHours)
            {
                response.ApprovalStatus =
                    "PENDING_MANAGER_REVIEW";

                response.Reason =
                    "Requested top-up exceeds limit.";

                return response;
            }

            if (response.TotalHours <
                config.RequiredTotalHours)
            {
                response.ApprovalStatus =
                    "PENDING_MANAGER_REVIEW";

                response.Reason =
                    "Total hours requirement not met.";

                return response;
            }

            response.ApprovalStatus =
                "AUTO_APPROVED";

            response.Reason =
                "All rules satisfied.";

            return response;
        }
    }
}