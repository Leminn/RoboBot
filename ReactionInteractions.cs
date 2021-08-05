using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;

namespace RoboBot
{
    public class ReactionInteractions
    {
        private readonly string ReactionMessagesLocalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reaction-messages.json");
        
        private DiscordClient _client;

        public List<ReactionMessage> ReactionMessages;
        public ReactionInteractions(DiscordClient client)
        {
            _client = client;
            _client.MessageReactionAdded += DiscordOnMessageReactionAdded;
            _client.MessageReactionRemoved += DiscordOnMessageReactionRemoved;

            LoadReactionMessages();
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
                
                if (e.User.IsBot)
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
                
                if (e.User.IsBot)
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
            List<SerializedReactionMessage> serializedReactionMessages = JsonConvert.DeserializeObject<List<SerializedReactionMessage>>(json);

            foreach (SerializedReactionMessage serializedReactionMessage in serializedReactionMessages)
            {
                ReactionMessages.Add(serializedReactionMessage.ToReactionMessage());
            }
        }
    }
}