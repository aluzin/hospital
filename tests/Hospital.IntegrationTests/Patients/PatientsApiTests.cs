using System.Net;
using System.Net.Http.Json;
using Hospital.Api.Contracts;
using Hospital.Api.Contracts.Patients;
using Hospital.Domain.Enums;
using Hospital.IntegrationTests.TestInfrastructure;

namespace Hospital.IntegrationTests.Patients;

public class PatientsApiTests : IClassFixture<HospitalApiFactory>, IAsyncLifetime
{
    private readonly HospitalApiFactory _factory;

    public PatientsApiTests(HospitalApiFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync()
    {
        return _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CrudEndpoints_ShouldSupportCreateReadUpdateAndDelete()
    {
        var createResponse = await _factory.Client.PostAsJsonAsync("/api/patients", new CreatePatientRequest
        {
            Name = new PatientNameRequest
            {
                Family = "Ivanov",
                Given = new[] { "Ivan", "Ivanovich" }
            },
            Gender = "male",
            BirthDate = new DateTimeOffset(2024, 1, 13, 18, 25, 43, TimeSpan.Zero),
            Active = true
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createdPatient = await createResponse.Content.ReadFromJsonAsync<PatientResponse>();
        Assert.NotNull(createdPatient);
        Assert.NotEqual(Guid.Empty, createdPatient.Id);
        Assert.Equal("Ivanov", createdPatient.Name.Family);

        var getResponse = await _factory.Client.GetAsync($"/api/patients/{createdPatient.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var patient = await getResponse.Content.ReadFromJsonAsync<PatientResponse>();
        Assert.NotNull(patient);
        Assert.Equal(createdPatient.Id, patient.Id);

        var updateResponse = await _factory.Client.PutAsJsonAsync($"/api/patients/{createdPatient.Id}", new UpdatePatientRequest
        {
            Name = new PatientNameRequest
            {
                Id = patient.Name.Id,
                Use = "usual",
                Family = "Petrov",
                Given = new[] { "Petr", "Petrovich" }
            },
            Gender = "other",
            BirthDate = new DateTimeOffset(2024, 2, 1, 10, 0, 0, TimeSpan.Zero),
            Active = false
        });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updatedPatient = await updateResponse.Content.ReadFromJsonAsync<PatientResponse>();
        Assert.NotNull(updatedPatient);
        Assert.Equal("Petrov", updatedPatient.Name.Family);
        Assert.Equal("other", updatedPatient.Gender);
        Assert.False(updatedPatient.Active);

        var deleteResponse = await _factory.Client.DeleteAsync($"/api/patients/{createdPatient.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var notFoundResponse = await _factory.Client.GetAsync($"/api/patients/{createdPatient.Id}");

        Assert.Equal(HttpStatusCode.NotFound, notFoundResponse.StatusCode);
        var error = await notFoundResponse.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("PatientNotFound", error.Type);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenPatientDoesNotExist()
    {
        var response = await _factory.Client.GetAsync($"/api/patients/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("PatientNotFound", error.Type);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenPatientDoesNotExist()
    {
        var response = await _factory.Client.PutAsJsonAsync($"/api/patients/{Guid.NewGuid()}", new UpdatePatientRequest
        {
            Name = new PatientNameRequest
            {
                Family = "Petrov",
                Given = new[] { "Petr" }
            },
            BirthDate = new DateTimeOffset(2024, 2, 1, 10, 0, 0, TimeSpan.Zero)
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("PatientNotFound", error.Type);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenPatientDoesNotExist()
    {
        var response = await _factory.Client.DeleteAsync($"/api/patients/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("PatientNotFound", error.Type);
    }

    [Fact]
    public async Task SearchEndpoint_ShouldSupportAndOrSemanticsAndPagination()
    {
        await _factory.SeedPatientsAsync(
            PatientTestData.CreatePatient(
                birthDate: new DateTimeOffset(2024, 1, 12, 23, 59, 59, TimeSpan.Zero),
                family: "Before"),
            PatientTestData.CreatePatient(
                birthDate: new DateTimeOffset(2024, 1, 13, 0, 0, 0, TimeSpan.Zero),
                family: "ExactStart"),
            PatientTestData.CreatePatient(
                birthDate: new DateTimeOffset(2024, 1, 13, 18, 25, 43, TimeSpan.Zero),
                family: "SameDay"),
            PatientTestData.CreatePatient(
                birthDate: new DateTimeOffset(2024, 1, 14, 0, 0, 0, TimeSpan.Zero),
                family: "NextDay"));

        var response = await _factory.Client.GetAsync(
            "/api/patients?birthDate=eq2024-01-12,eq2024-01-13&birthDate=eq2024-01-13,eq2024-01-14&skip=0&take=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<PatientResponse>>();
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal(0, result.Skip);
        Assert.Equal(1, result.Take);
        Assert.All(result.Items, x => Assert.Contains(x.Name.Family, new[] { "ExactStart", "SameDay" }));
    }

    [Fact]
    public async Task SearchEndpoint_ShouldReturnEmptyResult_WhenNoPatientsMatch()
    {
        await _factory.SeedPatientsAsync(
            PatientTestData.CreatePatient(
                birthDate: new DateTimeOffset(2024, 1, 13, 0, 0, 0, TimeSpan.Zero),
                family: "Existing"));

        var response = await _factory.Client.GetAsync("/api/patients?birthDate=eq2023-01-01");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<PatientResponse>>();
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task SearchEndpoint_ShouldSupportRepeatedBirthDateParametersAsAnd()
    {
        await _factory.SeedPatientsAsync(
            PatientTestData.CreatePatient(
                birthDate: new DateTimeOffset(2024, 1, 12, 23, 59, 59, TimeSpan.Zero),
                family: "Before"),
            PatientTestData.CreatePatient(
                birthDate: new DateTimeOffset(2024, 1, 13, 0, 0, 0, TimeSpan.Zero),
                family: "ExactStart"),
            PatientTestData.CreatePatient(
                birthDate: new DateTimeOffset(2024, 1, 13, 18, 25, 43, TimeSpan.Zero),
                family: "SameDay"),
            PatientTestData.CreatePatient(
                birthDate: new DateTimeOffset(2024, 1, 14, 0, 0, 0, TimeSpan.Zero),
                family: "NextDay"));

        var response = await _factory.Client.GetAsync("/api/patients?birthDate=ge2024-01-13&birthDate=lt2024-01-14");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<PatientResponse>>();
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, x => Assert.Contains(x.Name.Family, new[] { "ExactStart", "SameDay" }));
    }

    [Fact]
    public async Task SearchEndpoint_ShouldSupportCommaSeparatedBirthDateValuesAsOr()
    {
        await _factory.SeedPatientsAsync(
            PatientTestData.CreatePatient(
                birthDate: new DateTimeOffset(2024, 1, 12, 23, 59, 59, TimeSpan.Zero),
                family: "Before"),
            PatientTestData.CreatePatient(
                birthDate: new DateTimeOffset(2024, 1, 13, 18, 25, 43, TimeSpan.Zero),
                family: "SameDay"),
            PatientTestData.CreatePatient(
                birthDate: new DateTimeOffset(2024, 1, 14, 0, 0, 0, TimeSpan.Zero),
                family: "NextDay"));

        var response = await _factory.Client.GetAsync("/api/patients?birthDate=eq2024-01-12,eq2024-01-14");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<PatientResponse>>();
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Contains(result.Items, x => x.Name.Family == "Before");
        Assert.Contains(result.Items, x => x.Name.Family == "NextDay");
    }

    [Fact]
    public async Task SearchEndpoint_ShouldApplySkipAndTake()
    {
        await _factory.SeedPatientsAsync(
            PatientTestData.CreatePatient(family: "Alpha", birthDate: new DateTimeOffset(2024, 1, 13, 0, 0, 0, TimeSpan.Zero)),
            PatientTestData.CreatePatient(family: "Beta", birthDate: new DateTimeOffset(2024, 1, 13, 1, 0, 0, TimeSpan.Zero)),
            PatientTestData.CreatePatient(family: "Gamma", birthDate: new DateTimeOffset(2024, 1, 13, 2, 0, 0, TimeSpan.Zero)));

        var response = await _factory.Client.GetAsync("/api/patients?birthDate=eq2024-01-13&skip=1&take=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<PatientResponse>>();
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(1, result.Skip);
        Assert.Equal(1, result.Take);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task CreateEndpoint_ShouldReturnValidationError_ForInvalidRequest()
    {
        var response = await _factory.Client.PostAsJsonAsync("/api/patients", new CreatePatientRequest
        {
            Name = new PatientNameRequest
            {
                Family = string.Empty,
                Given = new[] { string.Empty }
            },
            Gender = "robot",
            BirthDate = default
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("ValidationError", error.Type);
        Assert.Equal("Validation failed.", error.Data.Message);
        Assert.NotNull(error.Data.Errors);
        Assert.Contains(error.Data.Errors, x => x.Field == "name.family");
        Assert.Contains(error.Data.Errors, x => x.Field == "name.given[0]");
        Assert.Contains(error.Data.Errors, x => x.Field == "gender");
        Assert.Contains(error.Data.Errors, x => x.Field == "birthDate");
    }

    [Fact]
    public async Task UpdateEndpoint_ShouldReturnValidationError_ForInvalidNestedName()
    {
        var response = await _factory.Client.PutAsJsonAsync($"/api/patients/{Guid.NewGuid()}", new UpdatePatientRequest
        {
            Name = new PatientNameRequest
            {
                Family = string.Empty,
                Given = new[] { string.Empty }
            },
            Gender = "female",
            BirthDate = DateTimeOffset.UtcNow
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("ValidationError", error.Type);
        Assert.NotNull(error.Data.Errors);
        Assert.Contains(error.Data.Errors, x => x.Field == "name.family");
        Assert.Contains(error.Data.Errors, x => x.Field == "name.given[0]");
    }

    [Fact]
    public async Task SearchEndpoint_ShouldReturnValidationError_ForInvalidBirthDate()
    {
        var response = await _factory.Client.GetAsync("/api/patients?birthDate=bad-value");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("ValidationError", error.Type);
        Assert.NotNull(error.Data.Errors);
        Assert.Contains(error.Data.Errors, x => x.Field == "birthDate[0]");
    }

    [Fact]
    public async Task SearchEndpoint_ShouldReturnValidationError_ForInvalidPagination()
    {
        var response = await _factory.Client.GetAsync("/api/patients?skip=-1&take=101");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("ValidationError", error.Type);
        Assert.NotNull(error.Data.Errors);
        Assert.Contains(error.Data.Errors, x => x.Field == "skip");
        Assert.Contains(error.Data.Errors, x => x.Field == "take");
    }
}
