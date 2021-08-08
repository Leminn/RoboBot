using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace RoboBot
{
    public class ReactionMessage
    {
        public DiscordMessage Message;
        public Dictionary<DiscordEmoji, DiscordRole> Rules = new Dictionary<DiscordEmoji, DiscordRole>();

        public ReactionMessage() { }

        public ReactionMessage(DiscordMessage message, Dictionary<DiscordEmoji, DiscordRole> rules)
        {
            Message = message;
            Rules = rules;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("");
            sb.Append("\tMessage : " + Message.JumpLink);
            sb.Append("\n\tRules :\n");
            foreach (KeyValuePair<DiscordEmoji, DiscordRole> rule in Rules)
            {
                sb.Append($"\t\t{rule.Key} : {rule.Value.Mention}\n");
            }

            return sb.ToString();
        }
    }

    public class SerializedReactionMessage
    {
        public ulong GuildId { get; }
        public ulong ChannelId { get; }
        public ulong MessageId { get; }
        
        public Dictionary<string, ulong> Rules { get; }

        public SerializedReactionMessage(ReactionMessage reactionMessage)
        {
            GuildId = (ulong)reactionMessage.Message.Channel.GuildId;
            ChannelId = reactionMessage.Message.ChannelId;
            MessageId = reactionMessage.Message.Id;

            Rules = new Dictionary<string, ulong>();

            foreach (KeyValuePair<DiscordEmoji, DiscordRole> rule in reactionMessage.Rules)
            {
                Rules.Add(rule.Key.GetDiscordName(), rule.Value.Id);
            }
        }

        [JsonConstructor]
        public SerializedReactionMessage(ulong guildId, ulong channelId, ulong messageId,
            Dictionary<string, ulong> rules)
        {
            GuildId = guildId;
            ChannelId = channelId;
            MessageId = messageId;
            Rules = rules;
        }
        
        public ReactionMessage ToReactionMessage()
        {
            if (!Program.discord.Guilds.TryGetValue(GuildId, out DiscordGuild guild))
            {
                Console.WriteLine("Could not get the guild of this message, perhaps the bot have been removed from it? (Skipping ReactionMessage)");
                return null;
            }

            if (!guild.Channels.TryGetValue(ChannelId, out DiscordChannel channel))
            {
                Console.WriteLine("Could not get the channel of this message, perhaps the bot have not access to it anymore? (Skipping ReactionMessage)");
                return null;
            }

            DiscordMessage message;
            try
            {
                message = channel.GetMessageAsync(MessageId).Result;
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not get the channel of this message, perhaps it has been removed? (Skipping ReactionMessage)");
                Console.WriteLine(e);
                return null;
            }

            Dictionary<DiscordEmoji, DiscordRole> reactionRules = new Dictionary<DiscordEmoji, DiscordRole>();

            foreach (KeyValuePair<string, ulong> rule in Rules)
            {
                if(!DiscordEmoji.TryFromName(Program.discord, rule.Key, out DiscordEmoji emoji))
                {
                    message.RespondAsync($"Could not load emoji {rule.Key}. Has it been removed or renamed? (Skipping this rule)");
                    continue;
                }
                
                DiscordRole role = guild.Roles[rule.Value];
                reactionRules.Add(emoji, role);
            }

            return new ReactionMessage(message, reactionRules);
        }
    }
}