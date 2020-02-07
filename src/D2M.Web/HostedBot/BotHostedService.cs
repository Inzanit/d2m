using System.Threading;
using System.Threading.Tasks;
using D2M.Bot;
using Microsoft.Extensions.Hosting;

namespace D2M.Web.HostedBot
{
    public class BotHostedService : IHostedService
    {
        private readonly IBotClient _botClient;

        public BotHostedService(IBotClient botClient)
        {
            _botClient = botClient;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _botClient.Start();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _botClient.Stop();
        }
    }
}
