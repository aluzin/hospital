using Hospital.Application.Patients.SearchPatients;
using Hospital.Domain.Entities;

namespace Hospital.UnitTests.Patients.SearchPatients;

public class BirthDateSearchExpressionBuilderTests
{
    [Fact]
    public void Apply_ShouldFilterByEqInterval()
    {
        var patients = CreatePatients().AsQueryable();
        var searchValue = BirthDateSearchParser.Parse("2024-01-13");

        var result = BirthDateSearchExpressionBuilder.Apply(patients, searchValue).ToArray();

        Assert.Equal(2, result.Length);
        Assert.All(result, x => Assert.Equal(new DateTimeOffset(2024, 1, 13, 0, 0, 0, TimeSpan.Zero).Date, x.BirthDate.Date));
    }

    [Fact]
    public void Apply_ShouldFilterByGtPrefix()
    {
        var patients = CreatePatients().AsQueryable();
        var searchValue = BirthDateSearchParser.Parse("gt2024-01-13");

        var result = BirthDateSearchExpressionBuilder.Apply(patients, searchValue).ToArray();

        Assert.Single(result);
        Assert.Equal(new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), result[0].Id);
    }

    [Fact]
    public void Apply_ShouldFilterByEbPrefix()
    {
        var patients = CreatePatients().AsQueryable();
        var searchValue = BirthDateSearchParser.Parse("eb2024-01-13");

        var result = BirthDateSearchExpressionBuilder.Apply(patients, searchValue).ToArray();

        Assert.Single(result);
        Assert.Equal(new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), result[0].Id);
    }

    [Fact]
    public void Apply_ShouldCombineMultipleConditionsWithAnd()
    {
        var patients = CreatePatients().AsQueryable();
        var lowerBound = BirthDateSearchParser.Parse("ge2024-01-13");
        var upperBound = BirthDateSearchParser.Parse("lt2024-01-14");

        var result = BirthDateSearchExpressionBuilder
            .Apply(patients, lowerBound);

        result = BirthDateSearchExpressionBuilder.Apply(result, upperBound);

        var items = result.ToArray();

        Assert.Equal(2, items.Length);
        Assert.DoesNotContain(items, x => x.Id == new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
        Assert.DoesNotContain(items, x => x.Id == new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"));
    }

    [Fact]
    public void Apply_ShouldUseProvidedNow_ForApproximateSearch()
    {
        var patients = CreatePatients().AsQueryable();
        var searchValue = BirthDateSearchParser.Parse("ap2024-01-13");
        var now = new DateTimeOffset(2024, 1, 23, 0, 0, 0, TimeSpan.Zero);

        var result = BirthDateSearchExpressionBuilder.Apply(patients, searchValue, now).ToArray();

        Assert.Equal(4, result.Length);
    }

    [Fact]
    public void Apply_ShouldCombineCommaSeparatedValuesWithOr()
    {
        var patients = CreatePatients().AsQueryable();
        var searchGroup = BirthDateSearchGroupParser.Parse("lt2024-01-13,ge2024-01-14");

        var result = BirthDateSearchExpressionBuilder.Apply(patients, searchGroup).ToArray();

        Assert.Equal(2, result.Length);
        Assert.Contains(result, x => x.Id == new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
        Assert.Contains(result, x => x.Id == new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"));
    }

    [Fact]
    public void Apply_ShouldCombineOrGroupsWithAndAcrossParameters()
    {
        var patients = CreatePatients().AsQueryable();
        var firstGroup = BirthDateSearchGroupParser.Parse("eq2024-01-12,eq2024-01-13");
        var secondGroup = BirthDateSearchGroupParser.Parse("eq2024-01-13,eq2024-01-14");

        var result = BirthDateSearchExpressionBuilder.Apply(patients, firstGroup);
        result = BirthDateSearchExpressionBuilder.Apply(result, secondGroup);

        var items = result.ToArray();

        Assert.Equal(2, items.Length);
        Assert.Contains(items, x => x.Id == new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));
        Assert.Contains(items, x => x.Id == new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"));
    }

    private static Patient[] CreatePatients()
    {
        return new[]
        {
            new Patient
            {
                Id = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                BirthDate = new DateTimeOffset(2024, 1, 12, 23, 59, 59, TimeSpan.Zero)
            },
            new Patient
            {
                Id = new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                BirthDate = new DateTimeOffset(2024, 1, 13, 0, 0, 0, TimeSpan.Zero)
            },
            new Patient
            {
                Id = new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                BirthDate = new DateTimeOffset(2024, 1, 13, 18, 25, 43, TimeSpan.Zero)
            },
            new Patient
            {
                Id = new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                BirthDate = new DateTimeOffset(2024, 1, 14, 0, 0, 0, TimeSpan.Zero)
            }
        };
    }
}
