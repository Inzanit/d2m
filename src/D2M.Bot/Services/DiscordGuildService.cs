using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace D2M.Bot.Services
{
    public interface IDiscordGuildService
    {
        Task<ulong> CreateChannel(string channelName, string topic, ulong categoryId, int? position = null);
        bool HasChannel(ulong channelId);
        bool HasChannel(string channelName);
        ulong GetChannelId(string channelName);
        Task<ulong> CreateCategory(string categoryName, int? position);
        bool HasCategory(ulong categoryId);
        bool HasCategory(string categoryName);
        ulong GetCategoryId(string categoryName);
        bool HasRole(ulong roleId);
        bool HasRole(string roleName);
        ulong GetRoleId(string roleName);
        ICategoryChannel GetCategory(ulong categoryId);
        IRole GetEveryoneRole();
        IRole GetRole(ulong roleId);
        ITextChannel GetChannel(ulong channelId);
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

        public async Task<ulong> CreateChannel(string channelName, string topic, ulong categoryId, int? position)
        {
            var guild = GetGuild();

            var channel = await guild.CreateTextChannelAsync(channelName, properties =>
            {
                properties.Topic = topic;
                properties.CategoryId = categoryId;
                properties.Position = position ?? 1;
            });

            return channel.Id;
        }

        public bool HasChannel(ulong channelId)
        {
            var guild = GetGuild();
            return guild.Channels.Any(x => x.Id == channelId);
        }

        public bool HasChannel(string channelName)
        {
            var guild = GetGuild();
            return guild.Channels.Any(x => x.Name == channelName);
        }

        public ulong GetChannelId(string channelName)
        {
            var guild = GetGuild();

            return guild.Channels
                .Where(x => x.Name == channelName)
                .Select(x => x.Id)
                .Single();
        }

        public async Task<ulong> CreateCategory(string categoryName, int? position = null)
        {
            var guild = GetGuild();

            var category = await guild.CreateCategoryChannelAsync(categoryName, prop => prop.Position = position ?? 1);

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

        public bool HasRole(ulong roleId)
        {
            var guild = GetGuild();

            return guild.Roles.Any(x => x.Id == roleId);
        }

        public bool HasRole(string roleName)
        {
            var guild = GetGuild();

            return guild.Roles.Any(x => x.Name == roleName);
        }

        public ulong GetRoleId(string roleName)
        {
            var guild = GetGuild();

            return guild
                .Roles
                .Where(x => x.Name == roleName)
                .Select(x => x.Id)
                .Single();
        }

        public ICategoryChannel GetCategory(ulong categoryId)
        {
            var guild = GetGuild();

            return guild.CategoryChannels
                .Single(x => x.Id == categoryId);
        }

        public IRole GetEveryoneRole()
        {
            var guild = GetGuild();

            return guild.EveryoneRole;
        }

        public IRole GetRole(ulong roleId)
        {
            var guild = GetGuild();

            return guild.GetRole(roleId);
        }

        public ITextChannel GetChannel(ulong channelId)
        {
            var guild = GetGuild();

            return guild.GetTextChannel(channelId);
        }
    }
}
