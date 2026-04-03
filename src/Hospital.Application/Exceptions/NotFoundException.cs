namespace Hospital.Application.Exceptions;

public abstract class NotFoundException : ApplicationExceptionBase
{
    protected NotFoundException(string message) : base(message)
    {
    }

    public override int StatusCode => 404;
    public override string ErrorType => "NotFound";
}
