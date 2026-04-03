using Hospital.Api.Contracts.Patients;
using Hospital.Api.Validators.Patients;

namespace Hospital.UnitTests.Validators.Patients;

public class PatientNameRequestValidatorTests
{
    private readonly PatientNameRequestValidator _validator = new();

    [Fact]
    public void Validate_ShouldReturnError_WhenFamilyIsEmpty()
    {
        var result = _validator.Validate(new PatientNameRequest
        {
            Family = string.Empty,
            Given = new[] { "Ivan" }
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Family" && x.ErrorMessage == "Family is required.");
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenGivenContainsEmptyValue()
    {
        var result = _validator.Validate(new PatientNameRequest
        {
            Family = "Ivanov",
            Given = new[] { "Ivan", string.Empty }
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Given[1]" && x.ErrorMessage == "Given names cannot contain empty values.");
    }

    [Fact]
    public void Validate_ShouldPass_ForValidRequest()
    {
        var result = _validator.Validate(new PatientNameRequest
        {
            Family = "Ivanov",
            Given = new[] { "Ivan", "Ivanovich" }
        });

        Assert.True(result.IsValid);
    }
}
