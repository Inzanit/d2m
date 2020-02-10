using System;
using System.Collections.Generic;
using System.Text;

namespace D2M.Data.Model
{
    public class Thread
    {
        public Thread(ulong assignedDiscordChannelId, ulong openedByDiscordUserId, DateTime openedDateTime)
        {
            Id = Guid.NewGuid();
            AssignedDiscordChannelId = assignedDiscordChannelId;
            OpenedByDiscordUserId = openedByDiscordUserId;
            OpenedDateTime = openedDateTime;
        }

        public Guid Id { get; internal set; }
        public ulong AssignedDiscordChannelId { get; internal set; }
        public ulong OpenedByDiscordUserId { get; internal set; }
        public DateTime OpenedDateTime { get; internal set; }

        public ulong ClosedByDiscordUserId { get; internal set; }
        public DateTime? ClosedDateTime { get; internal set; }

        public ICollection<Message> Messages { get; internal set; } = null!;
    }
}
