using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace D2M.Data
{
    public class EntityContextDesignTimeFactory : IDesignTimeDbContextFactory<EntityContext>
    {
        EntityContext IDesignTimeDbContextFactory<EntityContext>.CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets("0DCAFE0B-8DA3-47FE-ACBC-AE8D4C819DFD")
                .Build();

            var builder = new DbContextOptionsBuilder<EntityContext>();

            var connectionString = configuration.GetConnectionString("Default");

            builder.UseSqlite(connectionString);

            return new EntityContext(builder.Options);
        }
    }

}
