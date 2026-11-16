using backend.DTOs;
using backend.Models;

namespace backend.Services
{
    public class TopUpApprovalService
    {
        public TopUpResponseDto ProcessRequest(
            decimal swipeHours,
            decimal dailyTopUpHours,
            int attendedDaysThisWeek,
            ApprovalConfiguration config)
        {
            var response = new TopUpResponseDto
            {
                SwipeHours = swipeHours,
                DailyTopUpHours = dailyTopUpHours,
                TotalHours = swipeHours + dailyTopUpHours,
                RequiredSwipeHours = config.RequiredSwipeHours,
                RequiredOfficeHours = config.RequiredOfficeHours,
                RequiredTotalHours = config.RequiredSwipeHours
                    + config.RequiredOfficeHours,
                RequiredOfficeDaysPerWeek =
                    config.RequiredOfficeDaysPerWeek,
                AttendedDaysThisWeek = attendedDaysThisWeek
            };

            if (swipeHours < config.RequiredSwipeHours)
            {
                response.ApprovalStatus =
                    "PENDING_MANAGER_REVIEW";

                response.Reason =
                    "Required swipe hours not met.";

                return response;
            }

            if (dailyTopUpHours < config.RequiredOfficeHours)
            {
                response.ApprovalStatus =
                    "PENDING_MANAGER_REVIEW";

                response.Reason =
                    "Required top-up hours not met.";

                return response;
            }

            if (response.TotalHours < response.RequiredTotalHours)
            {
                response.ApprovalStatus =
                    "PENDING_MANAGER_REVIEW";

                response.Reason =
                    "Required total hours not met.";

                return response;
            }

            if (attendedDaysThisWeek <
                config.RequiredOfficeDaysPerWeek)
            {
                response.ApprovalStatus =
                    "PENDING_MANAGER_REVIEW";

                response.Reason =
                    "Required office days this week not met.";

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