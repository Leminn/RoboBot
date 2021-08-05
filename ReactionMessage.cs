using System.Collections.Generic;
using System.Linq;
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
            DiscordGuild guild = Program.discord.GetGuildAsync(GuildId).Result;
            DiscordMessage message = guild.GetChannel(ChannelId)
                .GetMessageAsync(MessageId).Result;

            Dictionary<DiscordEmoji, DiscordRole> reactionRules = new Dictionary<DiscordEmoji, DiscordRole>();

            foreach (KeyValuePair<string, ulong> rule in Rules)
            {
                DiscordEmoji emoji = DiscordEmoji.FromName(Program.discord, rule.Key);
                DiscordRole role = guild.Roles[rule.Value];
                reactionRules.Add(emoji, role);
            }

            return new ReactionMessage(message, reactionRules);
        }
    }
}