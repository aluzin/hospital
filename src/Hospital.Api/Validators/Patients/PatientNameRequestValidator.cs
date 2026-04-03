using FluentValidation;
using Hospital.Api.Contracts.Patients;

namespace Hospital.Api.Validators.Patients;

public class PatientNameRequestValidator : AbstractValidator<PatientNameRequest>
{
    public PatientNameRequestValidator()
    {
        RuleFor(x => x.Family)
            .NotEmpty()
            .WithMessage("Family is required.");

        RuleForEach(x => x.Given)
            .NotEmpty()
            .WithMessage("Given names cannot contain empty values.");
    }
}
