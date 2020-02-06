using System.Threading.Tasks;
using Discord.Commands;

namespace D2M.Bot.Modules
{
    [Group("debug")]
    public class DebugModule : ModuleBase
    {
        [Command("ping")]
        public Task Ping()
        {
            return ReplyAsync("Pong!");
        }
    }
}
