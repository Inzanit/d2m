using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using D2M.Services;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;

namespace D2M.Bot.Modules
{
    [RequireUserPermission(GuildPermission.Administrator)]
    public class SetUpModule : InteractiveBase
    {
        private readonly ICachedBehaviourConfiguration _cachedBehaviourConfiguration;

        public SetUpModule(ICachedBehaviourConfiguration cachedBehaviourConfiguration)
        {
            _cachedBehaviourConfiguration = cachedBehaviourConfiguration;
        }

        [Command("setup")]
        public async Task SetUp()
        {
            await InlineReactionReplyAsync(new ReactionCallbackData("D2M has already been set up in this guild, would you like to run the set up again?")
                .WithCallback(new Emoji("👍"), (context, reaction) => RunSetup())
                .WithCallback(new Emoji("👎"), (context, reaction) => reaction.Message.Value.DeleteAsync()));

            if (_cachedBehaviourConfiguration.HasDoneInitialSetUp)
            {
                return;
            }
        }

        private Task RunSetup()
        {
            return Task.CompletedTask;
        }
    }
}
