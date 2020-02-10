using System;
using System.Threading.Tasks;
using D2M.Bot.Handlers;
using D2M.Bot.Services;
using D2M.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
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
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ICachedBehaviourConfiguration _cachedBehaviourConfiguration;

        private readonly DiscordSocketClient _discordSocketClient;
        private readonly CommandService _commandService;
        
        private readonly BotConfiguration _botConfiguration;

        public BotClient(ILogger<BotClient> logger,
            IServiceScopeFactory serviceScopeFactory, 
            IOptions<BotConfiguration> botConfiguration, 
            ICachedBehaviourConfiguration cachedBehaviourConfiguration, 
            DiscordSocketClient discordSocketClient, CommandService commandService)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;

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

            using var scope = _serviceScopeFactory.CreateScope();
            await _commandService.AddModulesAsync(typeof(BotClient).Assembly, scope.ServiceProvider);
            await _discordSocketClient.LoginAsync(TokenType.Bot, _botConfiguration.DiscordToken);

            await _discordSocketClient.StartAsync();
        }

        private Task OnMessageReceived(SocketMessage message)
        {
            // Anything that isn't from an actual user, ignore
            if (!(message is SocketUserMessage receivedMessage) 
                || message.Author.IsBot 
                || message.Author.IsWebhook)
                return Task.CompletedTask;

            if (_cachedBehaviourConfiguration.IsDisabled)
                return Task.CompletedTask;

            var commandPrefix = _cachedBehaviourConfiguration.Prefix;

            var argPos = 0;

            var isIntendedForCommand = receivedMessage.HasCharPrefix(commandPrefix, ref argPos);
            var isIntendedForMention = receivedMessage.HasMentionPrefix(_discordSocketClient.CurrentUser, ref argPos);

            if (!(receivedMessage.Channel is IDMChannel)
                && !isIntendedForCommand
                && !isIntendedForMention)
                return Task.CompletedTask;

            using var scope = _serviceScopeFactory.CreateScope();

            // If this is a command, we don't care where this is happening (yet?)
            // just create the context and fire off to D.NET
            if (isIntendedForCommand)
            {
                Task.Run(async () => await ForwardCommand(argPos, receivedMessage));
            }
            // If we're in a direct message channel, forward the raw message
            else if (receivedMessage.Channel is IDMChannel)
            {
                Task.Run(async () => await ForwardMessage(receivedMessage));
            }

            return Task.CompletedTask;
        }

        private async Task ForwardCommand(int argPos, SocketUserMessage message)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var context = new SocketCommandContext(_discordSocketClient, message);
            await _commandService.ExecuteAsync(context, argPos, scope.ServiceProvider);
        }

        private async Task ForwardMessage(SocketUserMessage message)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IScopedMediator>();
            await mediator.Publish(new DirectMessageReceivedNotification(message));
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
