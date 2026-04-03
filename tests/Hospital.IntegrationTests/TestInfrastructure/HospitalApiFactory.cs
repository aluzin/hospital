using Hospital.Domain.Entities;
using Hospital.Application.Abstractions.Persistence;
using Hospital.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
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
    private string _databaseConnectionString = string.Empty;

    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _databaseContainer.StartAsync();
        _databaseConnectionString = _databaseContainer.GetConnectionString();

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
                ["ConnectionStrings:HospitalDatabase"] = _databaseConnectionString,
                ["Database:MigrateOnStartup"] = "true"
            });
        });
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<HospitalDbContext>));
            services.RemoveAll(typeof(HospitalDbContext));
            services.RemoveAll(typeof(IHospitalDbContext));

            services.AddDbContext<HospitalDbContext>(options => options.UseNpgsql(_databaseConnectionString));
            services.AddScoped<IHospitalDbContext>(provider => provider.GetRequiredService<HospitalDbContext>());
            services.PostConfigure<DatabaseOptions>(options =>
            {
                options.ConnectionString = _databaseConnectionString;
                options.MigrateOnStartup = true;
            });
        });
    }
}
