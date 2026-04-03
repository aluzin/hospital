using Hospital.Application.Abstractions.Persistence;
using Hospital.Domain.Entities;
using Hospital.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Hospital.UnitTests.TestInfrastructure;

internal class TestHospitalDbContext : DbContext, IHospitalDbContext
{
    public TestHospitalDbContext(DbContextOptions<TestHospitalDbContext> options) : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var stringArrayConverter = new ValueConverter<string[], string>(
            value => string.Join("||", value),
            value => string.IsNullOrEmpty(value)
                ? Array.Empty<string>()
                : value.Split("||", StringSplitOptions.None));

        modelBuilder.Entity<Patient>(builder =>
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Gender)
                .HasConversion(
                    value => value.ToString().ToLowerInvariant(),
                    value => Enum.Parse<PatientGender>(value, true));

            builder.OwnsOne(x => x.Name, nameBuilder =>
            {
                nameBuilder.Property(x => x.Id).IsRequired();
                nameBuilder.Property(x => x.Family).IsRequired();
                nameBuilder.Property(x => x.Given)
                    .HasConversion(stringArrayConverter)
                    .IsRequired();
            });
        });
    }
}
