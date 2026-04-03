namespace Hospital.Infrastructure.Persistence;

public sealed class DatabaseOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public bool MigrateOnStartup { get; set; }
}
