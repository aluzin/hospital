using Microsoft.EntityFrameworkCore;

namespace Hospital.UnitTests.TestInfrastructure;

internal static class TestDbContextFactory
{
    public static TestHospitalDbContext Create()
    {
        var options = new DbContextOptionsBuilder<TestHospitalDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new TestHospitalDbContext(options);
    }
}
