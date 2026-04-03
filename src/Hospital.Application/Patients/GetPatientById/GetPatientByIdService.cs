using Hospital.Application.Abstractions.Persistence;
using Hospital.Application.Patients.Models;
using Hospital.Application.Patients.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Application.Patients.GetPatientById;

public class GetPatientByIdService : IGetPatientByIdService
{
    private readonly IHospitalDbContext _dbContext;

    public GetPatientByIdService(IHospitalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PatientModel> ExecuteAsync(GetPatientByIdRequest request, CancellationToken cancellationToken = default)
    {
        var patient = await _dbContext.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (patient is null)
        {
            throw new PatientNotFoundException(request.Id);
        }

        return patient.ToModel();
    }
}
