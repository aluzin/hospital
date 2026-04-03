using Hospital.Domain.Entities;
using Hospital.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Hospital.IntegrationTests.TestInfrastructure;

public sealed class HospitalApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _databaseContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("hospital")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _databaseContainer.StartAsync();

        Client = CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

        await ResetDatabaseAsync();
    }

    public new async Task DisposeAsync()
    {
        Client.Dispose();
        await _databaseContainer.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HospitalDbContext>();

        dbContext.Patients.RemoveRange(await dbContext.Patients.ToListAsync());
        await dbContext.SaveChangesAsync();
    }

    public async Task SeedPatientsAsync(params Patient[] patients)
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HospitalDbContext>();

        dbContext.Patients.AddRange(patients);
        await dbContext.SaveChangesAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:HospitalDatabase"] = _databaseContainer.GetConnectionString(),
                ["Database:MigrateOnStartup"] = "true"
            });
        });
    }
}
