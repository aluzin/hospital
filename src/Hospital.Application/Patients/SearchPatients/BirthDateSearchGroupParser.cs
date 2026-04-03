namespace Hospital.Application.Patients.SearchPatients;

public static class BirthDateSearchGroupParser
{
    public static BirthDateSearchGroup Parse(string value)
    {
        if (!TryParse(value, out var result))
        {
            throw new FormatException("BirthDate must be a valid FHIR date search value.");
        }

        return result!;
    }

    public static bool TryParse(string? value, out BirthDateSearchGroup? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var parts = value.Split(',');
        if (parts.Length == 0)
        {
            return false;
        }

        var searchValues = new List<BirthDateSearchValue>(parts.Length);
        foreach (var part in parts)
        {
            var trimmedPart = part.Trim();
            if (string.IsNullOrWhiteSpace(trimmedPart))
            {
                return false;
            }

            if (!BirthDateSearchParser.TryParse(trimmedPart, out var searchValue))
            {
                return false;
            }

            searchValues.Add(searchValue!);
        }

        result = new BirthDateSearchGroup
        {
            Values = searchValues
        };

        return true;
    }
}
