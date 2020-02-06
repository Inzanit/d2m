using D2M.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace D2M.Data
{
    public class EntityContext : DbContext
    {
        public DbSet<Thread> Threads { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;
    }
}
