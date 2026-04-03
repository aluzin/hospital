namespace Hospital.Application.Patients.SearchPatients;

public class BirthDateSearchValue
{
    public BirthDateSearchPrefix Prefix { get; set; }
    public DateTimeOffset LowerBound { get; set; }
    public DateTimeOffset UpperBound { get; set; }
    public string RawValue { get; set; } = string.Empty;
}
