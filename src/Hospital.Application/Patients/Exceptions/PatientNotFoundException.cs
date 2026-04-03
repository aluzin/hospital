using Hospital.Application.Exceptions;

namespace Hospital.Application.Patients.Exceptions;

public class PatientNotFoundException : NotFoundException
{
    public PatientNotFoundException(Guid patientId)
        : base($"Patient with id '{patientId}' was not found.")
    {
    }

    public override string ErrorType => "PatientNotFound";
}
