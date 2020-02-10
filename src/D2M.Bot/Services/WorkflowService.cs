using System;
using System.Threading.Tasks;
using D2M.Services;
using Discord;

namespace D2M.Bot.Services
{
    public interface IWorkflowService
    {
        Task<ulong> CreateChannelForThread(IUser forUser);
    }

    public class WorkflowService : IWorkflowService
    {
        private readonly IDiscordGuildService _discordGuildService;
        private readonly IPermissionService _permissionService;
        private readonly IBehaviourConfigurationService _behaviourConfigurationService;

        public WorkflowService(IDiscordGuildService discordGuildService, 
            IPermissionService permissionService, 
            IBehaviourConfigurationService behaviourConfigurationService)
        {
            _discordGuildService = discordGuildService;
            _permissionService = permissionService;
            _behaviourConfigurationService = behaviourConfigurationService;
        }

        public async Task<ulong> CreateChannelForThread(IUser forUser)
        {
            var category = _behaviourConfigurationService.GetCategoryId();

            if (category is null)
                throw new InvalidOperationException("Cannot create channel for thread without category ID");

            var newChannelId = await _discordGuildService.CreateChannel($"{forUser.Username}-{forUser.Discriminator}", $"Thread for {forUser.Id}", category.Value);
            await _permissionService.SynchronisePermissionsWithCategory(newChannelId);

            return newChannelId;
        }
    }
}
