using Hospital.Api.Contracts.Patients;
using Hospital.Api.Validators.Patients;

namespace Hospital.UnitTests.Validators.Patients;

public class SearchPatientsRequestValidatorTests
{
    private readonly SearchPatientsRequestValidator _validator = new();

    [Fact]
    public void Validate_ShouldReturnErrors_ForInvalidPagination()
    {
        var result = _validator.Validate(new SearchPatientsRequest
        {
            Skip = -1,
            Take = 101
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Skip");
        Assert.Contains(result.Errors, x => x.PropertyName == "Take");
    }

    [Fact]
    public void Validate_ShouldReturnError_ForInvalidBirthDateFilter()
    {
        var result = _validator.Validate(new SearchPatientsRequest
        {
            BirthDate = new[] { "bad-value" }
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "BirthDate[0]" && x.ErrorMessage == "BirthDate must be a valid FHIR date search value.");
    }

    [Fact]
    public void Validate_ShouldPass_ForValidRequest()
    {
        var result = _validator.Validate(new SearchPatientsRequest
        {
            BirthDate = new[] { "ge2024-01-01", "lt2024-02-01,eq2024-03" },
            Skip = 0,
            Take = 20
        });

        Assert.True(result.IsValid);
    }
}
