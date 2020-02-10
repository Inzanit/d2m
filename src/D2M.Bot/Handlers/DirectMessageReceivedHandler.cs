using System.Threading;
using System.Threading.Tasks;
using D2M.Bot.Services;
using D2M.Services;
using Discord.WebSocket;
using MediatR;

namespace D2M.Bot.Handlers
{
    public class DirectMessageReceivedNotification : INotification
    {
        public DirectMessageReceivedNotification(SocketUserMessage receivedMessage)
        {
            ReceivedMessage = receivedMessage;
        }

        public SocketUserMessage ReceivedMessage { get; }
    }

    public class DirectMessageReceivedHandler : INotificationHandler<DirectMessageReceivedNotification>
    {
        private readonly IThreadService _threadService;
        private readonly IDiscordGuildService _discordGuildService;
        private readonly IWorkflowService _workflowService;
        private readonly IBehaviourConfigurationService _behaviourConfigurationService;

        public DirectMessageReceivedHandler(IThreadService threadService, IDiscordGuildService discordGuildService, 
            IBehaviourConfigurationService behaviourConfigurationService, IWorkflowService workflowService)
        {
            _threadService = threadService;
            _discordGuildService = discordGuildService;
            _behaviourConfigurationService = behaviourConfigurationService;
            _workflowService = workflowService;
        }

        public async Task Handle(DirectMessageReceivedNotification notification, CancellationToken cancellationToken)
        {
            if (!_behaviourConfigurationService.HasValidConfiguration())
            {
                await notification.ReceivedMessage.Channel.SendMessageAsync(
                    "Staff on the receiving server have not set up the bot, the message could not be sent!");

                return;
            }

            var userId = notification.ReceivedMessage.Author.Id;

            var hasOpenThread = await _threadService.HasOpenThread(userId);

            ulong channelId;

            if (!hasOpenThread)
            {
                channelId = await _workflowService.CreateChannelForThread(notification.ReceivedMessage.Author);
                await _threadService.StartThread(userId, channelId);
            }
            else
            {
                channelId = await _threadService.GetOpenThreadChannelId(userId);
            }

            var channel = _discordGuildService.GetChannel(channelId);

            await channel.SendMessageAsync(notification.ReceivedMessage.Content);
        }
    }
}
