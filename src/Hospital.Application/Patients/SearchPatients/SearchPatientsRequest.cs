namespace Hospital.Application.Patients.SearchPatients;

public class SearchPatientsRequest
{
    public IReadOnlyCollection<BirthDateSearchGroup> BirthDateSearchGroups { get; set; } = Array.Empty<BirthDateSearchGroup>();
    public int Skip { get; set; }
    public int Take { get; set; }
}
