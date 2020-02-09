using System.Threading.Tasks;
using Discord;

namespace D2M.Bot.Services
{
    public interface IPermissionService
    {
        Task RestrictCategoryToStaff(ulong categoryId, ulong staffRoleId);
        Task AddOverrideForLogChannel(ulong channelId, ulong staffRoleId);
        Task SynchronisePermissionsWithCategory(ulong channelId);
    }

    public class PermissionService : IPermissionService
    {
        private readonly IDiscordGuildService _discordGuildService;

        public PermissionService(IDiscordGuildService discordGuildService)
        {
            _discordGuildService = discordGuildService;
        }

        public async Task RestrictCategoryToStaff(ulong categoryId, ulong staffRoleId)
        {
            var category = _discordGuildService.GetCategory(categoryId);
            var everyoneRole = _discordGuildService.GetEveryoneRole();
            var staffRole = _discordGuildService.GetRole(staffRoleId);

            await category.AddPermissionOverwriteAsync(everyoneRole, OverwritePermissions.DenyAll(category));

            await category.AddPermissionOverwriteAsync(staffRole,
                new OverwritePermissions(addReactions: PermValue.Allow, 
                    viewChannel: PermValue.Allow,
                    sendMessages: PermValue.Allow, 
                    embedLinks: PermValue.Allow, 
                    manageMessages: PermValue.Allow,
                    attachFiles: PermValue.Allow, 
                    readMessageHistory: PermValue.Allow,
                    useExternalEmojis: PermValue.Allow));
        }

        public async Task AddOverrideForLogChannel(ulong channelId, ulong staffRoleId)
        {
            var category = _discordGuildService.GetChannel(channelId);
            var staffRole = _discordGuildService.GetRole(staffRoleId);

            await category.AddPermissionOverwriteAsync(staffRole,
                new OverwritePermissions(addReactions: PermValue.Deny, 
                    viewChannel: PermValue.Allow,
                    sendMessages: PermValue.Allow, 
                    embedLinks: PermValue.Allow, 
                    manageMessages: PermValue.Deny,
                    attachFiles: PermValue.Allow, 
                    readMessageHistory: PermValue.Allow,
                    useExternalEmojis: PermValue.Allow));
        }

        public async Task SynchronisePermissionsWithCategory(ulong channelId)
        {
            var channel = _discordGuildService.GetChannel(channelId);
            await channel.SyncPermissionsAsync();
        }
    }
}
