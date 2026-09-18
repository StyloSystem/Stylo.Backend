using Stylo.Backend.Stylo.Application.DTOs;
using Stylo.Backend.Stylo.Application.Interfaces;

namespace Stylo.Backend.Stylo.Application.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private const int RecentOrdersCount = 4;

        private readonly IAdminDashboardRepository _repository;

        public AdminDashboardService(IAdminDashboardRepository repository)
        {
            _repository = repository;
        }

        public async Task<DashboardStatsDto> GetStatsAsync()
        {
            var totalOrders = await _repository.GetTotalOrdersAsync();
            var totalRevenue = await _repository.GetTotalRevenueAsync();
            var totalProducts = await _repository.GetTotalProductsAsync();
            var totalCategories = await _repository.GetTotalCategoriesAsync();
            var totalCustomers = await _repository.GetTotalCustomersAsync();

            return new DashboardStatsDto
            {
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                TotalProducts = totalProducts,
                TotalCategories = totalCategories,
                TotalCustomers = totalCustomers
            };
        }

        public async Task<IEnumerable<RecentOrderDto>> GetRecentOrdersAsync()
        {
            var orders = await _repository.GetRecentOrdersAsync(RecentOrdersCount);

            return orders.Select(o => new RecentOrderDto
            {
                OrderNumber = $"#ST-{o.Id}",
                CustomerName = o.User?.Name ?? o.RecipientName,
                TotalPrice = o.TotalPrice,
                Status = o.Status.ToString()
            });
        }
    }
}