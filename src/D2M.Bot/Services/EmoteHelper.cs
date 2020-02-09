using System.Collections.Generic;
using Discord;

namespace D2M.Bot.Services
{
    public static class EmoteHelper
    {
        public static IEmote CancelEmote = new Emoji("❌");

        public static Dictionary<int, IEmote> NumericEmotes = new Dictionary<int, IEmote>
        {
            [0] = new Emoji("0️⃣"),
            [1] = new Emoji("1️⃣"),
            [2] = new Emoji("2️⃣"),
            [3] = new Emoji("3️⃣"),
            [4] = new Emoji("4️⃣"),
            [5] = new Emoji("5️⃣"),
        };
    }
}