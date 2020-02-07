using System;
using System.Collections.Generic;
using System.Text;

namespace D2M.Data.Model
{
    public class Thread
    {
        public Guid Id { get; set; }
        public ulong AssignedDiscordChannelId { get; set; }
        public ulong OpenedByDiscordUserId { get; set; }
        public DateTime OpenedDateTime { get; set; }

        public ulong ClosedByDiscordUserId { get; set; }
        public DateTime? ClosedDateTime { get; set; }

        public ICollection<Message> Messages { get; set; } = null!;
    }
}
