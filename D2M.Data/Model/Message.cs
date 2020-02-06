using System;

namespace D2M.Data.Model
{
    public class Message
    {
        public ulong Id { get; set; }
        public string Content { get; set; } = null!;
        public ulong SentByDiscordUserId { get; set; }
        public DateTime SentDateTime { get; set; }

        public Guid ThreadId { get; set; }
        public virtual Thread Thread { get; set; } = null!;
    }
}