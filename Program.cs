﻿using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using SpeedrunComSharp;
using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RoboBot
{
    internal class Program
    {
        public static string programDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public static string timeFormat = @"mm\:ss\.ff";
        public static string timeFormatWithHours = @"hh\:mm\:ss\.ff";
        public static DiscordClient discord;
        public static string gameId = "76ryx418";
        private static CommandsNextExtension commands;

        public static Game srb2Game;

        public static SpeedrunComClient srcClient = new SpeedrunComClient(maxCacheElements: 0) { AccessToken = ConfigurationManager.AppSettings["SRC_APIKey"] };

        public static Stats s = new Stats(ref srcClient);

        private static void Main(string[] args)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;
            srb2Game = srcClient.Games.GetGame(gameId);
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
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
         //   DiscordEmoji pog = DiscordEmoji.FromGuildEmote(discord, 805598061346291722);
            DiscordActivity activity = new DiscordActivity("GFZ1", ActivityType.ListeningTo);

            await discord.ConnectAsync(activity);
            await Task.Delay(-1);
        }
    }
}