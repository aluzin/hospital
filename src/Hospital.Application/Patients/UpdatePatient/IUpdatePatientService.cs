using Hospital.Application.Patients.Models;

namespace Hospital.Application.Patients.UpdatePatient;

public interface IUpdatePatientService
{
    Task<PatientModel> ExecuteAsync(UpdatePatientRequest request, CancellationToken cancellationToken = default);
}
