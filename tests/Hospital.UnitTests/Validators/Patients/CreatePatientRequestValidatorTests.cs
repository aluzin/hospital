using Hospital.Api.Contracts.Patients;
using Hospital.Api.Validators.Patients;

namespace Hospital.UnitTests.Validators.Patients;

public class CreatePatientRequestValidatorTests
{
    private readonly CreatePatientRequestValidator _validator = new();

    [Fact]
    public void Validate_ShouldReturnError_WhenBirthDateIsMissing()
    {
        var result = _validator.Validate(new CreatePatientRequest
        {
            Name = new PatientNameRequest
            {
                Family = "Ivanov",
                Given = new[] { "Ivan" }
            },
            BirthDate = default
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "BirthDate" && x.ErrorMessage == "BirthDate is required.");
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenGenderIsInvalid()
    {
        var result = _validator.Validate(new CreatePatientRequest
        {
            Name = new PatientNameRequest
            {
                Family = "Ivanov",
                Given = new[] { "Ivan" }
            },
            BirthDate = DateTimeOffset.UtcNow,
            Gender = "robot"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Gender" && x.ErrorMessage == "Gender must be one of: male, female, other, unknown.");
    }

    [Fact]
    public void Validate_ShouldPass_ForValidRequest()
    {
        var result = _validator.Validate(new CreatePatientRequest
        {
            Name = new PatientNameRequest
            {
                Family = "Ivanov",
                Given = new[] { "Ivan" }
            },
            BirthDate = DateTimeOffset.UtcNow,
            Gender = "male"
        });

        Assert.True(result.IsValid);
    }
}
