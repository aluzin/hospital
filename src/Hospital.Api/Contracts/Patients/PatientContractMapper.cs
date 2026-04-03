using Hospital.Application.Patients.Models;
using Hospital.Application.Patients.SearchPatients;
using Hospital.Domain.Enums;
using ApplicationCreatePatientRequest = Hospital.Application.Patients.CreatePatient.CreatePatientRequest;
using ApplicationSearchPatientsRequest = Hospital.Application.Patients.SearchPatients.SearchPatientsRequest;
using ApplicationUpdatePatientRequest = Hospital.Application.Patients.UpdatePatient.UpdatePatientRequest;

namespace Hospital.Api.Contracts.Patients;

public static class PatientContractMapper
{
    public static ApplicationCreatePatientRequest ToApplicationRequest(this CreatePatientRequest request)
    {
        return new ApplicationCreatePatientRequest
        {
            NameId = request.Name.Id ?? Guid.NewGuid(),
            NameUse = request.Name.Use,
            NameFamily = request.Name.Family,
            NameGiven = request.Name.Given,
            Gender = ParseGender(request.Gender),
            BirthDate = request.BirthDate,
            Active = request.Active ?? false
        };
    }

    public static ApplicationUpdatePatientRequest ToApplicationRequest(this UpdatePatientRequest request, Guid id)
    {
        return new ApplicationUpdatePatientRequest
        {
            Id = id,
            NameId = request.Name.Id ?? Guid.NewGuid(),
            NameUse = request.Name.Use,
            NameFamily = request.Name.Family,
            NameGiven = request.Name.Given,
            Gender = ParseGender(request.Gender),
            BirthDate = request.BirthDate,
            Active = request.Active ?? false
        };
    }

    public static ApplicationSearchPatientsRequest ToApplicationRequest(this SearchPatientsRequest request)
    {
        return new ApplicationSearchPatientsRequest
        {
            BirthDateSearchGroups = request.BirthDate
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(BirthDateSearchGroupParser.Parse)
                .ToArray(),
            Skip = request.Skip,
            Take = request.Take
        };
    }

    public static PatientResponse ToResponse(this PatientModel patient)
    {
        return new PatientResponse
        {
            Id = patient.Id,
            Name = new PatientNameResponse
            {
                Id = patient.Name.Id,
                Use = patient.Name.Use,
                Family = patient.Name.Family,
                Given = patient.Name.Given
            },
            Gender = ToApiValue(patient.Gender),
            BirthDate = patient.BirthDate,
            Active = patient.Active
        };
    }

    public static PagedResponse<PatientResponse> ToResponse(this SearchPatientsResult result)
    {
        return new PagedResponse<PatientResponse>
        {
            Items = result.Items.Select(ToResponse).ToArray(),
            TotalCount = result.TotalCount,
            Skip = result.Skip,
            Take = result.Take
        };
    }

    private static PatientGender ParseGender(string? value)
    {
        return value?.ToLowerInvariant() switch
        {
            "male" => PatientGender.Male,
            "female" => PatientGender.Female,
            "other" => PatientGender.Other,
            _ => PatientGender.Unknown
        };
    }

    private static string ToApiValue(PatientGender gender)
    {
        return gender switch
        {
            PatientGender.Male => "male",
            PatientGender.Female => "female",
            PatientGender.Other => "other",
            _ => "unknown"
        };
    }
}
