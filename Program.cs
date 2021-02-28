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
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.Configuration;
using SpeedrunComSharp;

namespace RoboBot
{
    class Program
    {
        public static string programDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public static string timeFormat = @"mm\:ss\.ff";
        public static string timeFormatWithHours = @"hh\:mm\:ss\.ff";
        public static DiscordClient discord;
        public static string gameId = "76ryx418";
        static CommandsNextExtension commands;
        
        public static Game srb2Game;
    
        public static SpeedrunComClient srcClient = new SpeedrunComClient(maxCacheElements:0) { AccessToken = ConfigurationManager.AppSettings["SRC_APIKey"] };

        static void Main(string[] args)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;
            srb2Game = srcClient.Games.GetGame(gameId);
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        
        static async Task MainAsync(string[] args)
        {
            
            discord = new DiscordClient(new DiscordConfiguration 
            { 
                Token = ConfigurationManager.AppSettings["APIKey"],
                TokenType = TokenType.Bot
            }); 

        

            commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { "!" },
                EnableDefaultHelp = false 
            });

            commands.RegisterCommands<MyCommands>();


            await discord.ConnectAsync();
            await Task.Delay(-1);
            
        }

    }
}
