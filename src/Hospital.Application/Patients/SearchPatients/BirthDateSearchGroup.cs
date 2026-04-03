namespace Hospital.Application.Patients.SearchPatients;

public class BirthDateSearchGroup
{
    public IReadOnlyCollection<BirthDateSearchValue> Values { get; set; } = Array.Empty<BirthDateSearchValue>();
}
