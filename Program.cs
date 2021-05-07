using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using SpeedrunComSharp;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RoboBot
{
    internal class Program
    {
        public static FileSystemWatcher txtWatcher = new FileSystemWatcher("/var/www/html/gifs/");

        public static string timeFormat = @"ss\.ff";
        public static string timeFormatWithMinutes = @"mm\:ss\.ff";
        public static string timeFormatWithHours = @"hh\:mm\:ss\.ff";
        public static DiscordClient discord;
        public static string gameId = "76ryx418";
        private static CommandsNextExtension commands;

        public static List<CommandContext> convertQueue = new List<CommandContext>();
        
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
            txtWatcher.EnableRaisingEvents = true;
            txtWatcher.Filters.Add("status.txt"); 
            txtWatcher.Created += OnCreated;
            txtWatcher.Changed += OnCreated;
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

            commands.RegisterCommands<Commands>();

            DiscordActivity activity = new DiscordActivity("greenflower", ActivityType.ListeningTo);

            await discord.ConnectAsync(activity);
            await Task.Delay(-1);
        }

        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            FileInfo filestuff = new FileInfo(e.FullPath);
            switch (filestuff.Extension)
            {
                case ".txt":
                    string txtContents = File.ReadAllText(e.FullPath);
                    if (txtContents != "ok")
                    {
                        var msg = new DiscordMessageBuilder()
                            .WithContent(txtContents)
                            .WithReply(convertQueue[0].Message.Id)
                            .SendAsync(convertQueue[0].Channel);
                    }
                    else
                    {
                        string filePath = $"/var/www/html/finishedgifs/";
                        string imageCode = Path.GetRandomFileName();
                        File.Move("/var/www/html/gifs/torename.gif", $"{filePath}{imageCode}.gif");
                        var msg = new DiscordMessageBuilder()
                            .WithContent($"https://roborecords.org/finishedgifs/{imageCode}.gif")
                            .WithReply(convertQueue[0].Message.Id, true)
                            .SendAsync(convertQueue[0].Channel);
                    }
                    convertQueue.RemoveAt(0);
                    if (convertQueue.Any())
                    {
                        var msg = new DiscordMessageBuilder()
                            .WithContent("This replay is next.")
                            .WithReply(convertQueue[0].Message.Id)
                            .SendAsync(convertQueue[0].Channel);
                    }
                    File.Delete(e.FullPath);
                    break;
            }
        }
    }
}