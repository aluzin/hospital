namespace Hospital.Api.Contracts;

public class ErrorResponse
{
    public string Type { get; set; } = string.Empty;
    public long? Id { get; set; }
    public ErrorDataResponse Data { get; set; } = new();
}
