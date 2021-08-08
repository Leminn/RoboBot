using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Newtonsoft.Json;

namespace RoboBot
{
    public class ReactionInteractions
    {
        private readonly string ReactionMessagesLocalPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reaction-messages.json");

        private DiscordClient _client;

        public List<ReactionMessage> ReactionMessages;

        public ReactionInteractions(DiscordClient client)
        {
            _client = client;
            _client.MessageReactionAdded += DiscordOnMessageReactionAdded;
            _client.MessageReactionRemoved += DiscordOnMessageReactionRemoved;

            LoadReactionMessages();
            CheckAllReactionMessages().Wait();
        }
        
        private Task DiscordOnMessageReactionAdded(DiscordClient sender, MessageReactionAddEventArgs e)
        {
            return Task.Run(() =>
            {
                ReactionMessage associatedMessage = ReactionMessages.FirstOrDefault(x => x.Message.Equals(e.Message));

                if (associatedMessage == null)
                {
                    e.Message.RespondAsync("nothing associated, do nothing");
                    return;
                }

                if (e.User.Equals(Program.discord.CurrentUser))
                    return;

                if (!associatedMessage.Rules.TryGetValue(e.Emoji, out DiscordRole roleToGrant))
                {
                    e.Message.RespondAsync("sadge");
                    return;
                }

                ((DiscordMember)e.User).GrantRoleAsync(roleToGrant);
                e.Message.RespondAsync($"User was granted {e.Emoji}");
            });
        }
        
        private Task DiscordOnMessageReactionRemoved(DiscordClient sender, MessageReactionRemoveEventArgs e)
        {
            return Task.Run(() =>
            {
                ReactionMessage associatedMessage = ReactionMessages.FirstOrDefault(x => x.Message.Equals(e.Message));

                if (associatedMessage == null)
                {
                    e.Message.RespondAsync("nothing associated, do nothing");
                    return;
                }

                if (e.User.Equals(Program.discord.CurrentUser))
                    return;

                if (!associatedMessage.Rules.TryGetValue(e.Emoji, out DiscordRole roleToGrant))
                {
                    e.Message.RespondAsync("sadge");
                    return;
                }

                ((DiscordMember)e.User).RevokeRoleAsync(roleToGrant);
                e.Message.RespondAsync($"User was revoked {e.Emoji}");
            });
        }


        public void SaveToFile()
        {
            List<SerializedReactionMessage> listToSerialize = new List<SerializedReactionMessage>();

            foreach (ReactionMessage reactionMessage in ReactionMessages)
            {
                listToSerialize.Add(new SerializedReactionMessage(reactionMessage));
            }

            string json = JsonConvert.SerializeObject(listToSerialize, Formatting.Indented);
            File.WriteAllText(ReactionMessagesLocalPath, json);
        }
        
        public void LoadReactionMessages()
        {
            ReactionMessages = new List<ReactionMessage>();
            if (!File.Exists(ReactionMessagesLocalPath))
            {
                return;
            }
            
            string json = File.ReadAllText(ReactionMessagesLocalPath);
            List<SerializedReactionMessage> serializedReactionMessages =
                JsonConvert.DeserializeObject<List<SerializedReactionMessage>>(json);

            foreach (SerializedReactionMessage serializedReactionMessage in serializedReactionMessages)
            {
                ReactionMessage reactionMessage = serializedReactionMessage.ToReactionMessage();

                if (reactionMessage != null)
                    ReactionMessages.Add(reactionMessage);
            }
        }

        public async Task CheckAllReactionMessages()
        {
            foreach (ReactionMessage reactionMessage in ReactionMessages)
            {
                await CheckAllRules(reactionMessage);
            }
        }

        public async Task CheckAllRules(ReactionMessage reactionMessage)
        {
            IReadOnlyCollection<DiscordMember> guildMembers = await reactionMessage.Message.Channel.Guild.GetAllMembersAsync();
            
            foreach (KeyValuePair<DiscordEmoji, DiscordRole> rule in reactionMessage.Rules)
            {
                IEnumerable<DiscordUser> reactedMembers = reactionMessage.Message.GetReactionsAsync(rule.Key, 500).Result;

                foreach (DiscordMember member in guildMembers)
                {
                    if (member.Equals(Program.discord.CurrentUser))
                        continue;
                    
                    bool reacted = false;
                    bool hasRole = false;

                    if (reactedMembers.FirstOrDefault(x => x.Equals(member)) != null)
                        reacted = true;

                    if (member.Roles.Contains(rule.Value))
                        hasRole = true;
                        
                    if ((reacted && hasRole) || (!reacted && !hasRole))
                        continue;

                    if (reacted)
                    {
                        Console.WriteLine(member.Username + " grant " + rule.Value.Name);
                        await member.GrantRoleAsync(rule.Value);
                        continue;
                    }

                    Console.WriteLine(member.Username + " revoke " + rule.Value.Name);
                    await member.RevokeRoleAsync(rule.Value);
                }
            }
        }
    }
}