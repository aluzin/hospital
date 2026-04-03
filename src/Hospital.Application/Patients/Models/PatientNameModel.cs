namespace Hospital.Application.Patients.Models;

public class PatientNameModel
{
    public Guid Id { get; set; }
    public string? Use { get; set; }
    public string Family { get; set; } = string.Empty;
    public string[] Given { get; set; } = Array.Empty<string>();
}
