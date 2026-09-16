namespace Stylo.Backend.Stylo.Application.Exceptions
{
    public abstract class AppException : Exception
    {
        public int StatusCode { get; }
        public string ErrorCode { get; }

        protected AppException(string message, int statusCode, string errorCode) : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }
    }

    public class BadRequestException : AppException
    {
        public BadRequestException(string message, string errorCode = "BAD_REQUEST")
            : base(message, 400, errorCode)
        {
        }
    }

    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message = "Invalid credentials.", string errorCode = "UNAUTHORIZED")
            : base(message, 401, errorCode)
        {
        }
    }

    public class ForbiddenException : AppException
    {
        public ForbiddenException(string message = "Access denied.", string errorCode = "FORBIDDEN")
            : base(message, 403, errorCode)
        {
        }
    }

    public class NotFoundException : AppException
    {
        public NotFoundException(string message = "Resource not found.", string errorCode = "NOT_FOUND")
            : base(message, 404, errorCode)
        {
        }
    }

    public class ConflictException : AppException
    {
        public ConflictException(string message, string errorCode = "CONFLICT")
            : base(message, 409, errorCode)
        {
        }
    }
}
