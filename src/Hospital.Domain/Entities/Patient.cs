using Hospital.Domain.Enums;

namespace Hospital.Domain.Entities;

public class Patient
{
    public Guid Id { get; set; }
    public PatientName Name { get; set; } = new();
    public PatientGender Gender { get; set; } = PatientGender.Unknown;
    public DateTimeOffset BirthDate { get; set; }
    public bool Active { get; set; }
}
