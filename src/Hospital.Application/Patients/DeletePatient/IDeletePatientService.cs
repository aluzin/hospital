namespace Hospital.Application.Patients.DeletePatient;

public interface IDeletePatientService
{
    Task ExecuteAsync(DeletePatientRequest request, CancellationToken cancellationToken = default);
}
