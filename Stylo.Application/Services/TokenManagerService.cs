using System.Collections.Concurrent;
using Stylo.Backend.Stylo.Application.Interfaces;

namespace Stylo.Backend.Stylo.Application.Services
{
    public class TokenManagerService : ITokenManagerService
    {
        private readonly ConcurrentDictionary<string, DateTime> _revokedTokens = new();

        public void InvalidateToken(string token, DateTime expiration)
        {
            if (string.IsNullOrWhiteSpace(token)) return;

            CleanUpExpiredTokens();
            _revokedTokens[token] = expiration;
        }

        public bool IsTokenInvalidated(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;

            if (_revokedTokens.TryGetValue(token, out var expiration))
            {
                if (expiration > DateTime.UtcNow)
                {
                    return true;
                }

                _revokedTokens.TryRemove(token, out _);
            }

            return false;
        }

        private void CleanUpExpiredTokens()
        {
            var now = DateTime.UtcNow;
            foreach (var key in _revokedTokens.Keys)
            {
                if (_revokedTokens.TryGetValue(key, out var exp) && exp <= now)
                {
                    _revokedTokens.TryRemove(key, out _);
                }
            }
        }
    }
}
