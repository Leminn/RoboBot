using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace RoboBot
{
    public class ReactionSetupCommands : BaseCommandModule
    {
        private bool IsSettingUpReactionMessage = false;

        private ReactionInteractions _reactionInteractions = Program.reactionInteractions;
        
        public ReactionMessage reactionMessage = new ReactionMessage();
        
        [Command("reactsetup")]
        public async Task ReactionMessageSetup(CommandContext ctx, string messageToUse)
        {
            if (IsSettingUpReactionMessage)
            {
                await ctx.RespondAsync(
                    "The bot is already setting up a reaction message, finish the previous one and try again");
            }
            
            if (!Uri.TryCreate(messageToUse, UriKind.Absolute, out Uri messageUri))
            {
                await ctx.RespondAsync("Bad message link, try again");
                return;
            }
            //ulong channelId = messageUri.Segments.First(x => x.)
            //ctx.Guild.GetChannel()
            string[] urlParts = messageUri.PathAndQuery.Remove(0, 1).Split('/');
            if (urlParts.Length != 4)
            {
                await ctx.RespondAsync("Bad message link, try again");
                return;
            }
            
            if(!ulong.TryParse(urlParts[2], out ulong channelId))
            {
                await ctx.RespondAsync("Bad message link, try again");
                return;
            }
            
            if(!ulong.TryParse(urlParts[3], out ulong messageId))
            {
                await ctx.RespondAsync("Bad message link, try again");
                return;
            }
            
            DiscordMessage message;

            try
            {
                message = await ctx.Guild.Channels[channelId].GetMessageAsync(messageId);
            }
            catch (Exception e)
            {
                await ctx.RespondAsync(e.ToString());
                throw;
            }

            if (message is null)
            {
                await ctx.RespondAsync("Could not get the message, make sure it is in the same server and that the bot has access to it");
                return;
            }

            if (_reactionInteractions.ReactionMessages.FirstOrDefault(x => x.Message.Equals(message)) != null)
            {
                await ctx.RespondAsync("This message already got reaction interactions setup");
                return;
            }

            reactionMessage.Message = message;

            await ctx.RespondAsync("Got the message to use, continue the setup with !reactadd (emoji) (mention to role)");
            
            IsSettingUpReactionMessage = true;
        }

        [Command("reactsetup")]
        public async Task ReactionBruh(CommandContext ctx)
        {
            await ctx.RespondAsync("You need to provide a message id to start the setup");
        }

        [Command("reactadd")]
        public async Task ReactionAdd(CommandContext ctx, DiscordEmoji emoji, DiscordRole role)
        {
            if (!IsSettingUpReactionMessage)
            {
                await ctx.RespondAsync("You need to have entered the setup to add reactions");
                return;
            }
            
            reactionMessage.Rules.Add(emoji, role);
            await ctx.RespondAsync($"Added {emoji} as the role \"{role.Name}\" to {reactionMessage.Message.JumpLink}");
        }
        
        [Command("reactadd")]
        public async Task ReactionAddBruh(CommandContext ctx)
        {
            await ctx.RespondAsync("You need to provide an emoji and mention the role to attribute it to");
        }
        
        [Command("reactadd")]
        public async Task ReactionAddBruhMoment(CommandContext ctx, params string[] rest)
        {
            await ctx.RespondAsync("You need to provide an emoji and mention the role to attribute it to");
        }

        [Command("reactfinish")]
        public async Task ReactFinish(CommandContext ctx)
        {
            if (reactionMessage.Rules.Count == 0)
            {
                await ctx.RespondAsync("You need to add at least 1 reaction and role to finish the setup");
            }

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
                $"Setup finished!\nMessage : {reactionMessage.Message.JumpLink}\nRules :\n{rulesString}");

            IsSettingUpReactionMessage = false;
        }

        [Command("reactlist")]
        public async Task ListReactionMessages(CommandContext ctx)
        {
            //TODO
        }
        
        [Command("reactdelete")]
        public async Task DeleteReactionMessage(CommandContext ctx)
        {
            //TODO
        }
    }
}