namespace Hospital.Api.Contracts.Patients;

public class PatientNameRequest
{
    public Guid? Id { get; set; }
    public string? Use { get; set; }
    public string Family { get; set; } = string.Empty;
    public string[] Given { get; set; } = Array.Empty<string>();
}
