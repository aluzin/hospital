using Hospital.Application.Patients.SearchPatients;

namespace Hospital.UnitTests.Patients.SearchPatients;

public class BirthDateSearchGroupParserTests
{
    [Fact]
    public void Parse_ShouldSplitCommaSeparatedValuesIntoGroup()
    {
        var result = BirthDateSearchGroupParser.Parse("lt2024-01-13,ge2024-01-14");

        Assert.Equal(2, result.Values.Count);
        Assert.Equal(BirthDateSearchPrefix.Lt, result.Values.ElementAt(0).Prefix);
        Assert.Equal(BirthDateSearchPrefix.Ge, result.Values.ElementAt(1).Prefix);
    }

    [Theory]
    [InlineData("lt2024-01-13,")]
    [InlineData(",ge2024-01-14")]
    [InlineData("lt2024-01-13,,ge2024-01-14")]
    [InlineData("lt2024-01-13,foo2024-01-14")]
    public void TryParse_ShouldReturnFalse_ForInvalidGroup(string value)
    {
        var result = BirthDateSearchGroupParser.TryParse(value, out var searchGroup);

        Assert.False(result);
        Assert.Null(searchGroup);
    }
}
