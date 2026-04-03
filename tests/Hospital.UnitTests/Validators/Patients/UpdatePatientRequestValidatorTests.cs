using Hospital.Api.Contracts.Patients;
using Hospital.Api.Validators.Patients;

namespace Hospital.UnitTests.Validators.Patients;

public class UpdatePatientRequestValidatorTests
{
    private readonly UpdatePatientRequestValidator _validator = new();

    [Fact]
    public void Validate_ShouldReturnNestedNameErrors_WhenNameIsInvalid()
    {
        var result = _validator.Validate(new UpdatePatientRequest
        {
            Name = new PatientNameRequest
            {
                Family = string.Empty,
                Given = new[] { string.Empty }
            },
            BirthDate = DateTimeOffset.UtcNow
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Name.Family");
        Assert.Contains(result.Errors, x => x.PropertyName == "Name.Given[0]");
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenGenderIsInvalid()
    {
        var result = _validator.Validate(new UpdatePatientRequest
        {
            Name = new PatientNameRequest
            {
                Family = "Ivanov",
                Given = new[] { "Ivan" }
            },
            BirthDate = DateTimeOffset.UtcNow,
            Gender = "invalid"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Gender");
    }

    [Fact]
    public void Validate_ShouldPass_ForValidRequest()
    {
        var result = _validator.Validate(new UpdatePatientRequest
        {
            Name = new PatientNameRequest
            {
                Family = "Ivanov",
                Given = new[] { "Ivan" }
            },
            BirthDate = DateTimeOffset.UtcNow,
            Gender = "female"
        });

        Assert.True(result.IsValid);
    }
}
