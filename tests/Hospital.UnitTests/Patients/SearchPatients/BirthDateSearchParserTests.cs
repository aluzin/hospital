using Hospital.Application.Patients.SearchPatients;

namespace Hospital.UnitTests.Patients.SearchPatients;

public class BirthDateSearchParserTests
{
    [Theory]
    [InlineData("2024", BirthDateSearchPrefix.Eq, "2024-01-01T00:00:00+00:00", "2025-01-01T00:00:00+00:00")]
    [InlineData("2024-01", BirthDateSearchPrefix.Eq, "2024-01-01T00:00:00+00:00", "2024-02-01T00:00:00+00:00")]
    [InlineData("2024-01-13", BirthDateSearchPrefix.Eq, "2024-01-13T00:00:00+00:00", "2024-01-14T00:00:00+00:00")]
    [InlineData("ge2024-01-13T18:25", BirthDateSearchPrefix.Ge, "2024-01-13T18:25:00+00:00", "2024-01-13T18:26:00+00:00")]
    [InlineData("lt2024-01-13T18:25:43", BirthDateSearchPrefix.Lt, "2024-01-13T18:25:43+00:00", "2024-01-13T18:25:44+00:00")]
    [InlineData("ap2024-01-13T18:25:43.123", BirthDateSearchPrefix.Ap, "2024-01-13T18:25:43.1230000+00:00", "2024-01-13T18:25:43.1240000+00:00")]
    [InlineData("eb2024-01-13T18:25:43+03:00", BirthDateSearchPrefix.Eb, "2024-01-13T15:25:43+00:00", "2024-01-13T15:25:44+00:00")]
    public void Parse_ShouldResolvePrefixAndBounds(
        string rawValue,
        BirthDateSearchPrefix expectedPrefix,
        string expectedLowerBound,
        string expectedUpperBound)
    {
        var result = BirthDateSearchParser.Parse(rawValue);

        Assert.Equal(expectedPrefix, result.Prefix);
        Assert.Equal(DateTimeOffset.Parse(expectedLowerBound), result.LowerBound);
        Assert.Equal(DateTimeOffset.Parse(expectedUpperBound), result.UpperBound);
        Assert.Equal(rawValue, result.RawValue);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ge")]
    [InlineData("2024-13")]
    [InlineData("foo2024-01-13")]
    [InlineData("2024-01-13T18")]
    public void TryParse_ShouldReturnFalse_ForInvalidValues(string? rawValue)
    {
        var result = BirthDateSearchParser.TryParse(rawValue, out var searchValue);

        Assert.False(result);
        Assert.Null(searchValue);
    }
}
