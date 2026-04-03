using System.Net.Http.Json;
using Hospital.Api.Contracts.Patients;

var cancellationTokenSource = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationTokenSource.Cancel();
};

var cancellationToken = cancellationTokenSource.Token;
var options = SeederOptions.FromEnvironment();

using var httpClient = new HttpClient
{
    BaseAddress = new Uri(options.ApiBaseUrl)
};

Console.WriteLine($"Seeding {options.PatientsCount} patients to {options.ApiBaseUrl}.");

await WaitUntilApiAvailableAsync(httpClient, options, cancellationToken);

var createdCount = 0;
foreach (var patient in PatientGenerator.Generate(options.PatientsCount))
{
    using var response = await httpClient.PostAsJsonAsync("/api/patients", patient, cancellationToken);
    if (!response.IsSuccessStatusCode)
    {
        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(
            $"Failed to create patient. StatusCode={(int)response.StatusCode}. Response={errorContent}");
    }

    createdCount++;
}

Console.WriteLine($"Successfully created {createdCount} patients.");

static async Task WaitUntilApiAvailableAsync(
    HttpClient httpClient,
    SeederOptions options,
    CancellationToken cancellationToken)
{
    for (var attempt = 1; attempt <= options.ReadinessAttempts; attempt++)
    {
        try
        {
            using var response = await httpClient.GetAsync("/swagger", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return;
            }
        }
        catch when (!cancellationToken.IsCancellationRequested)
        {
        }

        if (attempt == options.ReadinessAttempts)
        {
            throw new InvalidOperationException($"API {options.ApiBaseUrl} is not available.");
        }

        await Task.Delay(options.ReadinessDelay, cancellationToken);
    }
}

internal sealed class SeederOptions
{
    public string ApiBaseUrl { get; init; } = "http://localhost:8080";
    public int PatientsCount { get; init; } = 100;
    public int ReadinessAttempts { get; init; } = 30;
    public TimeSpan ReadinessDelay { get; init; } = TimeSpan.FromSeconds(2);

    public static SeederOptions FromEnvironment()
    {
        return new SeederOptions
        {
            ApiBaseUrl = GetString("API_BASE_URL", "http://localhost:8080"),
            PatientsCount = GetInt("SEEDER_PATIENTS_COUNT", 100),
            ReadinessAttempts = GetInt("SEEDER_READINESS_ATTEMPTS", 30),
            ReadinessDelay = TimeSpan.FromSeconds(GetInt("SEEDER_READINESS_DELAY_SECONDS", 2))
        };
    }

    private static string GetString(string name, string fallback)
    {
        return string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name))
            ? fallback
            : Environment.GetEnvironmentVariable(name)!;
    }

    private static int GetInt(string name, int fallback)
    {
        return int.TryParse(Environment.GetEnvironmentVariable(name), out var value)
            ? value
            : fallback;
    }
}

internal static class PatientGenerator
{
    private static readonly string[] MaleFamilies =
    {
        "Ivanov",
        "Petrov",
        "Sidorov",
        "Kuznetsov",
        "Smirnov"
    };

    private static readonly string[] FemaleFamilies =
    {
        "Ivanova",
        "Petrova",
        "Sidorova",
        "Kuznetsova",
        "Smirnova"
    };

    private static readonly string[] MaleGivenNames =
    {
        "Ivan",
        "Petr",
        "Nikolai",
        "Alexey",
        "Mikhail"
    };

    private static readonly string[] FemaleGivenNames =
    {
        "Anna",
        "Maria",
        "Elena",
        "Olga",
        "Natalia"
    };

    private static readonly string[] MalePatronymics =
    {
        "Ivanovich",
        "Petrovich",
        "Nikolaevich",
        "Alexeevich",
        "Mikhailovich"
    };

    private static readonly string[] FemalePatronymics =
    {
        "Ivanovna",
        "Petrovna",
        "Nikolaevna",
        "Alexeevna",
        "Mikhailovna"
    };

    private static readonly string[] NameUses =
    {
        "official",
        "usual"
    };

    public static IEnumerable<CreatePatientRequest> Generate(int count)
    {
        var random = new Random(173);

        for (var index = 0; index < count; index++)
        {
            var gender = random.Next(0, 2) == 0 ? "male" : "female";
            var family = gender == "male"
                ? MaleFamilies[random.Next(MaleFamilies.Length)]
                : FemaleFamilies[random.Next(FemaleFamilies.Length)];
            var given = gender == "male"
                ? MaleGivenNames[random.Next(MaleGivenNames.Length)]
                : FemaleGivenNames[random.Next(FemaleGivenNames.Length)];
            var patronymic = gender == "male"
                ? MalePatronymics[random.Next(MalePatronymics.Length)]
                : FemalePatronymics[random.Next(FemalePatronymics.Length)];

            yield return new CreatePatientRequest
            {
                Name = new PatientNameRequest
                {
                    Id = Guid.NewGuid(),
                    Use = NameUses[random.Next(NameUses.Length)],
                    Family = family,
                    Given = new[]
                    {
                        given,
                        patronymic
                    }
                },
                Gender = gender,
                BirthDate = GenerateBirthDate(random),
                Active = random.Next(0, 2) == 0
            };
        }
    }

    private static DateTimeOffset GenerateBirthDate(Random random)
    {
        var start = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2024, 3, 31, 23, 59, 59, TimeSpan.Zero);
        var ticks = (long)(random.NextDouble() * (end - start).Ticks);

        return start.AddTicks(ticks);
    }
}
