using FluentValidation;
using Hospital.Api.Contracts.Patients;
using Hospital.Application.Patients.SearchPatients;
using SearchPatientsApiRequest = Hospital.Api.Contracts.Patients.SearchPatientsRequest;

namespace Hospital.Api.Validators.Patients;

public class SearchPatientsRequestValidator : AbstractValidator<SearchPatientsApiRequest>
{
    public SearchPatientsRequestValidator()
    {
        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Take)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);

        RuleForEach(x => x.BirthDate)
            .Must(BeValidBirthDate)
            .WithMessage("BirthDate must be a valid FHIR date search value.");
    }

    private static bool BeValidBirthDate(string? value)
    {
        return BirthDateSearchGroupParser.TryParse(value, out _);
    }
}
