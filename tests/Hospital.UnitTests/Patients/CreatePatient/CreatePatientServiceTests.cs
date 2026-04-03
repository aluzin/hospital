using Hospital.Application.Patients.CreatePatient;
using Hospital.Domain.Enums;
using Hospital.UnitTests.TestInfrastructure;

namespace Hospital.UnitTests.Patients.CreatePatient;

public class CreatePatientServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldCreatePatientAndPersistIt()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var service = new CreatePatientService(dbContext);
        var request = new Application.Patients.CreatePatient.CreatePatientRequest
        {
            NameId = Guid.NewGuid(),
            NameUse = "official",
            NameFamily = "Ivanov",
            NameGiven = new[] { "Ivan", "Ivanovich" },
            Gender = PatientGender.Male,
            BirthDate = new DateTimeOffset(2024, 1, 13, 18, 25, 43, TimeSpan.Zero),
            Active = true
        };

        var result = await service.ExecuteAsync(request);

        var persistedPatient = dbContext.Patients.Single();
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(request.NameId, result.Name.Id);
        Assert.Equal(request.NameFamily, result.Name.Family);
        Assert.Equal(request.NameGiven, result.Name.Given);
        Assert.Equal(request.BirthDate, result.BirthDate);
        Assert.Equal(result.Id, persistedPatient.Id);
        Assert.Equal(request.NameFamily, persistedPatient.Name.Family);
    }
}
