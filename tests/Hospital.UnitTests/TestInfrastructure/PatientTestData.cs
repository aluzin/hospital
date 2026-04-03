using Hospital.Domain.Entities;
using Hospital.Domain.Enums;

namespace Hospital.UnitTests.TestInfrastructure;

internal static class PatientTestData
{
    public static Patient CreatePatient(
        Guid? id = null,
        Guid? nameId = null,
        string family = "Ivanov",
        string[]? given = null,
        PatientGender gender = PatientGender.Male,
        DateTimeOffset? birthDate = null,
        bool active = true)
    {
        return new Patient
        {
            Id = id ?? Guid.NewGuid(),
            Name = new PatientName
            {
                Id = nameId ?? Guid.NewGuid(),
                Use = "official",
                Family = family,
                Given = given ?? new[] { "Ivan", "Ivanovich" }
            },
            Gender = gender,
            BirthDate = birthDate ?? new DateTimeOffset(2024, 1, 13, 18, 25, 43, TimeSpan.Zero),
            Active = active
        };
    }
}
