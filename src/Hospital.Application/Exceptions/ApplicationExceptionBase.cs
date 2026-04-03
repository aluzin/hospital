namespace Hospital.Application.Exceptions;

public abstract class ApplicationExceptionBase : Exception
{
    protected ApplicationExceptionBase(string message) : base(message)
    {
    }

    public abstract int StatusCode { get; }
    public abstract string ErrorType { get; }
}
