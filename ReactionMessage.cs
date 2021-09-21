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
    
    public class SerializedRule
    {
        public bool IsGuildEmoji;
        
        public string EmojiNameOrId;

        public ulong RoleId;

        public SerializedRule(bool isGuildEmoji, string emojiNameOrId, ulong roleId)
        {
            IsGuildEmoji = isGuildEmoji;
            EmojiNameOrId = emojiNameOrId;
            RoleId = roleId;
        }
    }

    public class SerializedReactionMessage
    {
        [JsonIgnore] private static DiscordClient _client;
        public ulong GuildId { get; }
        public ulong ChannelId { get; }
        public ulong MessageId { get; }
        
        public List<SerializedRule> Rules { get; }
        
        public static void SetDiscordClient(DiscordClient client) => _client = client;

        public SerializedReactionMessage(ReactionMessage reactionMessage)
        {
            GuildId = (ulong)reactionMessage.Message.Channel.GuildId;
            ChannelId = reactionMessage.Message.ChannelId;
            MessageId = reactionMessage.Message.Id;

            Rules = new List<SerializedRule>();

            foreach (KeyValuePair<DiscordEmoji, DiscordRole> rule in reactionMessage.Rules)
            {
                if (rule.Key.Id != 0)
                {
                    Rules.Add(new SerializedRule(true, rule.Key.Id.ToString(), rule.Value.Id));
                    continue;
                }
                
                Rules.Add(new SerializedRule(false, rule.Key.GetDiscordName(), rule.Value.Id));
            }
        }

        [JsonConstructor]
        public SerializedReactionMessage(ulong guildId, ulong channelId, ulong messageId,
            List<SerializedRule> rules)
        {
            GuildId = guildId;
            ChannelId = channelId;
            MessageId = messageId;
            Rules = rules;
        }
        
        public ReactionMessage ToReactionMessage()
        {
            if (!_client.Guilds.TryGetValue(GuildId, out DiscordGuild guild))
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

            foreach (SerializedRule rule in Rules)
            {
                DiscordEmoji emoji;

                if (rule.IsGuildEmoji)
                {
                    ulong id = ulong.Parse(rule.EmojiNameOrId);
                    
                    if(!DiscordEmoji.TryFromGuildEmote(_client, id, out emoji))
                    {
                        GuildEventLogger.Instance.LogWarning(guild, $"Could not load emoji {rule.EmojiNameOrId}. Has it been removed or renamed? (Skipping this rule)");
                        continue;
                    }
                }
                else
                {
                    if(!DiscordEmoji.TryFromName(_client, rule.EmojiNameOrId, out emoji))
                    {
                        GuildEventLogger.Instance.LogWarning(guild, $"Could not load emoji {rule.EmojiNameOrId}. Has it been removed or renamed? (Skipping this rule)");
                        continue;
                    }
                }
                
                
                if(!guild.Roles.TryGetValue(rule.RoleId, out DiscordRole role))
                {
                    GuildEventLogger.Instance.LogWarning(guild, $"Could not load role ID {rule.RoleId}. Has it been removed? (Skipping this rule)");
                    continue;
                }
                
                reactionRules.Add(emoji, role);
            }

            return new ReactionMessage(message, reactionRules);
        }
    }
}