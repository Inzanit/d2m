using System;
using System.Text;
using System.Threading.Tasks;
using D2M.Bot.Handlers;
using D2M.Bot.Services;
using D2M.Common.Extensions;
using D2M.Services;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using MediatR;

namespace D2M.Bot.Modules
{
    [RequireUserPermission(GuildPermission.Administrator)]
    public class SetUpModule : InteractiveBase
    {
        private readonly IScopedMediator _mediator;

        public SetUpModule(IScopedMediator mediator)
        {
            _mediator = mediator;
        }

        [Command("setup")]
        public Task SetUp()
        {
            _mediator.Publish(new SetUpRequest(Context.Message)).FireAndForget();

            return Task.CompletedTask;
        }
    }
}
