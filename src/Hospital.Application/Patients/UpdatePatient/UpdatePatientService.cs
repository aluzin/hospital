using Hospital.Application.Abstractions.Persistence;
using Hospital.Application.Patients.Models;
using Hospital.Application.Patients.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Application.Patients.UpdatePatient;

public class UpdatePatientService : IUpdatePatientService
{
    private readonly IHospitalDbContext _dbContext;

    public UpdatePatientService(IHospitalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PatientModel> ExecuteAsync(UpdatePatientRequest request, CancellationToken cancellationToken = default)
    {
        var patient = await _dbContext.Patients
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (patient is null)
        {
            throw new PatientNotFoundException(request.Id);
        }

        patient.Name.Id = request.NameId;
        patient.Name.Use = request.NameUse;
        patient.Name.Family = request.NameFamily;
        patient.Name.Given = request.NameGiven;
        patient.Gender = request.Gender;
        patient.BirthDate = request.BirthDate;
        patient.Active = request.Active;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return patient.ToModel();
    }
}
