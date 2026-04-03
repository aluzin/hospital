using Hospital.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Application.Patients.SearchPatients;

public class SearchPatientsService : ISearchPatientsService
{
    private readonly IHospitalDbContext _dbContext;

    public SearchPatientsService(IHospitalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SearchPatientsResult> ExecuteAsync(
        SearchPatientsRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Patients
            .AsNoTracking()
            .AsQueryable();

        foreach (var birthDateSearchGroup in request.BirthDateSearchGroups)
        {
            query = BirthDateSearchExpressionBuilder.Apply(query, birthDateSearchGroup);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.BirthDate)
            .ThenBy(x => x.Id)
            .Skip(request.Skip)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        return new SearchPatientsResult
        {
            Items = items.Select(x => x.ToModel()).ToArray(),
            TotalCount = totalCount,
            Skip = request.Skip,
            Take = request.Take
        };
    }
}
