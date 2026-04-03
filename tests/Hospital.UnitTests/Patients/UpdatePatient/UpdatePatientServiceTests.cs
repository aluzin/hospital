using Hospital.Application.Patients.Exceptions;
using Hospital.Application.Patients.UpdatePatient;
using Hospital.Domain.Enums;
using Hospital.UnitTests.TestInfrastructure;

namespace Hospital.UnitTests.Patients.UpdatePatient;

public class UpdatePatientServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldUpdateExistingPatient()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var patient = PatientTestData.CreatePatient();
        dbContext.Patients.Add(patient);
        await dbContext.SaveChangesAsync();

        var service = new UpdatePatientService(dbContext);
        var request = new Application.Patients.UpdatePatient.UpdatePatientRequest
        {
            Id = patient.Id,
            NameId = Guid.NewGuid(),
            NameUse = "usual",
            NameFamily = "Petrov",
            NameGiven = new[] { "Petr", "Petrovich" },
            Gender = PatientGender.Other,
            BirthDate = new DateTimeOffset(2024, 2, 1, 10, 0, 0, TimeSpan.Zero),
            Active = false
        };

        var result = await service.ExecuteAsync(request);

        var persistedPatient = dbContext.Patients.Single();
        Assert.Equal(request.NameId, result.Name.Id);
        Assert.Equal(request.NameFamily, result.Name.Family);
        Assert.Equal(request.NameGiven, result.Name.Given);
        Assert.Equal(request.Gender, result.Gender);
        Assert.Equal(request.BirthDate, result.BirthDate);
        Assert.Equal(request.Active, result.Active);
        Assert.Equal(request.NameFamily, persistedPatient.Name.Family);
        Assert.Equal(request.Active, persistedPatient.Active);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrow_WhenPatientDoesNotExist()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var service = new UpdatePatientService(dbContext);

        await Assert.ThrowsAsync<PatientNotFoundException>(() => service.ExecuteAsync(
            new Application.Patients.UpdatePatient.UpdatePatientRequest
            {
                Id = Guid.NewGuid(),
                NameId = Guid.NewGuid(),
                NameFamily = "Petrov",
                NameGiven = new[] { "Petr" },
                BirthDate = new DateTimeOffset(2024, 2, 1, 10, 0, 0, TimeSpan.Zero)
            }));
    }
}
