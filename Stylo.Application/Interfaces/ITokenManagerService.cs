namespace Stylo.Backend.Stylo.Application.Interfaces
{
    public interface ITokenManagerService
    {
        void InvalidateToken(string token, DateTime expiration);
        bool IsTokenInvalidated(string token);
    }
}
