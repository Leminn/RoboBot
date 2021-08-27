using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace RoboBot
{
    public class ReactionSetupCommands : BaseCommandModule
    {
        private static class CommandNames
        {
            public const string StartSetup = "reactsetup";
            public const string FinishSetup = "reactfinish";
            
            public const string PreviewReactionMessage = "reactpreview";
            
            public const string EditReactionMessage = "reactedit";
            public const string RemoveReactionMessage = "reactremove";
            
            public const string AddEmojiToReactionMessage = "reactadd";

            public const string ListReactionMessages = "reactlist";
        }

        private static ReactionInteractions _reactionInteractions;
        
        private bool IsSettingUpReactionMessage = false;
        private bool IsEditingReactionMessage = false;

        private ReactionMessage reactionMessage = new ReactionMessage();
        private ReactionMessage originalOfEditedMessage = new ReactionMessage();

        private const Permissions RequiredPermissions = Permissions.Administrator;

        public static void SetReactionInteractions(ReactionInteractions reactionInteractions) => _reactionInteractions = reactionInteractions;
        
        [RequireGuild]
        [Command(CommandNames.StartSetup)]
        public async Task StartSetup(CommandContext ctx, string messageUrl)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;

            if (IsSettingUpReactionMessage || IsEditingReactionMessage)
            {
                await ctx.RespondAsync(
                    "The bot is already setting up / editing a reaction message, finish the previous one and try again");
                return;
            }

            DiscordMessage message = await CommandHelpers.GetMessageFromUrl(ctx, messageUrl);
            
            if (message is null)
                return;

            if (_reactionInteractions.ReactionMessages.FirstOrDefault(x => x.Message.Equals(message)) != null)
            {
                await ctx.RespondAsync("This message already got reaction interactions setup");
                return;
            }

            reactionMessage.Message = message;

            await ctx.RespondAsync($"Got the message to use, continue the setup with {ctx.Prefix}{CommandNames.AddEmojiToReactionMessage} (emoji) (mention to role)\nPreview the rules with {ctx.Prefix}{CommandNames.PreviewReactionMessage}");
            
            IsSettingUpReactionMessage = true;
        }
        
        [RequireGuild]
        [Command(CommandNames.StartSetup)]
        public async Task StartSetup(CommandContext ctx)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;
            
            await ctx.RespondAsync("You need to provide a message id to start the setup");
        }
        
        [RequireGuild]
        [Command(CommandNames.EditReactionMessage)]
        public async Task EditReactionMessage(CommandContext ctx, string messageUrl)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;
            
            if (IsSettingUpReactionMessage || IsEditingReactionMessage)
            {
                await ctx.RespondAsync(
                    "The bot is already setting up / editing a reaction message, finish it then try again");
                return;
            }
            
            DiscordMessage message = await CommandHelpers.GetMessageFromUrl(ctx, messageUrl);
            
            if (message is null)
                return;

            ReactionMessage associatedMessage = _reactionInteractions.ReactionMessages.FirstOrDefault(x => x.Message.Equals(message));
            
            if (associatedMessage == null)
            {
                await ctx.RespondAsync("This message doesn't have reaction interactions setup");
                return;
            }
            
            reactionMessage.Message = associatedMessage.Message;
            foreach (KeyValuePair<DiscordEmoji, DiscordRole> rule in associatedMessage.Rules)
            {
                reactionMessage.Rules.Add(rule.Key, rule.Value);
            }
            
            originalOfEditedMessage = associatedMessage;

            await ctx.RespondAsync($"Got the message to edit, continue the edit with {ctx.Prefix}{CommandNames.AddEmojiToReactionMessage} (emoji) (mention to role)\nPreview the rules with {ctx.Prefix}{CommandNames.PreviewReactionMessage}");
            
            IsEditingReactionMessage = true;
        }
        
        [RequireGuild]
        [Command(CommandNames.EditReactionMessage)]
        public async Task EditReactionMessage(CommandContext ctx)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;
            
            await ctx.RespondAsync("You need to provide a message id to start the edit");
        }
        
        [RequireGuild]
        [Command(CommandNames.AddEmojiToReactionMessage)]
        public async Task AddEmojiToReactionMessage(CommandContext ctx, DiscordEmoji emoji, DiscordRole role)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;
            
            if (!IsSettingUpReactionMessage && !IsEditingReactionMessage)
            {
                await ctx.RespondAsync("You need to have entered setup or edit mode to add reactions");
                return;
            }
            
            reactionMessage.Rules.Add(emoji, role);
            //await ctx.RespondAsync($"Added {emoji} as the role \"{role.Name}\" to {reactionMessage.Message.JumpLink}");
        }
        
        [RequireGuild]
        [Command(CommandNames.AddEmojiToReactionMessage)]
        public async Task AddEmojiToReactionMessage(CommandContext ctx)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;
            
            await ctx.RespondAsync("You need to provide an emoji and mention the role to attribute it to");
        }
        
        [RequireGuild]
        [Command(CommandNames.AddEmojiToReactionMessage)]
        public async Task AddEmojiToReactionMessage(CommandContext ctx, params string[] rest)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;
            
            await ctx.RespondAsync("You need to provide an emoji and mention the role to attribute it to");
        }

        [RequireGuild]
        [Command(CommandNames.PreviewReactionMessage)]
        public async Task PreviewReactionMessage(CommandContext ctx)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;
            
            if (!IsSettingUpReactionMessage && !IsEditingReactionMessage)
            {
                await ctx.RespondAsync("You need to have entered setup or edit mode before previewing the result");
                return;
            }
            
            if (reactionMessage.Rules.Count == 0)
            {
                await ctx.RespondAsync("You need to add at least 1 reaction and role to finish the setup");
                return;
            }

            await ctx.RespondAsync(reactionMessage.ToString());
        }
        
        [RequireGuild]
        [Command(CommandNames.FinishSetup)]
        public async Task FinishSetup(CommandContext ctx)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;
            
            if (!IsSettingUpReactionMessage && !IsEditingReactionMessage)
            {
                await ctx.RespondAsync("You need to have entered setup or edit mode before finishing it");
                return;
            }
            
            if (reactionMessage.Rules.Count == 0)
            {
                await ctx.RespondAsync("You need to have at least 1 reaction and role to finish the setup / edit");
                return;
            }

            if (IsEditingReactionMessage)
                _reactionInteractions.ReactionMessages.Remove(originalOfEditedMessage);

            _reactionInteractions.ReactionMessages.Add(reactionMessage);
            _reactionInteractions.SaveToFile();

            string rulesString = "";
            foreach (KeyValuePair<DiscordEmoji, DiscordRole> rule in reactionMessage.Rules)
            {
                rulesString += rule.Key + " : " + rule.Value.Name + '\n';
                try
                {
                    await reactionMessage.Message.CreateReactionAsync(rule.Key);
                }
                catch (Exception e)
                {
                    await ctx.RespondAsync(e.ToString());
                    throw;
                }
            }

            await ctx.RespondAsync(
                $"Setup / edit finished!\nMessage : {reactionMessage.Message.JumpLink}\nRules :\n{rulesString}");

            reactionMessage = new ReactionMessage();
            originalOfEditedMessage = new ReactionMessage();
            
            IsSettingUpReactionMessage = false;
            IsEditingReactionMessage = false;
        }
        
        [RequireGuild]
        [Command(CommandNames.ListReactionMessages)]
        public async Task ListReactionMessages(CommandContext ctx)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;
            
            StringBuilder sb = new StringBuilder("Reaction Messages :");
            foreach (ReactionMessage reactionMessage in _reactionInteractions.ReactionMessages.Where(x => x.Message.Channel.GuildId == ctx.Channel.GuildId))
            {
                sb.Append("\n\n" + reactionMessage.ToString() + '\n');
            }

            await ctx.RespondAsync(sb.ToString());
        }
        
        [RequireGuild]
        [Command(CommandNames.RemoveReactionMessage)]
        public async Task RemoveReactionMessage(CommandContext ctx, string messageToUse)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;

            DiscordMessage message = await CommandHelpers.GetMessageFromUrl(ctx, messageToUse);

            if (message is null)
                return;

            ReactionMessage associatedReactionMessage =
                _reactionInteractions.ReactionMessages.FirstOrDefault(x => x.Message.Equals(message));
            
            if (associatedReactionMessage == null)
            {
                await ctx.RespondAsync("Couldn't find the ReactionMessage associated with this Discord message");
                return;
            }

            _reactionInteractions.ReactionMessages.Remove(associatedReactionMessage);
            await ctx.RespondAsync("Succesfully removed the corresponding ReactionMessage");
            
            _reactionInteractions.SaveToFile();
            
            foreach (KeyValuePair<DiscordEmoji, DiscordRole> rule in associatedReactionMessage.Rules)
            {
                try
                {
                    await associatedReactionMessage.Message.DeleteOwnReactionAsync(rule.Key);
                }
                catch { }
            }
        }

        [RequireGuild]
        [Command(CommandNames.RemoveReactionMessage)]
        public async Task RemoveReactionMessage(CommandContext ctx)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;

            await ctx.RespondAsync("You need to provide the original message id you want to delete");
        }
    }
}