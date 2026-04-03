namespace Hospital.Api.Contracts;

public class ValidationErrorItemResponse
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
