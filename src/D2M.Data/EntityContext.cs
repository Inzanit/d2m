using D2M.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace D2M.Data
{
    public class EntityContext : DbContext
    {
        public EntityContext(DbContextOptions<EntityContext> builderOptions) : base(builderOptions)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EntityContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Thread> Threads { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;
        public DbSet<BehaviourConfiguration> Configurations { get; set; } = null!;
    }
}
