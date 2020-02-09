using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using D2M.Bot.Extensions;
using D2M.Bot.Services;
using D2M.Common;
using D2M.Services;
using Discord;
using MediatR;

namespace D2M.Bot.Handlers
{
    public class SetUpRequest : INotification
    {
        public SetUpRequest(IUserMessage message)
        {
            Message = message;
        }

        public IUserMessage Message { get; }
    }

    public class SetUpRequestHandler : INotificationHandler<SetUpRequest>
    {
        private readonly IBehaviourConfigurationService _behaviourConfigurationService;
        private readonly IDiscordMessageService _discordMessageService;
        private readonly IDiscordGuildService _discordGuildService;

        public SetUpRequestHandler(IBehaviourConfigurationService behaviourConfigurationService,
            IDiscordGuildService discordGuildService, IDiscordMessageService discordMessageService)
        {
            _behaviourConfigurationService = behaviourConfigurationService;
            _discordGuildService = discordGuildService;
            _discordMessageService = discordMessageService;
        }

        private Dictionary<string, Func<SetUpRequest, IUserMessage, Task>> ReactionHandlers => new Dictionary<string, Func<SetUpRequest, IUserMessage, Task>>
        {
            [EmoteHelper.NumericEmotes[0].Name] = HandlePrefixUpdate,
            [EmoteHelper.NumericEmotes[2].Name] = HandleCategoryUpdate,
        };

        public async Task Handle(SetUpRequest request, CancellationToken cancellationToken)
        {
            var setupMessage = await request.Message.Channel.SendMessageAsync(embed: BuildSetupEmbed());

            var setupReaction = await _discordMessageService.WaitForReactionFromUser(setupMessage, request.Message.Author.Id,
                TimeSpan.FromSeconds(30), EmoteHelper.NumericEmotes[0], EmoteHelper.NumericEmotes[1], EmoteHelper.NumericEmotes[2], EmoteHelper.CancelEmote);

            if (setupReaction is null)
                return;

            if (setupReaction.Emote.Equals(EmoteHelper.CancelEmote))
            {
                await setupMessage.RemoveAllReactionsAsync();
                return;
            }

            await ReactionHandlers[setupReaction.Emote.Name](request, setupMessage);
        }

        private Embed BuildSetupEmbed()
        {
            var setupPayload = new StringBuilder();

            setupPayload.AppendLine($"{new Emoji("0️⃣✅")} Command prefix (current: `{_behaviourConfigurationService.GetPrefix()}`)");

            var staffRole = _behaviourConfigurationService.GetStaffRole();

            setupPayload.AppendLine(staffRole is null
                ? $"{new Emoji("1️⃣🟥")} Staff role"
                : $"{new Emoji("1️⃣✅")} Staff role ({MentionUtils.MentionRole(staffRole.Value)})");

            var categoryId = _behaviourConfigurationService.GetCategoryId();

            setupPayload.AppendLine(categoryId is null
                ? $"{new Emoji("2️⃣🟥")} Category"
                : $"{new Emoji("2️⃣✅")} Category ({MentionUtils.MentionChannel(categoryId.Value)})");

            var logChannelId = _behaviourConfigurationService.GetLogChannelId();

            setupPayload.AppendLine(logChannelId is null
                ? $"{new Emoji("3️⃣🟥")} Log channel"
                : $"{new Emoji("3️⃣✅")} Log channel ({MentionUtils.MentionChannel(logChannelId.Value)})");

            var setupEmbed = new EmbedBuilder()
                .WithTitle("D2M Setup")
                .WithDescription(setupPayload.ToString());

            return setupEmbed.Build();
        }

        private async Task HandlePrefixUpdate(SetUpRequest request, IUserMessage message)
        {
            await message.Channel.SendMessageAsync("Enter prefix for all commands, this needs to be one letter! E.g. `?`, `!`, `.`");

            while (true)
            {
                var response = await _discordMessageService.WaitForNextMessageFromUser(request.Message.Author.Id, TimeSpan.FromSeconds(30));

                if (response is null)
                    break;

                var isValidChar = char.TryParse(response.Content, out var newPrefix);

                if (!isValidChar
                    || string.IsNullOrWhiteSpace(response.Content))
                {
                    await response.AddErrorEmote();
                    continue;
                }

                await _behaviourConfigurationService.SetPrefix(newPrefix);

                await response.AddSuccessEmote();

                break;
            }
        }

        private async Task HandleCategoryUpdate(SetUpRequest request, IUserMessage message)
        {
            await message.Channel.SendMessageAsync("Enter name or Id of the category for all new threads to be nested in or reply `auto` to create a new category");

            while (true)
            {
                var response = await _discordMessageService.WaitForNextMessageFromUser(request.Message.Author.Id, TimeSpan.FromSeconds(30));

                if (response is null)
                    break;

                if (string.Equals(response.Content, "auto", StringComparison.InvariantCultureIgnoreCase))
                {
                    var hasExistingD2MCategory = _discordGuildService.HasCategory(Constants.DEFAULT_CATEGORY_NAME);

                    if (hasExistingD2MCategory)
                    {
                        var existingCategoryId = _discordGuildService.GetCategoryId(Constants.DEFAULT_CATEGORY_NAME);
                        await _behaviourConfigurationService.SetCategory(existingCategoryId);
                    }
                    else
                    {
                        var createdCategoryId = await _discordGuildService.CreateCategory(Constants.DEFAULT_CATEGORY_NAME);
                        await _behaviourConfigurationService.SetCategory(createdCategoryId);
                    }

                    await response.AddSuccessEmote();

                    break;
                }

                var isValidUlong = ulong.TryParse(response.Content, out var categoryId);

                if (isValidUlong)
                {
                    var isValidCategory = _discordGuildService.HasCategory(categoryId);

                    if (!isValidCategory)
                    {
                        await response.AddErrorEmote();
                        continue;
                    }

                    await _behaviourConfigurationService.SetCategory(categoryId);
                    await response.AddSuccessEmote();

                    break;
                }

                // assume anything else is the raw name of the category

                var hasNamedCategory = _discordGuildService.HasCategory(response.Content);

                if (!hasNamedCategory)
                {
                    await response.AddErrorEmote();
                    continue;
                }

                var namedCategoryId = _discordGuildService.GetCategoryId(response.Content);
                await _behaviourConfigurationService.SetCategory(namedCategoryId);
                await response.AddSuccessEmote();

                break;
            }
        }
    }
}
