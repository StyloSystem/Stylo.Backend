﻿using Stylo.Backend.Stylo.Domain.Entities;

namespace Stylo.Backend.Stylo.Application.Interfaces
{
    public interface IAdminDashboardRepository
    {
        Task<int> GetTotalOrdersAsync();
        Task<decimal> GetTotalRevenueAsync();
        Task<int> GetTotalProductsAsync();
        Task<int> GetTotalCategoriesAsync();
        Task<int> GetTotalCustomersAsync();
        Task<IEnumerable<Order>> GetRecentOrdersAsync(int count);
    }
}