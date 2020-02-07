using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace D2M.Bot.Services
{
    public interface IDiscordGuildService
    {
        Task CreateChannel(string channelName, string topic, ulong categoryId);
    }

    public class DiscordGuildService : IDiscordGuildService
    {
        private readonly DiscordSocketClient _discordSocketClient;
        private SocketGuild? _guild;

        public DiscordGuildService(DiscordSocketClient discordSocketClient)
        {
            _discordSocketClient = discordSocketClient;
        }

        private SocketGuild GetGuild()
        {
            return _guild ??= _discordSocketClient.Guilds.First();
        }

        public Task CreateChannel(string channelName, string topic, ulong categoryId)
        {
            var guild = GetGuild();

            return guild.CreateTextChannelAsync(channelName, properties =>
            {
                properties.Topic = topic;
                properties.CategoryId = categoryId;
            });
        }
    }
}
