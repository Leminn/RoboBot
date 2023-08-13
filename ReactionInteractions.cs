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
        private readonly ulong ExpertPendingRoleID = 1140285898743881728;
        private readonly ulong MasterPendingRoleID = 1140288682637664276;
        private readonly ulong ModeratorRoleID = 1008209009330896946;
        private readonly string ReactionMessagesLocalPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reaction-messages.json");

        private DiscordClient _client;

        public List<ReactionMessage> ReactionMessages;

        public ReactionInteractions(DiscordClient client)
        {
            _client = client;
            
            SerializedReactionMessage.SetDiscordClient(_client);
            ReactionSetupCommands.SetReactionInteractions(this);

            foreach (ulong guildId in client.Guilds.Keys)
            {
                ReactionSetupCommands.CreateGuildState(guildId);
            }

            _client.GuildCreated += (o, e) => 
                Task.Run(() => ReactionSetupCommands.CreateGuildState(e.Guild.Id)); 
            _client.GuildDeleted += (o, e) => 
                Task.Run(() => ReactionSetupCommands.RemoveGuildState(e.Guild.Id));

            LoadReactionMessages();
            CheckAllReactionMessages().Wait();
            
            _client.MessageReactionAdded += DiscordOnMessageReactionAdded;
            _client.MessageReactionRemoved += DiscordOnMessageReactionRemoved;
        }
        
        private Task DiscordOnMessageReactionAdded(DiscordClient sender, MessageReactionAddEventArgs e)
        {
            return Task.Run(() =>
            {
                ReactionMessage associatedMessage = ReactionMessages.FirstOrDefault(x => x.Message.Equals(e.Message));

                if (associatedMessage == null || e.User.Equals(_client.CurrentUser))
                    return;

                if (!associatedMessage.Rules.TryGetValue(e.Emoji, out DiscordRole roleToGrant))
                    return;

                DiscordMember member = ((DiscordMember)e.User);
                member.GrantRoleAsync(roleToGrant).Wait();
                GuildEventLogger.Instance
                    .LogInfo(e.Guild, $"{member.Mention} was granted the role {roleToGrant.Mention} by reacting to [this message]({associatedMessage.Message.JumpLink})").Wait();
                if (roleToGrant.Id == ExpertPendingRoleID)
                {
                    GuildEventLogger.Instance
                        .NotifyModerator(e.Guild,
                            $"{member.Mention} has requested a Expert Role review. <@&{ModeratorRoleID}>");
                }
                else if (roleToGrant.Id == MasterPendingRoleID)
                {
                    GuildEventLogger.Instance
                        .NotifyModerator(e.Guild,
                            $"{member.Mention} has requested an Master Role review. <@&{ModeratorRoleID}>");
                }
            });
        }
        
        private Task DiscordOnMessageReactionRemoved(DiscordClient sender, MessageReactionRemoveEventArgs e)
        {
            return Task.Run(() =>
            {
                ReactionMessage associatedMessage = ReactionMessages.FirstOrDefault(x => x.Message.Equals(e.Message));

                if (associatedMessage == null || e.User.Equals(_client.CurrentUser))
                    return;

                if (!associatedMessage.Rules.TryGetValue(e.Emoji, out DiscordRole roleToGrant))
                    return;

                DiscordMember member = ((DiscordMember)e.User);
                member.RevokeRoleAsync(roleToGrant).Wait();
                GuildEventLogger.Instance
                    .LogInfo(e.Guild, $"{member.Mention} was revoked the role {roleToGrant.Mention} by reacting to [this message]({associatedMessage.Message.JumpLink})").Wait();
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
                try
                {
                    await CheckAllRules(reactionMessage);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
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
                    if (member.Equals(_client.CurrentUser))
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