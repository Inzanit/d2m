using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace D2M.Bot.Services
{
    public interface IScopedMediator
    {
        Task Publish(INotification notification);
    }

    public class ScopedMediator : IScopedMediator
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ScopedMediator(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Publish(INotification notification)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IMediator>();
            await service.Publish(notification);
        }
    }
}
