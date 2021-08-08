//#define NO_SRC

using System;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using SpeedrunComSharp;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using FFMpegCore;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;


namespace RoboBot
{

    internal class Program
    {
        public static string timeFormat = @"ss\.ff";
        public static string timeFormatWithMinutes = @"mm\:ss\.ff";
        public static string timeFormatWithHours = @"hh\:mm\:ss\.ff";
        public static DiscordClient discord;
        public static string gameId = "76ryx418";
        private static CommandsNextExtension commands;
        
        public static List<CommandContext> convertQueue = new List<CommandContext>();

        public static ReplayWorker replayEvents = new ReplayWorker();

        public static ReactionInteractions reactionInteractions;
        
#if NO_SRC
        public static SpeedrunComClient srcClient;
        public static Game srb2Game;
        public static Stats s;
#else
        public static SpeedrunComClient srcClient = new SpeedrunComClient(maxCacheElements: 0) { AccessToken = ConfigurationManager.AppSettings["SRC_APIKey"] };
        public static Game srb2Game = srcClient.Games.GetGame(gameId);
        public static Stats s = new Stats(ref srcClient);
#endif


        private static void Main(string[] args)
        {
            replayEvents.StartProcessing();
            replayEvents.Processed += ReplayProcessed;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private static void ReplayProcessed(object sender, ReplayEventArgs args)
        {

            switch (args.Status)
            {
                case ReplayStatus.BadDemo:
                    SendMessage("Bad Demo. Is your file a valid replay?");
                    return;
                case ReplayStatus.NoMap:
                    SendMessage("No map found. Did you forget to put the addon name?");
                    return;
                case ReplayStatus.UnhandledException:
                    SendMessage("i am stupid");
                    return;
                case ReplayStatus.Success:
                    Console.WriteLine("success");
                    break;
            }
            MediaPaths outputPaths = new MediaPaths();
                string imageCode = Path.GetRandomFileName();
                switch (convertQueue[0].Command.Name)
                {
                    case "reptogif":
                        outputPaths.filePath += "finishedgifs/";
                        outputPaths.urlPath += $"finishedgifs/{imageCode}.gif";
                        File.Move(args.OutputPath, $"{outputPaths.filePath}{imageCode}.gif");
                        break;
                    case "reptomp4":
                        outputPaths.filePath += "finishedmp4s/";
                        outputPaths.urlPath += $"finishedmp4s/{imageCode}.mp4";
                        File.Move(args.OutputPath, $"/var/www/html/gifs/{imageCode}.gif");
                        ConvertToMp4($"{outputPaths.filePath}{imageCode}.mp4", imageCode);
                        File.Delete($"/var/www/html/gifs/{imageCode}.gif");
                        break;
                }

                var finishedmedia = new DiscordMessageBuilder()
                    .WithContent(outputPaths.urlPath)
                    .WithReply(convertQueue[0].Message.Id, true)
                    .SendAsync(convertQueue[0].Channel);

                convertQueue.RemoveAt(0);
                if (convertQueue.Any())
                {
                    var nextQueue = new DiscordMessageBuilder()
                        .WithContent("This replay is next.")
                        .WithReply(convertQueue[0].Message.Id)
                        .SendAsync(convertQueue[0].Channel);
                }
            
        }

        private static void SendMessage(string errorMessage)
        {
            new DiscordMessageBuilder()
                .WithContent(errorMessage)
                .WithReply(convertQueue[0].Message.Id)
                .SendAsync(convertQueue[0].Channel);
            
            convertQueue.RemoveAt(0);
        }

        private static void ConvertToMp4(string output, string imageCode)
        {
            FFMpegArguments
                .FromFileInput($"/var/www/html/gifs/{imageCode}.gif")
                .OutputToFile(output, true, options => options
                    .UsingMultithreading(true)
                    .WithVideoCodec("h264")
                    .WithFastStart()
                    .ForcePixelFormat("yuv420p")
                    .ForceFormat("mp4"))
                .ProcessSynchronously();
                
        }
        
        private static async Task MainAsync(string[] args)
        {

            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = ConfigurationManager.AppSettings["APIKey"],
                TokenType = TokenType.Bot
            });
            discord.UseInteractivity(new InteractivityConfiguration()
            {
                PollBehaviour = PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromSeconds(30)
            });

            commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { "!" },
                EnableDefaultHelp = false
            });

            discord.GuildDownloadCompleted += (sender, eventArgs) =>
            {
                return Task.Run(() =>
                {
                    Console.WriteLine("Initializing ReactionInteractions...");
                    reactionInteractions = new ReactionInteractions(discord);
                    Console.WriteLine("ReactionInteractions Initialized!");
                });
            };

            commands.RegisterCommands<Commands>();
            commands.RegisterCommands<ReactionSetupCommands>();

            DiscordActivity activity = new DiscordActivity("greenflower", ActivityType.ListeningTo);

            await discord.ConnectAsync(activity);
            await Task.Delay(-1);
        }
    }
}