namespace ShipSharp.Application.Common.Exceptions;

public class UnprocessableEntityException : Exception
{
    public string Code { get; }

    public UnprocessableEntityException(string message, string code = "unprocessable_entity")
        : base(message)
    {
        Code = code;
    }
}
