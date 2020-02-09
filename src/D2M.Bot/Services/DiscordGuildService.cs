using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace D2M.Bot.Services
{
    public interface IDiscordGuildService
    {
        Task<ulong> CreateChannel(string channelName, string topic, ulong categoryId);
        Task<ulong> CreateCategory(string categoryName);
        bool HasCategory(ulong categoryId);
        bool HasCategory(string categoryName);
        ulong GetCategoryId(string categoryName);
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

        public async Task<ulong> CreateChannel(string channelName, string topic, ulong categoryId)
        {
            var guild = GetGuild();

            var channel = await guild.CreateTextChannelAsync(channelName, properties =>
            {
                properties.Topic = topic;
                properties.CategoryId = categoryId;
            });

            return channel.Id;
        }

        public async Task<ulong> CreateCategory(string categoryName)
        {
            var guild = GetGuild();

            var category = await guild.CreateCategoryChannelAsync(categoryName);

            return category.Id;
        }

        public bool HasCategory(ulong categoryId)
        {
            var guild = GetGuild();

            return guild.CategoryChannels.Any(x => x.Id == categoryId);
        }

        public bool HasCategory(string categoryName)
        {
            var guild = GetGuild();

            return guild.CategoryChannels.Any(x => x.Name == categoryName);
        }

        public ulong GetCategoryId(string categoryName)
        {
            var guild = GetGuild();

            return guild.CategoryChannels
                .Where(x => x.Name == categoryName)
                .Select(x => x.Id)
                .Single();
        }
    }
}
