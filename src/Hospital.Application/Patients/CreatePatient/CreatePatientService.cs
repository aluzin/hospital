using Hospital.Application.Abstractions.Persistence;
using Hospital.Application.Patients.Models;
using Hospital.Domain.Entities;

namespace Hospital.Application.Patients.CreatePatient;

public class CreatePatientService : ICreatePatientService
{
    private readonly IHospitalDbContext _dbContext;

    public CreatePatientService(IHospitalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PatientModel> ExecuteAsync(CreatePatientRequest request, CancellationToken cancellationToken = default)
    {
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            Name = new PatientName
            {
                Id = request.NameId,
                Use = request.NameUse,
                Family = request.NameFamily,
                Given = request.NameGiven
            },
            Gender = request.Gender,
            BirthDate = request.BirthDate,
            Active = request.Active
        };

        _dbContext.Patients.Add(patient);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return patient.ToModel();
    }
}
