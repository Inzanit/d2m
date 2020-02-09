using System.Threading.Tasks;
using Discord;

namespace D2M.Bot.Extensions
{
    public static class MessageExtensions
    {
        public static Task AddErrorEmote(this IUserMessage message)
        {
            return message.AddReactionAsync(new Emoji("❌"));
        }

        public static Task AddSuccessEmote(this IUserMessage message)
        {
            return message.AddReactionAsync(new Emoji("✅"));
        }
    }
}