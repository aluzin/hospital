using Hospital.Application.Patients.Models;
using Hospital.Domain.Entities;

namespace Hospital.Application.Patients;

internal static class PatientMappings
{
    public static PatientModel ToModel(this Patient patient)
    {
        return new PatientModel
        {
            Id = patient.Id,
            Name = new PatientNameModel
            {
                Id = patient.Name.Id,
                Use = patient.Name.Use,
                Family = patient.Name.Family,
                Given = patient.Name.Given
            },
            Gender = patient.Gender,
            BirthDate = patient.BirthDate,
            Active = patient.Active
        };
    }
}
