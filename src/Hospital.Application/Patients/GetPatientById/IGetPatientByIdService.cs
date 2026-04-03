using Hospital.Application.Patients.Models;

namespace Hospital.Application.Patients.GetPatientById;

public interface IGetPatientByIdService
{
    Task<PatientModel> ExecuteAsync(GetPatientByIdRequest request, CancellationToken cancellationToken = default);
}
