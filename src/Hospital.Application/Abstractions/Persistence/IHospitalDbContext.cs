using Hospital.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Application.Abstractions.Persistence;

public interface IHospitalDbContext
{
    DbSet<Patient> Patients { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
