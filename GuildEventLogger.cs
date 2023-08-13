using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace RoboBot
{
    public class GuildEventLogger
    {
        private static readonly string LogChannelsLocalPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log-channels.json");
        
        private static readonly DiscordColor InfoColor = new DiscordColor(0, 100, 255);
        private static readonly DiscordColor WarningColor = new DiscordColor(200, 200, 0);
        private static readonly DiscordColor ErrorColor = new DiscordColor(175, 0, 0);
        
        private static DiscordClient _client;
        
        private Dictionary<ulong, DiscordChannel> _logChannels = new Dictionary<ulong, DiscordChannel>();

        private GuildEventLogger() { }

        public struct GuildLogInfoResult
        {
            public bool IsSet;
            public DiscordChannel LogChannel;
        }
        
        public static readonly GuildEventLogger Instance = new GuildEventLogger();

        public static void Initialize(ref DiscordClient client)
        {
            _client = client;
            Instance.LoadChannelsFromFile();
        }

        public void SetLogChannel(DiscordGuild guild, DiscordChannel channel)
        {
            if (_logChannels.ContainsKey(guild.Id))
                _logChannels.Remove(guild.Id);
            
            _logChannels.Add(guild.Id, channel);
            SaveChannelsToFile();
        }
        
        public void UnsetLogChannel(DiscordGuild guild)
        {
            if (!_logChannels.ContainsKey(guild.Id))
                return;
            
            _logChannels.Remove(guild.Id);
            SaveChannelsToFile();
        }

        public GuildLogInfoResult GetGuildLogInfo(DiscordGuild guild)
        {
            bool isSet = _logChannels.TryGetValue(guild.Id, out DiscordChannel channel);

            return new GuildLogInfoResult() { IsSet = isSet, LogChannel = channel };
        }

        public async Task NotifyModerator(DiscordGuild guild, string message)
        {
            ulong guildId = guild.Id;
            
            if (!_logChannels.ContainsKey(guildId))
                return;
            try
            {
                IMention modRole = new RoleMention(1008209009330896946);
                await _logChannels[guildId].SendMessageAsync(new DiscordMessageBuilder()
                    .WithAllowedMention(modRole)
                    .WithContent("<@&1008209009330896946>"));
                Task<DiscordMessage> sendTask = _logChannels[guildId].SendMessageAsync(new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Red)
                    .WithDescription(message))
                ;
                

                await sendTask;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{nameof(GuildEventLogger)}.{nameof(LogInfo)}: {e.Message}");
            }

        }

        public async Task LogInfo(DiscordGuild guild, string message)
        {
            ulong guildId = guild.Id;

            if (!_logChannels.ContainsKey(guildId))
                return;

            try
            {
                Task<DiscordMessage> sendTask = _logChannels[guildId].SendMessageAsync(new DiscordEmbedBuilder()
                    .WithDescription(message)
                    .WithColor(InfoColor));
                
                await sendTask;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{nameof(GuildEventLogger)}.{nameof(LogInfo)}: {e.Message}");
            }
        }
        
        public async Task LogWarning(DiscordGuild guild, string message)
        {
            ulong guildId = guild.Id;
            
            if (!_logChannels.ContainsKey(guildId))
                return;
            
            try
            {
                Task<DiscordMessage> sendTask = _logChannels[guildId].SendMessageAsync(new DiscordEmbedBuilder()
                    .WithDescription(message)
                    .WithColor(WarningColor));
                
                await sendTask;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{nameof(GuildEventLogger)}.{nameof(LogWarning)}: {e.Message}");
            }
        }
        
        public async Task LogError(DiscordGuild guild, string message)
        {
            ulong guildId = guild.Id;

            if (!_logChannels.ContainsKey(guildId))
                return;

            try
            {
                Task<DiscordMessage> sendTask = _logChannels[guildId].SendMessageAsync(new DiscordEmbedBuilder()
                    .WithDescription(message)
                    .WithColor(ErrorColor));
                
                await sendTask;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{nameof(GuildEventLogger)}.{nameof(LogError)}: {e.Message}");
            }
        }

        private void LoadChannelsFromFile()
        {
            _logChannels = new Dictionary<ulong, DiscordChannel>();
            if (!File.Exists(LogChannelsLocalPath))
                return;
            
            string json = File.ReadAllText(LogChannelsLocalPath);
            Dictionary<ulong, ulong> dictionaryFromFile = JsonConvert.DeserializeObject<Dictionary<ulong, ulong>>(json);

            foreach (KeyValuePair<ulong, ulong> guildAndChannelId in dictionaryFromFile)
            {
                if (!_client.Guilds.TryGetValue(guildAndChannelId.Key, out DiscordGuild guild) || !guild.Channels.TryGetValue(guildAndChannelId.Value, out DiscordChannel channel))
                    continue;

                _logChannels.Add(guildAndChannelId.Key, channel);
            }
        }

        private void SaveChannelsToFile()
        {
            Dictionary<ulong, ulong> dictionaryToSerialize = new Dictionary<ulong, ulong>();

            foreach (KeyValuePair<ulong, DiscordChannel> logChannel in _logChannels)
            {
                dictionaryToSerialize.Add(logChannel.Key, logChannel.Value.Id);
            }

            string json = JsonConvert.SerializeObject(dictionaryToSerialize);
            File.WriteAllText(LogChannelsLocalPath, json);
        }
    }
}