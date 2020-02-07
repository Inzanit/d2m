using System;
using System.Threading.Tasks;
using D2M.Bot.Handlers;
using D2M.Common.Extensions;
using D2M.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace D2M.Bot
{
    public interface IBotClient
    {
        Task Start();
        Task Stop();
    }

    public class BotClient : IBotClient
    {
        private readonly ILogger<BotClient> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ICachedBehaviourConfiguration _cachedBehaviourConfiguration;

        private readonly DiscordSocketClient _discordSocketClient;
        private readonly CommandService _commandService;
        
        private readonly BotConfiguration _botConfiguration;

        public BotClient(ILogger<BotClient> logger, 
            IServiceProvider serviceProvider, 
            IOptions<BotConfiguration> botConfiguration, 
            ICachedBehaviourConfiguration cachedBehaviourConfiguration, 
            DiscordSocketClient discordSocketClient, CommandService commandService)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;

            _discordSocketClient = discordSocketClient;
            _commandService = commandService;
            _cachedBehaviourConfiguration = cachedBehaviourConfiguration;

            _botConfiguration = botConfiguration.Value;
        }

        public async Task Start()
        {
            _discordSocketClient.Log += OnLog;
            _discordSocketClient.Ready += OnReady;
            _discordSocketClient.Connected += OnConnected;
            _discordSocketClient.Disconnected += OnDisconnected;
            _discordSocketClient.MessageReceived += OnMessageReceived;

            _commandService.CommandExecuted += OnCommandExecuted;

            await _commandService.AddModulesAsync(typeof(BotClient).Assembly, _serviceProvider);
            await _discordSocketClient.LoginAsync(TokenType.Bot, _botConfiguration.DiscordToken);

            await _discordSocketClient.StartAsync();
        }

        private async Task OnMessageReceived(SocketMessage message)
        {
            // Anything that isn't from an actual user, ignore
            if (!(message is SocketUserMessage receivedMessage) 
                || message.Author.IsBot 
                || message.Author.IsWebhook) 
                return;

            if (_cachedBehaviourConfiguration.IsDisabled)
                return;

            var commandPrefix = _cachedBehaviourConfiguration.Prefix;

            var argPos = 0;

            var isIntendedForCommand = receivedMessage.HasCharPrefix(commandPrefix, ref argPos);
            var isIntendedForMention = receivedMessage.HasMentionPrefix(_discordSocketClient.CurrentUser, ref argPos);

            if (!(receivedMessage.Channel is IDMChannel)
                && !isIntendedForCommand
                && !isIntendedForMention) 
                return;

            // If this is a command, we don't care where this is happening (yet?)
            // just create the context and fire off to D.NET
            if (isIntendedForCommand)
            {
                var context = new SocketCommandContext(_discordSocketClient, receivedMessage);

                await _commandService.ExecuteAsync(context, argPos, _serviceProvider);
            }
            // If we're in a direct message channel, forward the raw message
            else if (receivedMessage.Channel is IDMChannel)
            {
                using var scope = _serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                mediator.Publish(new DirectMessageReceivedNotification(receivedMessage)).FireAndForget();
            }
        }

        private Task OnCommandExecuted(Optional<CommandInfo> commandInfo, ICommandContext context, IResult result)
        {
            if (!result.IsSuccess)
            {
                _logger.LogError("Failed running command {CommandName} for reason {Reason} in guild {Guild} ({GuildId}) for user {User} ({UserId})", 
                    commandInfo.IsSpecified ? commandInfo.Value.Name : "Unknown", 
                    result.ErrorReason,
                    context.Guild?.Name ?? "DM",
                    context.Guild?.Id ?? context.User.Id,
                    context.User.Username,
                    context.User.Id);
            }

            return Task.CompletedTask;
        }

        private Task OnConnected()
        {
            _logger.LogInformation("Discord client connected");
            return Task.CompletedTask;
        }

        private Task OnDisconnected(Exception arg)
        {
            _logger.LogInformation("Discord client disconnected");
            return Task.CompletedTask;
        }

        private Task OnReady()
        {
            _logger.LogInformation("Discord client ready");
            return Task.CompletedTask;
        }

        private Task OnLog(LogMessage log)
        {
            var level = log.Severity switch
            {
                LogSeverity.Verbose => LogLevel.Debug,
                LogSeverity.Debug => LogLevel.Debug,
                LogSeverity.Info => LogLevel.Information,
                LogSeverity.Warning => LogLevel.Warning,
                LogSeverity.Error => LogLevel.Error,
                LogSeverity.Critical => LogLevel.Critical,
                _ => throw new ArgumentOutOfRangeException($"{log.Severity} log out of range")
            };

            _logger.Log(level, log.Exception,
                log.Source != null
                    ? $"{log.Message} at source {log.Source}"
                    : log.Message);

            return Task.CompletedTask;
        }

        public Task Stop()
        {
            return _discordSocketClient.LogoutAsync();
        }
    }
}
