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
        private bool IsSettingUpReactionMessage = false;

        private ReactionMessage reactionMessage = new ReactionMessage();

        private const Permissions RequiredPermissions = Permissions.Administrator;

        [RequireGuild]
        [Command("reactsetup")]
        public async Task ReactionMessageSetup(CommandContext ctx, string messageUrl)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;

            if (IsSettingUpReactionMessage)
            {
                await ctx.RespondAsync(
                    "The bot is already setting up a reaction message, finish the previous one and try again");
            }

            DiscordMessage message = await CommandHelpers.GetMessageFromUrl(ctx, messageUrl);
            
            if (message is null)
                return;

            if (Program.reactionInteractions.ReactionMessages.FirstOrDefault(x => x.Message.Equals(message)) != null)
            {
                await ctx.RespondAsync("This message already got reaction interactions setup");
                return;
            }

            reactionMessage.Message = message;

            await ctx.RespondAsync("Got the message to use, continue the setup with !reactadd (emoji) (mention to role)");
            
            IsSettingUpReactionMessage = true;
        }
        
        [RequireGuild]
        [Command("reactsetup")]
        public async Task ReactionMessageSetup(CommandContext ctx)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;
            
            await ctx.RespondAsync("You need to provide a message id to start the setup");
        }
        
        [RequireGuild]
        [Command("reactadd")]
        public async Task ReactionAdd(CommandContext ctx, DiscordEmoji emoji, DiscordRole role)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;
            
            if (!IsSettingUpReactionMessage)
            {
                await ctx.RespondAsync("You need to have entered the setup to add reactions");
                return;
            }
            
            reactionMessage.Rules.Add(emoji, role);
            await ctx.RespondAsync($"Added {emoji} as the role \"{role.Name}\" to {reactionMessage.Message.JumpLink}");
        }
        
        [RequireGuild]
        [Command("reactadd")]
        public async Task ReactionAdd(CommandContext ctx)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;
            
            await ctx.RespondAsync("You need to provide an emoji and mention the role to attribute it to");
        }
        
        [RequireGuild]
        [Command("reactadd")]
        public async Task ReactionAdd(CommandContext ctx, params string[] rest)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;
            
            await ctx.RespondAsync("You need to provide an emoji and mention the role to attribute it to");
        }
        
        [RequireGuild]
        [Command("reactfinish")]
        public async Task ReactFinish(CommandContext ctx)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;
            
            if (reactionMessage.Rules.Count == 0)
            {
                await ctx.RespondAsync("You need to add at least 1 reaction and role to finish the setup");
            }

            Program.reactionInteractions.ReactionMessages.Add(reactionMessage);
            Program.reactionInteractions.SaveToFile();

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
                $"Setup finished!\nMessage : {reactionMessage.Message.JumpLink}\nRules :\n{rulesString}");

            IsSettingUpReactionMessage = false;
        }
        
        [RequireGuild]
        [Command("reactlist")]
        public async Task ListReactionMessages(CommandContext ctx)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;
            
            StringBuilder sb = new StringBuilder("Reaction Messages :");
            foreach (ReactionMessage reactionMessage in Program.reactionInteractions.ReactionMessages.Where(x => x.Message.Channel.GuildId == ctx.Channel.GuildId))
            {
                sb.Append("\n\n" + reactionMessage.ToString() + '\n');
            }

            await ctx.RespondAsync(sb.ToString());
        }
        
        [RequireGuild]
        [Command("reactdelete")]
        public async Task DeleteReactionMessage(CommandContext ctx, string messageToUse)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;

            DiscordMessage message = await CommandHelpers.GetMessageFromUrl(ctx, messageToUse);

            if (message is null)
                return;

            ReactionMessage associatedReactionMessage =
                Program.reactionInteractions.ReactionMessages.FirstOrDefault(x => x.Message.Equals(message));
            
            if (associatedReactionMessage == null)
            {
                await ctx.RespondAsync("Couldn't find the ReactionMessage associated with this Discord message");
                return;
            }

            Program.reactionInteractions.ReactionMessages.Remove(associatedReactionMessage);
            await ctx.RespondAsync("Succesfully removed the corresponding ReactionMessage");
            
            Program.reactionInteractions.SaveToFile();
        }

        [RequireGuild]
        [Command("reactdelete")]
        public async Task DeleteReactionMessage(CommandContext ctx)
        {
            if (!await CommandHelpers.CheckPermissions(ctx, RequiredPermissions))
                return;

            await ctx.RespondAsync("You need to provide the original message id you want to delete");
        }
    }
}