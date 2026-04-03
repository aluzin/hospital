using Hospital.Application.Patients.Models;

namespace Hospital.Application.Patients.SearchPatients;

public class SearchPatientsResult
{
    public IReadOnlyCollection<PatientModel> Items { get; set; } = Array.Empty<PatientModel>();
    public int TotalCount { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}
