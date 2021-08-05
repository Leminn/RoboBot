using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace RoboBot
{
    public class ReactionMessage
    {
        public DiscordMessage Message;
        public Dictionary<DiscordEmoji, DiscordRole> Rules = new Dictionary<DiscordEmoji, DiscordRole>();

        public ReactionMessage() { }
        
        public ReactionMessage(DiscordMessage message)
        {
            Message = message;
        }
        
        public ReactionMessage(DiscordMessage message, Dictionary<DiscordEmoji, DiscordRole> rules)
        {
            Message = message;
            Rules = rules;
        }
    }
}