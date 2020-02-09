using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace D2M.Bot.Services
{
    public interface IDiscordMessageService
    {
        Task<IUserMessage?> WaitForNextMessageFromUser(ulong userId, TimeSpan timeUntilTimeout);
        Task<SocketReaction?> WaitForReactionFromUser(IUserMessage onMessage, ulong userId,
            TimeSpan timeUntilTimeout, params IEmote[] validReactions);
    }

    public class DiscordMessageService : IDiscordMessageService
    {
        private readonly DiscordSocketClient _discordSocketClient;

        public DiscordMessageService(DiscordSocketClient discordSocketClient)
        {
            _discordSocketClient = discordSocketClient;
        }

        public async Task<IUserMessage?> WaitForNextMessageFromUser(ulong userId, TimeSpan timeUntilTimeout)
        {
            var taskCompletionSource = new TaskCompletionSource<IUserMessage>();

            _discordSocketClient.MessageReceived += OnMessageReceived;

            var response = await Task.WhenAny(taskCompletionSource.Task, Task.Delay(timeUntilTimeout));

            _discordSocketClient.MessageReceived -= OnMessageReceived;

            if (response == taskCompletionSource.Task)
            {
                return await taskCompletionSource.Task;
            }

            return null;

            Task OnMessageReceived(SocketMessage socketMessage)
            {
                if (socketMessage.Author.Id == userId
                    && socketMessage is IUserMessage userMessage)
                {
                    taskCompletionSource.TrySetResult(userMessage);
                }

                return Task.CompletedTask;
            }
        }

        public async Task<SocketReaction?> WaitForReactionFromUser(IUserMessage onMessage, ulong userId, 
            TimeSpan timeUntilTimeout, params IEmote[] validReactions)
        {
            var taskCompletionSource = new TaskCompletionSource<SocketReaction>();

            foreach (var validReaction in validReactions)
            {
                await onMessage.AddReactionAsync(validReaction);
            }

            _discordSocketClient.ReactionAdded += OnReactionAdded;

            var response = await Task.WhenAny(taskCompletionSource.Task, Task.Delay(timeUntilTimeout));

            _discordSocketClient.ReactionAdded -= OnReactionAdded;

            if (response == taskCompletionSource.Task)
            {
                return await taskCompletionSource.Task;
            }

            return null;

            Task OnReactionAdded(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction)
            {
                if (cachedMessage.Id == onMessage.Id && reaction.UserId == userId && validReactions.Contains(reaction.Emote))
                {
                    taskCompletionSource.TrySetResult(reaction);
                }

                return Task.CompletedTask;
            }
        }
    }
}