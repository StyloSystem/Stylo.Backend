using Stylo.Backend.Stylo.Application.DTOs;

namespace Stylo.Backend.Stylo.Application.Interfaces
{
    public interface IAdminDashboardService
    {
        Task<DashboardStatsDto> GetStatsAsync();
        Task<IEnumerable<RecentOrderDto>> GetRecentOrdersAsync();
    }
}