using Hospital.Application.Patients.Models;

namespace Hospital.Application.Patients.CreatePatient;

public interface ICreatePatientService
{
    Task<PatientModel> ExecuteAsync(CreatePatientRequest request, CancellationToken cancellationToken = default);
}
