using Hospital.Domain.Entities;
using Hospital.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hospital.Infrastructure.Persistence.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("patients");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.Gender)
            .HasColumnName("gender")
            .HasConversion(
                value => value.ToString().ToLowerInvariant(),
                value => Enum.Parse<PatientGender>(value, true))
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(x => x.BirthDate)
            .HasColumnName("birth_date")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.Active)
            .HasColumnName("active")
            .IsRequired();

        builder.OwnsOne(x => x.Name, nameBuilder =>
        {
            nameBuilder.Property(x => x.Id)
                .HasColumnName("name_id")
                .IsRequired();

            nameBuilder.Property(x => x.Use)
                .HasColumnName("name_use")
                .HasMaxLength(32);

            nameBuilder.Property(x => x.Family)
                .HasColumnName("name_family")
                .HasMaxLength(256)
                .IsRequired();

            nameBuilder.Property(x => x.Given)
                .HasColumnName("name_given")
                .HasColumnType("text[]")
                .IsRequired();
        });
    }
}
