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
        private readonly IPermissionService _permissionService;

        public SetUpRequestHandler(IBehaviourConfigurationService behaviourConfigurationService,
            IDiscordGuildService discordGuildService, IDiscordMessageService discordMessageService, 
            IPermissionService permissionService)
        {
            _behaviourConfigurationService = behaviourConfigurationService;
            _discordGuildService = discordGuildService;
            _discordMessageService = discordMessageService;
            _permissionService = permissionService;
        }

        private Dictionary<string, Func<SetUpRequest, IUserMessage, Task>> ReactionHandlers => new Dictionary<string, Func<SetUpRequest, IUserMessage, Task>>
        {
            [EmoteHelper.NumericEmotes[0].Name] = HandlePrefixUpdate,
            [EmoteHelper.NumericEmotes[1].Name] = HandleStaffRoleUpdate,
            [EmoteHelper.NumericEmotes[2].Name] = HandleCategoryUpdate,
            [EmoteHelper.NumericEmotes[3].Name] = HandleLogChannelUpdate,
        };

        public async Task Handle(SetUpRequest request, CancellationToken cancellationToken)
        {
            var setupMessage = await request.Message.Channel.SendMessageAsync(embed: BuildSetupEmbed());

            var setupReaction = await _discordMessageService.WaitForReactionFromUser(setupMessage, request.Message.Author.Id,
                TimeSpan.FromSeconds(30),
                EmoteHelper.NumericEmotes[0],
                EmoteHelper.NumericEmotes[1],
                EmoteHelper.NumericEmotes[2],
                EmoteHelper.NumericEmotes[3],
                EmoteHelper.CancelEmote);

            if (setupReaction is null
                || setupReaction.Emote.Equals(EmoteHelper.CancelEmote)
                || !ReactionHandlers.ContainsKey(setupReaction.Emote.Name))
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

            var staffRole = _behaviourConfigurationService.GetStaffRoleId();

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

        private async Task HandleStaffRoleUpdate(SetUpRequest request, IUserMessage message)
        {
            await message.Channel.SendMessageAsync("Enter the staff role used for high level permissions");

            while (true)
            {
                var response = await _discordMessageService.WaitForNextMessageFromUser(request.Message.Author.Id, TimeSpan.FromSeconds(30));

                if (response is null)
                    break;

                var isValidUlong = ulong.TryParse(response.Content, out var roleId)
                                   || MentionUtils.TryParseRole(response.Content, out roleId);

                if (isValidUlong)
                {
                    var isValidCategory = _discordGuildService.HasRole(roleId);

                    if (!isValidCategory)
                    {
                        await response.AddErrorEmote();
                        continue;
                    }

                    await _behaviourConfigurationService.SetStaffRole(roleId);
                    await response.AddSuccessEmote();

                    break;
                }

                var hasNamedRole = _discordGuildService.HasRole(response.Content);

                if (!hasNamedRole)
                {
                    await response.AddErrorEmote();
                    continue;
                }

                var namedRoleId = _discordGuildService.GetRoleId(response.Content);

                await _behaviourConfigurationService.SetStaffRole(namedRoleId);

                await response.AddSuccessEmote();

                break;
            }
        }

        private async Task HandleCategoryUpdate(SetUpRequest request, IUserMessage message)
        {
            var staffRoleId = _behaviourConfigurationService.GetStaffRoleId();

            if (staffRoleId is null)
            {
                await message.Channel.SendMessageAsync("Cannot set up log channel without staff role");
                await message.AddErrorEmote();
                return;
            }

            await message.Channel.SendMessageAsync("Enter name or Id of the category for all new threads to be nested in or reply `auto` to create a new category");

            while (true)
            {
                // Loop until we have a response from the user, or break if we don't get a response, i.e. if
                // we hit the soft timeout of the waiting period for the next message

                var response = await _discordMessageService.WaitForNextMessageFromUser(request.Message.Author.Id, TimeSpan.FromSeconds(30));

                if (response is null)
                    break;

                // User has indicated they want the bot to set up the category
                if (string.Equals(response.Content, "auto", StringComparison.InvariantCultureIgnoreCase))
                {
                    // We need to check if there is a category with this name already, since we'll 
                    // likely need to reuse this channel if it exists
                    var hasExistingD2MCategory = _discordGuildService.HasCategory(Constants.DEFAULT_CATEGORY_NAME);

                    if (hasExistingD2MCategory)
                    {
                        // If it exists, get the ID and hook up automagically for us, since the category
                        // was either originally created by D2M or the user self-created this category
                        var existingCategoryId = _discordGuildService.GetCategoryId(Constants.DEFAULT_CATEGORY_NAME);
                        await _behaviourConfigurationService.SetCategory(existingCategoryId);
                    }
                    else
                    {
                        // Create the category
                        var createdCategoryId = await _discordGuildService.CreateCategory(Constants.DEFAULT_CATEGORY_NAME, 1);
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

                // Assume anything else is the raw name of the category

                var sanitizedCategoryMention = response.Content.Replace("#", string.Empty);

                var hasNamedCategory = _discordGuildService.HasCategory(sanitizedCategoryMention);

                if (!hasNamedCategory)
                {
                    await response.AddErrorEmote();
                    continue;
                }

                var namedCategoryId = _discordGuildService.GetCategoryId(sanitizedCategoryMention);
                await _behaviourConfigurationService.SetCategory(namedCategoryId);
                await response.AddSuccessEmote();

                break;
            }

            var setUpCategoryId = _behaviourConfigurationService.GetCategoryId();

            if (setUpCategoryId is null)
                return;

            await _permissionService.RestrictCategoryToStaff(setUpCategoryId.Value, staffRoleId.Value);
        }

        private async Task HandleLogChannelUpdate(SetUpRequest request, IUserMessage message)
        {
            var staffRoleId = _behaviourConfigurationService.GetStaffRoleId();

            if (staffRoleId is null)
            {
                await message.Channel.SendMessageAsync("Cannot set up log channel without staff role");
                await message.AddErrorEmote();
                return;
            }

            await message.Channel.SendMessageAsync("Enter name or Id of the channel for all logs to be sent to or reply `auto` to create a new channel, under the D2M category");

            while (true)
            {
                var response = await _discordMessageService.WaitForNextMessageFromUser(request.Message.Author.Id, TimeSpan.FromSeconds(30));

                if (response is null)
                    break;

                if (string.Equals(response.Content, "auto", StringComparison.InvariantCultureIgnoreCase))
                {
                    var hasExistingD2MLogChannel = _discordGuildService.HasChannel(Constants.DEFAULT_LOG_CHANNEL_NAME);

                    if (hasExistingD2MLogChannel)
                    {
                        var existingLogChannelId = _discordGuildService.GetChannelId(Constants.DEFAULT_LOG_CHANNEL_NAME);
                        await _behaviourConfigurationService.SetLogChannel(existingLogChannelId);
                    }
                    else
                    {
                        var categoryId = _behaviourConfigurationService.GetCategoryId();

                        if (categoryId is null)
                        {
                            await message.Channel.SendMessageAsync("Cannot set up log channel without D2M category");
                            await message.AddErrorEmote();
                            return;
                        }

                        var createdChannelId = await _discordGuildService.CreateChannel(Constants.DEFAULT_LOG_CHANNEL_NAME, "Logs for D2M", categoryId.Value, 1);
                        await _behaviourConfigurationService.SetLogChannel(createdChannelId);
                    }

                    await response.AddSuccessEmote();

                    break;
                }

                var isValidUlong = ulong.TryParse(response.Content, out var channelId)
                                   || MentionUtils.TryParseChannel(response.Content, out channelId);

                if (isValidUlong)
                {
                    var isValidChannel = _discordGuildService.HasChannel(channelId);

                    if (!isValidChannel)
                    {
                        await response.AddErrorEmote();
                        continue;
                    }

                    await _behaviourConfigurationService.SetLogChannel(channelId);
                    await response.AddSuccessEmote();

                    break;
                }

                var sanitzedChannelMention = response.Content.Replace("#", string.Empty);

                var hasNamedCategory = _discordGuildService.HasChannel(sanitzedChannelMention);

                if (!hasNamedCategory)
                {
                    await response.AddErrorEmote();
                    continue;
                }

                var namedCategoryId = _discordGuildService.GetChannelId(sanitzedChannelMention);
                await _behaviourConfigurationService.SetLogChannel(namedCategoryId);
                await response.AddSuccessEmote();

                break;
            }

            var setUpLogChannelId = _behaviourConfigurationService.GetLogChannelId();

            if (setUpLogChannelId is null)
                return;

            await _permissionService.SynchronisePermissionsWithCategory(setUpLogChannelId.Value);
            await _permissionService.AddOverrideForLogChannel(setUpLogChannelId.Value, staffRoleId.Value);
        }
    }
}
