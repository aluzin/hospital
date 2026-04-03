using Hospital.Domain.Enums;

namespace Hospital.Application.Patients.Models;

public class PatientModel
{
    public Guid Id { get; set; }
    public PatientNameModel Name { get; set; } = new();
    public PatientGender Gender { get; set; } = PatientGender.Unknown;
    public DateTimeOffset BirthDate { get; set; }
    public bool Active { get; set; }
}
