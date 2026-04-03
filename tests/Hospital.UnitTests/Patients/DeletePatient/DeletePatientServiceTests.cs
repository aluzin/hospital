using Hospital.Application.Patients.DeletePatient;
using Hospital.Application.Patients.Exceptions;
using Hospital.UnitTests.TestInfrastructure;

namespace Hospital.UnitTests.Patients.DeletePatient;

public class DeletePatientServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldRemovePatient()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var patient = PatientTestData.CreatePatient();
        dbContext.Patients.Add(patient);
        await dbContext.SaveChangesAsync();

        var service = new DeletePatientService(dbContext);

        await service.ExecuteAsync(new Application.Patients.DeletePatient.DeletePatientRequest
        {
            Id = patient.Id
        });

        Assert.Empty(dbContext.Patients);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrow_WhenPatientDoesNotExist()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var service = new DeletePatientService(dbContext);

        await Assert.ThrowsAsync<PatientNotFoundException>(() => service.ExecuteAsync(
            new Application.Patients.DeletePatient.DeletePatientRequest
            {
                Id = Guid.NewGuid()
            }));
    }
}
