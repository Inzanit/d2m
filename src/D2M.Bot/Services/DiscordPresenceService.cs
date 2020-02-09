using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace D2M.Bot.Services
{
    public interface IDiscordPresenceService
    {
        IDisposable TriggerTyping(IMessageChannel channel);
    }

    public class DiscordPresenceService : IDiscordPresenceService
    {
        public IDisposable TriggerTyping(IMessageChannel channel)
        {
            return channel.EnterTypingState();
        }
    }
}
