namespace Stylo.Backend.Stylo.Application.Exceptions
{
    public class RateLimitExceededException : AppException
    {
        public RateLimitExceededException(string message)
            : base(message, 429, "RATE_LIMIT_EXCEEDED")
        {
        }
    }
}