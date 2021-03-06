using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using SpeedrunComSharp;
using System;
using System.IO;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RoboBot
{
    internal class Program
    {
        public static string programDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public static DirectoryInfo dinfo = new DirectoryInfo(programDirectory);

        public static string homeFolder = dinfo.Parent.Parent.FullName; // /root/
        public static FileSystemWatcher gifWatcher = new FileSystemWatcher("/var/www/html/gifs/");

        public static string timeFormat = @"ss\.ff";
        public static string timeFormatWithMinutes = @"mm\:ss\.ff";
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
            gifWatcher.EnableRaisingEvents = true;
            gifWatcher.Created += OnCreated;
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
            DiscordActivity activity = new DiscordActivity("greenflower", ActivityType.Playing);

            await discord.ConnectAsync(activity);
            await Task.Delay(-1);
        }

        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            FileInfo filestuff = new FileInfo(e.FullPath);
            if (filestuff.Extension == ".gif")
            {
                int i = 0;
                string filePath = "/var/www/html/finishedgifs/";
                while (File.Exists(e.FullPath))
                {
                    if (!File.Exists($"{filePath}{i}.gif")) { System.IO.File.Move(e.FullPath, $"{filePath}{i}.gif"); }
                    else { i++; }
                }
                MyCommands.loool.SendMessageAsync($"http://77.68.95.193/finishedgifs/{i}.gif");
            }
        }
    }
}