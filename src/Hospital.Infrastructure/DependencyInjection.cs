using Hospital.Application.Abstractions.Persistence;
using Hospital.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hospital.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("HospitalDatabase");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Database connection string is not configured.");
        }

        var migrateOnStartup = bool.TryParse(configuration["Database:MigrateOnStartup"], out var migrate) && migrate;

        services.Configure<DatabaseOptions>(options =>
        {
            options.ConnectionString = connectionString;
            options.MigrateOnStartup = migrateOnStartup;
        });

        services.AddDbContext<HospitalDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IHospitalDbContext>(provider => provider.GetRequiredService<HospitalDbContext>());

        return services;
    }
}
