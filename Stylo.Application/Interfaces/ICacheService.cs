namespace Stylo.Backend.Stylo.Application.Interfaces
{
    public interface ICacheService
    {
        Task SetStringAsync(string key, string value, TimeSpan expiration, CancellationToken cancellationToken = default);
        Task<string?> GetStringAsync(string key, CancellationToken cancellationToken = default);
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    }
}