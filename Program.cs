using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks; 
using System.Configuration;
using System.Collections.Generic;
using System.Net;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.Configuration;
using SpeedrunComSharp;

namespace RoboBot
{
    public static class SRB2Enums
    {
        public static Dictionary<string, string> levelsID = new Dictionary<string, string>()
        {
            {"GF1", "ywejpqld"},
            {"GF2", "69zq08x9"},
            {"GF3", "r9gj87j9"},
            {"TH1", "o9xz0x3w"}
        };
        public static Dictionary<string, string> categoriesID = new Dictionary<string, string>()
        {
            {"sonic", "xd1g1j4d"},
            {"tails", "zd3wvjvk"},
            {"knuckles", "9kvoq882"},
            {"amy", "rkll95nk"},
            {"fang", "ndx9e3rd"},
            {"metal sonic", "w20g8q5k"}
        };
    }
    class Program
    {
        public static string timeFormat = @"mm\:ss\.ff";
        static DiscordClient discord;
        static CommandsNextModule commands;
        
        public static Game srb2Game;
    
        public static SpeedrunComClient srcClient = new SpeedrunComClient() { AccessToken = ConfigurationManager.AppSettings["SRC_APIKey"] };

        static void Main(string[] args)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            srb2Game = srcClient.Games.GetGame("76ryx418");
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            
            discord = new DiscordClient(new DiscordConfiguration 
            { 

            Token = ConfigurationManager.AppSettings["APIKey"],
            TokenType = TokenType.Bot,
            UseInternalLogHandler = true,
            LogLevel = LogLevel.Debug
            }); 

            discord.MessageCreated += async e =>
            {
                if (e.Message.Content.ToLower().StartsWith("ping"))
                    await e.Message.RespondAsync("pong!");
                else if(e.Message.Content.ToLower().StartsWith("peas"))
                    await e.Message.RespondAsync(":duck:");
            };

            commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefix = ";;"
            });

            commands.RegisterCommands<MyCommands>();


            await discord.ConnectAsync();
            await Task.Delay(-1);

            
        }

    }
}
