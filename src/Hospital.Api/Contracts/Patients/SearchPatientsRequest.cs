namespace Hospital.Api.Contracts.Patients;

public class SearchPatientsRequest
{
    public string[] BirthDate { get; set; } = Array.Empty<string>();
    public int Skip { get; set; }
    public int Take { get; set; } = 20;
}
