using FluentValidation;
using Hospital.Api.Contracts.Patients;

namespace Hospital.Api.Validators.Patients;

public class CreatePatientRequestValidator : AbstractValidator<CreatePatientRequest>
{
    public CreatePatientRequestValidator()
    {
        RuleFor(x => x.Name)
            .SetValidator(new PatientNameRequestValidator());

        RuleFor(x => x.BirthDate)
            .NotEmpty()
            .WithMessage("BirthDate is required.");

        RuleFor(x => x.Gender)
            .Must(BeValidGender)
            .When(x => !string.IsNullOrWhiteSpace(x.Gender))
            .WithMessage("Gender must be one of: male, female, other, unknown.");
    }

    private static bool BeValidGender(string? value)
    {
        return value?.ToLowerInvariant() is "male" or "female" or "other" or "unknown";
    }
}
