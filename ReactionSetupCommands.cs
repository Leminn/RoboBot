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
            public const string AbortSetup = "reactabort";
            public const string RestartSetup = "reactrestart";
            
            public const string PreviewReactionMessage = "reactpreview";
            
            public const string EditReactionMessage = "reactedit";
            public const string RemoveReactionMessage = "reactremove";
            
            public const string AddEmojiToReactionMessage = "reactadd";
            public const string DeleteEmojiFromReactionMessage = "reactdel";

            public const string ListReactionMessages = "reactlist";
            public const string CheckReactionMessage = "reactcheck";
        }

        private class SetupState
        {
            public bool IsInSetupMode;
            public bool IsInEditMode;

            public ReactionMessage ReactionMessage = new ReactionMessage();
            public ReactionMessage OriginalReactionMessage = new ReactionMessage();
        }

        private static ReactionInteractions _reactionInteractions;

        private const Permissions RequiredPermissions = Permissions.Administrator;

        private static Dictionary<ulong, SetupState> _setupStates = new Dictionary<ulong, SetupState>();

        public static void SetReactionInteractions(ReactionInteractions reactionInteractions) => _reactionInteractions = reactionInteractions;
        
        public static void CreateGuildState(ulong guildId) => _setupStates.Add(guildId, new SetupState());
        
        public static void RemoveGuildState(ulong guildId) => _setupStates.Remove(guildId);

        [RequireGuild]
        [Command(CommandNames.StartSetup)]
        public async Task StartSetup(CommandContext ctx, string messageUrl)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;

            SetupState state = _setupStates[ctx.Guild.Id];

            if (state.IsInSetupMode || state.IsInEditMode)
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

            state.ReactionMessage.Message = message;

            await ctx.RespondAsync($"Got the message to use, continue the setup with {ctx.Prefix}{CommandNames.AddEmojiToReactionMessage} (emoji) (mention to role)\nPreview the rules with {ctx.Prefix}{CommandNames.PreviewReactionMessage}");
            
            state.IsInSetupMode = true;
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
            
            SetupState state = _setupStates[ctx.Guild.Id];

            if (state.IsInSetupMode || state.IsInEditMode)
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
            
            state.ReactionMessage.Message = associatedMessage.Message;
            foreach (KeyValuePair<DiscordEmoji, DiscordRole> rule in associatedMessage.Rules)
            {
                state.ReactionMessage.Rules.Add(rule.Key, rule.Value);
            }
            
            state.OriginalReactionMessage = associatedMessage;

            await ctx.RespondAsync($"Got the message to edit, continue the edit with {ctx.Prefix}{CommandNames.AddEmojiToReactionMessage} (emoji) (mention to role)\nPreview the rules with {ctx.Prefix}{CommandNames.PreviewReactionMessage}");
            
            state.IsInEditMode = true;
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
        [Command(CommandNames.AbortSetup)]
        public async Task AbortSetup(CommandContext ctx)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;
            
            SetupState state = _setupStates[ctx.Guild.Id];
            
            if (!state.IsInSetupMode && !state.IsInEditMode)
            {
                await ctx.RespondAsync("Nothing to abort");
                return;
            }
            
            state.ReactionMessage = new ReactionMessage();
            state.OriginalReactionMessage = new ReactionMessage();
            
            state.IsInSetupMode = false;
            state.IsInEditMode = false;
            
            await ctx.RespondAsync("Aborted setup / edit");
        }
        
        [RequireGuild]
        [Command(CommandNames.RestartSetup)]
        public async Task RestartSetup(CommandContext ctx)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;
            
            SetupState state = _setupStates[ctx.Guild.Id];
            
            if (!state.IsInSetupMode && !state.IsInEditMode)
            {
                await ctx.RespondAsync("Nothing to restart");
                return;
            }

            if (state.IsInSetupMode)
            {
                state.ReactionMessage.Rules = new Dictionary<DiscordEmoji, DiscordRole>();
            }
            else if (state.IsInEditMode)
            {
                state.ReactionMessage = new ReactionMessage();
                
                state.ReactionMessage.Message = state.OriginalReactionMessage.Message;
                foreach (KeyValuePair<DiscordEmoji, DiscordRole> rule in state.OriginalReactionMessage.Rules)
                {
                    state.ReactionMessage.Rules.Add(rule.Key, rule.Value);
                }
            }

            await ctx.RespondAsync("Restarted setup / edit");
        }
        
        [RequireGuild]
        [Command(CommandNames.AddEmojiToReactionMessage)]
        public async Task AddEmojiToReactionMessage(CommandContext ctx, DiscordEmoji emoji, DiscordRole role)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;
            
            SetupState state = _setupStates[ctx.Guild.Id];
            
            if (!state.IsInSetupMode && !state.IsInEditMode)
            {
                await ctx.RespondAsync("You need to have entered setup or edit mode to add reactions");
                return;
            }
            
            //FIXME: This is pretty bad, try to find an other solution.
            //Check to see if the bot has access to the emoji or not
            try
            {
                await ctx.Message.CreateReactionAsync(emoji);
                await ctx.Message.DeleteOwnReactionAsync(emoji);
            }
            catch
            {
                await ctx.RespondAsync("This emoji is not available to the bot");
                return;
            }

            if (state.ReactionMessage.Rules.ContainsKey(emoji))
            {
                await ctx.RespondAsync("This emoji is already assigned to a role");
                return;
            }

            state.ReactionMessage.Rules.Add(emoji, role);
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
        [Command(CommandNames.DeleteEmojiFromReactionMessage)]
        public async Task DeleteEmojiToReactionMessage(CommandContext ctx, DiscordEmoji emoji)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;
            
            SetupState state = _setupStates[ctx.Guild.Id];
            
            if (!state.IsInSetupMode && !state.IsInEditMode)
            {
                await ctx.RespondAsync("You need to have entered setup or edit mode to delete reactions from a message");
                return;
            }
            
            if (!state.ReactionMessage.Rules.ContainsKey(emoji))
            {
                await ctx.RespondAsync("This emoji isn't associated to a role");
                return;
            }

            state.ReactionMessage.Rules.Remove(emoji);
        }

        [RequireGuild]
        [Command(CommandNames.DeleteEmojiFromReactionMessage)]
        public async Task DeleteEmojiToReactionMessage(CommandContext ctx)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;
            
            await ctx.RespondAsync("You need to provide an emoji to delete from the message");
        }
        
        [RequireGuild]
        [Command(CommandNames.DeleteEmojiFromReactionMessage)]
        public async Task DeleteEmojiToReactionMessage(CommandContext ctx, params string[] rest)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;
            
            await ctx.RespondAsync("You need to provide an emoji to delete from the message");
        }

        [RequireGuild]
        [Command(CommandNames.PreviewReactionMessage)]
        public async Task PreviewReactionMessage(CommandContext ctx)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;
            
            SetupState state = _setupStates[ctx.Guild.Id];
            
            if (!state.IsInSetupMode && !state.IsInEditMode)
            {
                await ctx.RespondAsync("You need to have entered setup or edit mode before previewing the result");
                return;
            }
            
            if (state.ReactionMessage.Rules.Count == 0)
            {
                await ctx.RespondAsync("You need to add at least 1 reaction and role to finish the setup");
                return;
            }

            await ctx.RespondAsync(state.ReactionMessage.ToString());
        }
        
        [RequireGuild]
        [Command(CommandNames.FinishSetup)]
        public async Task FinishSetup(CommandContext ctx)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;
            
            SetupState state = _setupStates[ctx.Guild.Id];
            
            if (!state.IsInSetupMode && !state.IsInEditMode)
            {
                await ctx.RespondAsync("You need to have entered setup or edit mode before finishing it");
                return;
            }
            
            if (state.ReactionMessage.Rules.Count == 0)
            {
                await ctx.RespondAsync("You need to have at least 1 reaction and role to finish the setup / edit");
                return;
            }

            if (state.IsInEditMode)
                _reactionInteractions.ReactionMessages.Remove(state.OriginalReactionMessage);

            _reactionInteractions.ReactionMessages.Add(state.ReactionMessage);
            _reactionInteractions.SaveToFile();

            string rulesString = "";
            foreach (KeyValuePair<DiscordEmoji, DiscordRole> rule in state.ReactionMessage.Rules)
            {
                rulesString += rule.Key + " : " + rule.Value.Name + '\n';
                try
                {
                    await state.ReactionMessage.Message.CreateReactionAsync(rule.Key);
                }
                catch (Exception e)
                {
                    await ctx.RespondAsync(e.ToString());
                    throw;
                }
            }

            await ctx.RespondAsync(
                $"Setup / edit finished!\nMessage : {state.ReactionMessage.Message.JumpLink}\nRules :\n{rulesString}");

            state.ReactionMessage = new ReactionMessage();
            state.OriginalReactionMessage = new ReactionMessage();
            
            state.IsInSetupMode = false;
            state.IsInEditMode = false;
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
        
        [RequireGuild]
        [Command(CommandNames.CheckReactionMessage)]
        public async Task CheckReactionMessage(CommandContext ctx, string messageToUse)
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

            await ctx.TriggerTypingAsync();
            await _reactionInteractions.CheckAllRules(associatedReactionMessage);
            await ctx.RespondAsync("Done");
        }
    }
}