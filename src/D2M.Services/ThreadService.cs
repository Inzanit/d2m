using System;
using System.Linq;
using System.Threading.Tasks;
using D2M.Data;
using D2M.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace D2M.Services
{
    public interface IThreadService
    {
        Task<bool> HasOpenThread(ulong discordUserId);
        Task<ulong> GetOpenThreadChannelId(ulong discordUserId);
        Task StartThread(ulong discordUserId, ulong discordChannelId);
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

        public async Task<ulong> GetOpenThreadChannelId(ulong discordUserId)
        {
            return await _db.Threads
                .Where(x => x.OpenedByDiscordUserId == discordUserId)
                .Where(x => x.ClosedDateTime == null)
                .Select(x => x.AssignedDiscordChannelId)
                .SingleAsync();
        }

        public async Task StartThread(ulong discordUserId, ulong discordChannelId)
        {
            var thread = new Thread(discordChannelId, discordUserId, DateTime.UtcNow);

            _db.Threads.Add(thread);

            await _db.SaveChangesAsync();
        }
    }
}
