using Hospital.Application.Patients.Exceptions;
using Hospital.Application.Patients.GetPatientById;
using Hospital.UnitTests.TestInfrastructure;

namespace Hospital.UnitTests.Patients.GetPatientById;

public class GetPatientByIdServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnPatient_WhenPatientExists()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var patient = PatientTestData.CreatePatient();
        dbContext.Patients.Add(patient);
        await dbContext.SaveChangesAsync();

        var service = new GetPatientByIdService(dbContext);

        var result = await service.ExecuteAsync(new Application.Patients.GetPatientById.GetPatientByIdRequest
        {
            Id = patient.Id
        });

        Assert.Equal(patient.Id, result.Id);
        Assert.Equal(patient.Name.Family, result.Name.Family);
        Assert.Equal(patient.BirthDate, result.BirthDate);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrow_WhenPatientDoesNotExist()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var service = new GetPatientByIdService(dbContext);
        var patientId = Guid.NewGuid();

        var exception = await Assert.ThrowsAsync<PatientNotFoundException>(() => service.ExecuteAsync(
            new Application.Patients.GetPatientById.GetPatientByIdRequest
            {
                Id = patientId
            }));

        Assert.Equal("PatientNotFound", exception.ErrorType);
        Assert.Equal(404, exception.StatusCode);
        Assert.Contains(patientId.ToString(), exception.Message);
    }
}
