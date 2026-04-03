namespace Hospital.Api.Contracts;

public class ErrorDataResponse
{
    public string Message { get; set; } = string.Empty;
    public IReadOnlyCollection<ValidationErrorItemResponse>? Errors { get; set; }
}
