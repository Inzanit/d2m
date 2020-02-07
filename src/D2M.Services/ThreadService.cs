using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D2M.Data;
using Microsoft.EntityFrameworkCore;

namespace D2M.Services
{
    public interface IThreadService
    {
        Task<bool> HasOpenThread(ulong discordUserId);
    }

    public class ThreadService : IThreadService
    {
        private readonly EntityContext _db;

        public ThreadService(EntityContext db)
        {
            _db = db;
        }

        public async Task<bool> HasOpenThread(ulong discordUserId)
        {
            return await _db.Threads
                .Where(x => x.OpenedByDiscordUserId == discordUserId)
                .Where(x => x.ClosedDateTime == null)
                .AnyAsync();
        }
    }
}
