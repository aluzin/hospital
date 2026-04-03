using System.Globalization;

namespace Hospital.Application.Patients.SearchPatients;

public static class BirthDateSearchParser
{
    private static readonly string[] Prefixes =
    {
        "eq",
        "ne",
        "gt",
        "lt",
        "ge",
        "le",
        "sa",
        "eb",
        "ap"
    };

    private static readonly string[] DateTimeOffsetFormats =
    {
        "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK",
        "yyyy-MM-dd'T'HH:mm:ssK",
        "yyyy-MM-dd'T'HH:mmK"
    };

    private static readonly string[] DateTimeFormats =
    {
        "yyyy-MM-dd'T'HH:mm:ss.FFFFFFF",
        "yyyy-MM-dd'T'HH:mm:ss",
        "yyyy-MM-dd'T'HH:mm"
    };

    public static BirthDateSearchValue Parse(string value)
    {
        if (!TryParse(value, out var result))
        {
            throw new FormatException("BirthDate must be a valid FHIR date search value.");
        }

        return result!;
    }

    public static bool TryParse(string? value, out BirthDateSearchValue? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmedValue = value.Trim();
        var prefix = BirthDateSearchPrefix.Eq;
        var rawDateValue = trimmedValue;

        foreach (var knownPrefix in Prefixes)
        {
            if (!trimmedValue.StartsWith(knownPrefix, StringComparison.Ordinal))
            {
                continue;
            }

            prefix = ParsePrefix(knownPrefix);
            rawDateValue = trimmedValue[knownPrefix.Length..];
            break;
        }

        if (string.IsNullOrWhiteSpace(rawDateValue))
        {
            return false;
        }

        if (!TryParseBounds(rawDateValue, out var lowerBound, out var upperBound))
        {
            return false;
        }

        result = new BirthDateSearchValue
        {
            Prefix = prefix,
            LowerBound = lowerBound,
            UpperBound = upperBound,
            RawValue = trimmedValue
        };

        return true;
    }

    private static bool TryParseBounds(
        string rawDateValue,
        out DateTimeOffset lowerBound,
        out DateTimeOffset upperBound)
    {
        lowerBound = default;
        upperBound = default;

        if (TryParseYear(rawDateValue, out lowerBound, out upperBound))
        {
            return true;
        }

        if (TryParseYearMonth(rawDateValue, out lowerBound, out upperBound))
        {
            return true;
        }

        if (TryParseDate(rawDateValue, out lowerBound, out upperBound))
        {
            return true;
        }

        return TryParseDateTime(rawDateValue, out lowerBound, out upperBound);
    }

    private static bool TryParseYear(
        string rawDateValue,
        out DateTimeOffset lowerBound,
        out DateTimeOffset upperBound)
    {
        lowerBound = default;
        upperBound = default;

        if (!DateTime.TryParseExact(
                rawDateValue,
                "yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedDate))
        {
            return false;
        }

        lowerBound = CreateUtcDate(parsedDate.Year, 1, 1);
        upperBound = lowerBound.AddYears(1);

        return true;
    }

    private static bool TryParseYearMonth(
        string rawDateValue,
        out DateTimeOffset lowerBound,
        out DateTimeOffset upperBound)
    {
        lowerBound = default;
        upperBound = default;

        if (!DateTime.TryParseExact(
                rawDateValue,
                "yyyy-MM",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedDate))
        {
            return false;
        }

        lowerBound = CreateUtcDate(parsedDate.Year, parsedDate.Month, 1);
        upperBound = lowerBound.AddMonths(1);

        return true;
    }

    private static bool TryParseDate(
        string rawDateValue,
        out DateTimeOffset lowerBound,
        out DateTimeOffset upperBound)
    {
        lowerBound = default;
        upperBound = default;

        if (!DateTime.TryParseExact(
                rawDateValue,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedDate))
        {
            return false;
        }

        lowerBound = CreateUtcDate(parsedDate.Year, parsedDate.Month, parsedDate.Day);
        upperBound = lowerBound.AddDays(1);

        return true;
    }

    private static bool TryParseDateTime(
        string rawDateValue,
        out DateTimeOffset lowerBound,
        out DateTimeOffset upperBound)
    {
        lowerBound = default;
        upperBound = default;

        if (DateTimeOffset.TryParseExact(
                rawDateValue,
                DateTimeOffsetFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var parsedOffsetDateTime))
        {
            lowerBound = parsedOffsetDateTime.ToUniversalTime();
            upperBound = lowerBound.Add(GetDateTimePrecision(rawDateValue));

            return true;
        }

        if (!DateTime.TryParseExact(
                rawDateValue,
                DateTimeFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedDateTime))
        {
            return false;
        }

        lowerBound = new DateTimeOffset(DateTime.SpecifyKind(parsedDateTime, DateTimeKind.Utc));
        upperBound = lowerBound.Add(GetDateTimePrecision(rawDateValue));

        return true;
    }

    private static TimeSpan GetDateTimePrecision(string rawDateValue)
    {
        var timeSeparatorIndex = rawDateValue.IndexOf('T');
        if (timeSeparatorIndex < 0)
        {
            throw new FormatException("DateTime precision cannot be resolved for a non-date-time value.");
        }

        var timePart = rawDateValue[(timeSeparatorIndex + 1)..];
        var normalizedTimePart = StripOffset(timePart);

        var dotIndex = normalizedTimePart.IndexOf('.');
        if (dotIndex >= 0)
        {
            var fractionPart = normalizedTimePart[(dotIndex + 1)..];
            var ticks = (long)Math.Pow(10, 7 - fractionPart.Length);
            return TimeSpan.FromTicks(ticks);
        }

        var colonCount = normalizedTimePart.Count(x => x == ':');
        return colonCount switch
        {
            1 => TimeSpan.FromMinutes(1),
            2 => TimeSpan.FromSeconds(1),
            _ => throw new FormatException("Unsupported date-time precision.")
        };
    }

    private static string StripOffset(string timePart)
    {
        var zIndex = timePart.IndexOf('Z');
        if (zIndex >= 0)
        {
            return timePart[..zIndex];
        }

        for (var i = 1; i < timePart.Length; i++)
        {
            if (timePart[i] is '+' or '-')
            {
                return timePart[..i];
            }
        }

        return timePart;
    }

    private static BirthDateSearchPrefix ParsePrefix(string value)
    {
        return value switch
        {
            "eq" => BirthDateSearchPrefix.Eq,
            "ne" => BirthDateSearchPrefix.Ne,
            "gt" => BirthDateSearchPrefix.Gt,
            "lt" => BirthDateSearchPrefix.Lt,
            "ge" => BirthDateSearchPrefix.Ge,
            "le" => BirthDateSearchPrefix.Le,
            "sa" => BirthDateSearchPrefix.Sa,
            "eb" => BirthDateSearchPrefix.Eb,
            "ap" => BirthDateSearchPrefix.Ap,
            _ => throw new FormatException("Unsupported birthDate search prefix.")
        };
    }

    private static DateTimeOffset CreateUtcDate(int year, int month, int day)
    {
        return new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.Zero);
    }
}
