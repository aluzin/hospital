namespace Hospital.Api.Contracts.Patients;

public class PatientResponse
{
    public Guid Id { get; set; }
    public PatientNameResponse Name { get; set; } = new();
    public string Gender { get; set; } = string.Empty;
    public DateTimeOffset BirthDate { get; set; }
    public bool Active { get; set; }
}
