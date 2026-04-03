using Hospital.Application.Abstractions.Persistence;
using Hospital.Application.Patients.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Application.Patients.DeletePatient;

public class DeletePatientService : IDeletePatientService
{
    private readonly IHospitalDbContext _dbContext;

    public DeletePatientService(IHospitalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ExecuteAsync(DeletePatientRequest request, CancellationToken cancellationToken = default)
    {
        var patient = await _dbContext.Patients
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (patient is null)
        {
            throw new PatientNotFoundException(request.Id);
        }

        _dbContext.Patients.Remove(patient);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
