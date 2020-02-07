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

        public DirectMessageReceivedHandler(IThreadService threadService, IDiscordGuildService discordGuildService)
        {
            _threadService = threadService;
            _discordGuildService = discordGuildService;
        }

        public async Task Handle(DirectMessageReceivedNotification notification, CancellationToken cancellationToken)
        {
            var userId = notification.ReceivedMessage.Author.Id;

            var hasOpenThread = await _threadService.HasOpenThread(userId);

            if (!hasOpenThread)
            {
                // todo forward to existing channel
            }
        }
    }
}
