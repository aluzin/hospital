namespace Hospital.Application.Patients.SearchPatients;

public interface ISearchPatientsService
{
    Task<SearchPatientsResult> ExecuteAsync(SearchPatientsRequest request, CancellationToken cancellationToken = default);
}
