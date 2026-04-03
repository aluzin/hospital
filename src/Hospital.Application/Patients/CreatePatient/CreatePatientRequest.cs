using Hospital.Domain.Enums;

namespace Hospital.Application.Patients.CreatePatient;

public class CreatePatientRequest
{
    public Guid NameId { get; set; }
    public string? NameUse { get; set; }
    public string NameFamily { get; set; } = string.Empty;
    public string[] NameGiven { get; set; } = Array.Empty<string>();
    public PatientGender Gender { get; set; } = PatientGender.Unknown;
    public DateTimeOffset BirthDate { get; set; }
    public bool Active { get; set; }
}
