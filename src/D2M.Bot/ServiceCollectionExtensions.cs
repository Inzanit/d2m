using System;
using System.IO;
using D2M.Bot.Services;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace D2M.Bot
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBotClient(this IServiceCollection collection)
        {
            var environmentName = Environment.GetEnvironmentVariable("D2M_ENVIRONMENT");

            var pathToAppSettings = Directory.GetCurrentDirectory();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(pathToAppSettings)
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile($"appsettings.{environmentName}.json", true)
                .AddEnvironmentVariables()
                .AddUserSecrets<BotClient>(true)
                .Build();

            return collection
                .AddOptions()
                .Configure<BotConfiguration>(configuration)
                .AddSingleton<IDiscordGuildService, DiscordGuildService>()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<InteractiveService>()
                .AddSingleton<CommandService>()
                .AddSingleton<IBotClient, BotClient>()
                .AddMediatR(x => x.AsScoped(), typeof(BotClient).Assembly);
        }
    }
}