//#define NO_SRC

using System;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using SpeedrunComSharp;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using FFMpegCore;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
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
        private static SlashCommandsExtension slashCommands;

        public static List<CommandContext> convertQueue = new List<CommandContext>();

        public static ReplayWorker replayEvents = new ReplayWorker();

        public static ReactionInteractions reactionInteractions;
        
        public static JoinRolesInteractions joinRolesInteractions;

        private ulong guildId;
#if NO_SRC
        public static SpeedrunComClient srcClient;
        public static Game srb2Game;
        public static Stats s;
#else
        public static SpeedrunComClient srcClient = new SpeedrunComClient(maxCacheElements: 0)
            { AccessToken = ConfigurationManager.AppSettings["SRC_APIKey"] };

        public static Game srb2Game = srcClient.Games.GetGame(gameId);
        public static Stats s = new Stats(ref srcClient);
#endif

        private static Timer leminMentionTimer = new Timer(86_400_000) { AutoReset = true, Enabled = true };

        private static void Main(string[] args)
        {
            replayEvents.StartProcessing();
            replayEvents.Processed += ReplayProcessed;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private static void LeminMentionTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            foreach (DiscordGuild guild in discord.Guilds.Values)
            {
                if (guild.Name == "the gamer place")
                {
                    foreach (DiscordChannel channel in guild.Channels.Values)
                    {
                        if (channel.Name == "ps3")
                        {
                            DiscordMember member = guild.GetMemberAsync(111175736701779968).Result;
                            channel.SendMessageAsync($"{member.Mention} 200 emblems run when").Wait();
                        }
                    }
                }
            }
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
                case ReplayStatus.GameError:
                    SendMessage("The game crashed with the signal: " + args.ErrorMessage);
                    return;
                case ReplayStatus.UnhandledException:
                    SendMessage("i am stupid");
                    return;
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
                    .WithFramerate(35)
                    .ForcePixelFormat("yuv420p")
                    .ForceFormat("mp4"))
                .ProcessSynchronously();
        }

        private static async Task MainAsync(string[] args)
        {
            discord = new DiscordClient(new DiscordConfiguration
            {
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMembers,
                Token = ConfigurationManager.AppSettings["APIKey"],
                TokenType = TokenType.Bot
            });
            discord.UseInteractivity(new InteractivityConfiguration()
            {
                PollBehaviour = PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromSeconds(30)
            });

            slashCommands = discord.UseSlashCommands(new SlashCommandsConfiguration());

            commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { "!" },
                EnableDefaultHelp = false
            });
            discord.ComponentInteractionCreated += async (s, e) =>
            {
                if (e.Id == AddonsCommands.AddonSelectorComponentId)
                {
                    await AddonsCommands.AddonsManagementCommands.DownloadSelectedAddon(e);
                    return;
                }
                
                if (e.Interaction.User.Username == Commands.helpUser || e.Interaction.User.Username == Commands.addonsUser.Username)
                {
                    switch (e.Interaction.Data.ComponentType)
                    {
                        case ComponentType.Select:
                            string message = "";
                            foreach (var thing in e.Values)
                            {
                                message += thing;
                            }

                            DiscordEmbedBuilder commandList = new DiscordEmbedBuilder();
                            switch (message)
                            {
                                case "records_label":
                                    commandList = new DiscordEmbedBuilder
                                    {
                                        Title = "Help (!records)",
                                        Description =
                                            "The !records command can be used for ILs while !fgrecords is used for Full-game Runs\n\n IL: !records (level) (character) \n FG: !fgrecords (category) (character) (version) \n\n For SRB1 Remake and All Emblems you don't need to put the character.",
                                        Color = DiscordColor.Gold
                                    };
                                    commandList.AddField("IL Example",
                                        "!records GFZ1 sonic = Greenflower Zone Act 1 Sonic");
                                    commandList.AddField("Full-game Example",
                                        "!fgrecords any% knuckles 2.1 = Knuckles Any% 2.1");
                                    commandList.AddField("Full-game Example 2",
                                        "!fgrecords emblems 2.1 = All Emblems 2.1");
                                    break;

                                case "replay_label":
                                    commandList = new DiscordEmbedBuilder
                                    {
                                        Title = "Help (!reptomp4)",
                                        Description =
                                            "Use !reptomp4 and attach a file to convert your replay to an mp4 file. \n\n You can add addons by first looking at the addons available with !addons and then put !reptomp4 (addonname.pk3/wad) \n\n Lastly, you can use !queue to see when your replay will be converted when there are multiple replays being converted.",
                                        Color = DiscordColor.Gold
                                    };
                                    break;
                                case "host_label":
                                    commandList = new DiscordEmbedBuilder
                                    {
                                        Title = "Help (!host)",
                                        Description =
                                            "Attach a file to your message and send '!host' to upload your replay to roborecords.org. \n\n The bot will give you back a link you can use for verification on SRC ILs.",
                                        Color = DiscordColor.Gold
                                    };
                                    break;
                            }


                            DiscordMessageBuilder ruleMessage =
                                new DiscordMessageBuilder().AddEmbed(commandList).WithContent("");
                            await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                                new DiscordInteractionResponseBuilder(ruleMessage));
                            break;
                    }
                    
                }
                else
                {
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                }
            };


            discord.GuildDownloadCompleted += (sender, eventArgs) =>
            {
                return Task.Run(() =>
                {
                    GuildEventLogger.Initialize(ref discord);
                    commands.RegisterCommands<GuildEventLoggerCommands>();

                    Console.WriteLine("Initializing ReactionInteractions...");
                    reactionInteractions = new ReactionInteractions(discord);
                    commands.RegisterCommands<ReactionSetupCommands>();
                    Console.WriteLine("ReactionInteractions Initialized!");
                    
                    Console.WriteLine("Initializing JoinRolesInteractions...");
                    joinRolesInteractions = new JoinRolesInteractions(discord);
                    commands.RegisterCommands<JoinRolesSetupCommands>();
                    Console.WriteLine("JoinRolesInteractions Initialized!");

                    leminMentionTimer.Elapsed += LeminMentionTimerOnElapsed;
                });
            };

            slashCommands.RegisterCommands<AddonsCommands.AddonsManagementCommands>();
            slashCommands.RegisterCommands<AddonsCommands.AddonsListCommands>();

            commands.RegisterCommands<Commands>();

            DiscordActivity activity = new DiscordActivity("gfz1 stream", ActivityType.Watching);

            await discord.ConnectAsync(activity);
            await Task.Delay(-1);
        }
    }
}