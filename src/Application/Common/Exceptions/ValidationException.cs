using ShipSharp.Application.Common.Models;

namespace ShipSharp.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public List<ApiErrorDetail> Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = new List<ApiErrorDetail>();
    }

    public ValidationException(IEnumerable<ApiErrorDetail> errors)
        : base("One or more validation failures have occurred.")
    {
        Errors = errors.ToList();
    }
}
