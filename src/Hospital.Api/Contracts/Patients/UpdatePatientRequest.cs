namespace Hospital.Api.Contracts.Patients;

public class UpdatePatientRequest
{
    public PatientNameRequest Name { get; set; } = new();
    public string? Gender { get; set; }
    public DateTimeOffset BirthDate { get; set; }
    public bool? Active { get; set; }
}
