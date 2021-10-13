using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Entities;
using RoboBot_SRB2;
using SpeedrunComSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Interactivity.Extensions;
using FFMpegCore;

namespace RoboBot
{
    public class ParsingException : Exception
    {
        public ParsingException(string? message) : base(message)
        {
        }
    }
    
    public class MediaPaths
    {
        public string filePath = "/var/www/html/";
        public string urlPath = "https://roborecords.org/";
    }

    public class Commands : BaseCommandModule
    {
        public static DiscordChannel currentChannel;
        public static string finalVersion = "";
        public static string helpUser = "";
        public static DiscordMember addonsUser;
        
        [Command("addons")]
        public async Task AddonsInfo(CommandContext ctx)
        {
                    BaseDiscordClient client = Program.discord;
                    helpUser = ctx.User.Username;
                    DiscordEmoji mapEmoji = DiscordEmoji.FromName(client, ":map:");
                    DiscordEmoji questionEmoji = DiscordEmoji.FromName(client, ":question:");

                    var builder = new DiscordMessageBuilder()
                        .WithContent("Which addons would you like to see?")
                        .AddComponents(new DiscordComponent[]
                        {
                            new DiscordButtonComponent(ButtonStyle.Primary, "Characters", "Characters (2.2)", false, new DiscordComponentEmoji(questionEmoji)),
                            new DiscordButtonComponent(ButtonStyle.Success, "Levels", "Levels (2.2)",false, new DiscordComponentEmoji(mapEmoji)),
                            new DiscordButtonComponent(ButtonStyle.Secondary, "2.1", "2.1 Addons")
                        });
                    addonsUser = ctx.Member;
                    
            
            // var addonList = new DiscordEmbedBuilder
            // {
            //     Title = "Addons for Replay2Gif",
            //     Description = "Here are the addons available for use with the converter.",
            //     Color = DiscordColor.Gold
            // };
            //
            // addonList.AddField($"2.2 Addons", MakeModList(addons22Characters));
            // addonList.AddField($"2.1 Addons", MakeModList(addons21));
             await ctx.RespondAsync(builder);
        }



        [Command("queue")]
        public async Task ReplayQueue(CommandContext ctx)
        {
            if (Program.convertQueue.Any())
            {
                var queueList = new DiscordEmbedBuilder
                {
                    Title = "Queue for Replay to Gif converter",
                    Description = "Use this command to find where you are in the queue!",
                    Color = DiscordColor.Gold
                };
                for (int i = 0; i < Program.convertQueue.Count(); i++)
                {
                    var currentMember = Program.convertQueue[i];
                    queueList.AddField($"{i + 1}.{currentMember.Member.DisplayName}", currentMember.Message.Attachments.First().FileName);
                }
                await ctx.RespondAsync(embed: queueList);
            }
            else
            {
                await ctx.RespondAsync("Queue is empty");
            }
        }

        [Command("reptogif")]
        public async Task ReplayToGifNoAddons(CommandContext ctx) => await ReplayConverterWithAddons(ctx, null);


        [Command("reptomp4")]
        public async Task ReplayToMp4NoAddons(CommandContext ctx) => await ReplayConverterWithAddons(ctx, null);
        
        [Command("reptomp4")]
        public async Task ReplayToMp4WithAddons(CommandContext ctx, params string[] addons) => await ReplayConverterWithAddons(ctx, addons);
        
        [Command("reptogif")]
        public async Task ReplayConverterWithAddons(CommandContext ctx, params string[] addons)
        {
            try
            {
                await ctx.TriggerTypingAsync();
                JobInfo addonsJobInfo = addons != null ? JobInfo.CreateFromStrings((byte)addons.Length, addons) : JobInfo.NoAddons;
                currentChannel = ctx.Channel;
                

                if (ctx.Message.Attachments.Count != 0)
                {
                    if (ctx.Message.Attachments.First().FileSize > 300000)
                    {
                        await ctx.RespondAsync("File is too big.");
                        return;
                    }
                    
                    using (WebClient wwwClient = new WebClient())
                    {
                        wwwClient.DownloadFile(ctx.Message.Attachments.First().Url, ctx.Message.Attachments.First().FileName);
                    }

                    try
                    {
                        
                        FileInfo replay = new FileInfo(ctx.Message.Attachments.First().FileName);
                        byte[] fileBytes = File.ReadAllBytes(replay.FullName).ToArray();
                        string addonPath = "";
                        string version = "";
                        if (fileBytes[12] == 201) 
                        {
                            addonPath = $"/root/.srb2/.srb21/addons/";
                            version = "2.1";
                        }
                        else if (fileBytes[12] == 202) 
                        {
                            addonPath = $"/root/.srb2/addons";
                            version = "2.2";
                        }
                        else
                        {
                            await ctx.RespondAsync("File not playable on 2.2 or 2.1. Is it a valid replay?");
                            return;
                        }
                        File.Move(replay.Name,$"/root/.srb2/replaystogif/{replay.Name}");
                       
                        string confirmationMessage = $"Processing {version} replay sent by {ctx.Member.Username}";
                        if (addons != null)
                        {
                            for (int i = 0; i < addons.Length; i++)
                            {
                                if (version == "2.1")
                                {
                                    if (!File.Exists($"{addonPath}/{addons[i]}"))
                                    {
                                        await ctx.RespondAsync("Addon does not exist on the server.");
                                        File.Delete($"/root/.srb2/replaystogif/{replay.Name}");
                                        return;
                                    }
                                }
                                else
                                {
                                    if (!File.Exists($"{addonPath}/Levels/{addons[i]}") && !File.Exists($"{addonPath}/Characters/{addons[i]}"))
                                    {
                                        await ctx.RespondAsync("Addon does not exist on the server.");
                                        File.Delete($"/root/.srb2/replaystogif/{replay.Name}");
                                        return;
                                    }
                                }
                            }
                            confirmationMessage += " with addon(s) " + string.Join(" ", addons);
                        }
                       
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))

                        {
                            await ctx.RespondAsync(confirmationMessage);
                            Program.convertQueue.Add(ctx);
                        }
                        
                        Program.replayEvents.AddToQueue(addonsJobInfo, $"/root/.srb2/replaystogif/{replay.Name}", "/var/www/html/gifs/torename.gif");
                        
                    }
                    catch (Exception e)
                    {
                         await ctx.RespondAsync("Error: " + e.Message);
                        // await ctx.RespondAsync(e.StackTrace);
                        // await ctx.RespondAsync(e.Source);
                        // await ctx.RespondAsync(e.InnerException.Message);
                    }
                }
                else
                {
                    await ctx.RespondAsync("No file attached.");
                }
            }
            catch (Exception e)
            {
                 await ctx.RespondAsync("Error: " + e.Message);
                // await ctx.RespondAsync(e.StackTrace);
                // await ctx.RespondAsync(e.Source);
                // await ctx.RespondAsync(e.InnerException.Message);
            }
        }




        [Command("help")]
        public async Task Help(CommandContext ctx)
        {
            BaseDiscordClient client = Program.discord;
            helpUser = ctx.User.Username;
            DiscordEmoji joystickEmoji = DiscordEmoji.FromName(client, ":joystick:");
            DiscordEmoji filmEmoji = DiscordEmoji.FromName(client, ":film_frames:");
            DiscordEmoji cloudEmoji = DiscordEmoji.FromName(client, ":cloud:");
            
            var options = new List<DiscordSelectComponentOption>()
            {
                new DiscordSelectComponentOption("!records", "records_label", "Get the top 5 records of any level/category!",emoji: new DiscordComponentEmoji(joystickEmoji)),
                new DiscordSelectComponentOption("!reptomp4", "replay_label", "Convert your replays into Mp4s!", emoji: new DiscordComponentEmoji(filmEmoji)),
                new DiscordSelectComponentOption("!host", "host_label", "Easily host your replay files!", emoji: new DiscordComponentEmoji(cloudEmoji))
            };
            var dropdown = new DiscordSelectComponent("dropdown", null, options,false,1,1);
            
            var builder = new DiscordMessageBuilder().WithContent("What command would you like to learn about?").AddComponents(dropdown);
            
            await builder.SendAsync(ctx.Channel); 
        }

        [Command("site")]
        public async Task Site(CommandContext ctx)
        {
            var linkList = new DiscordEmbedBuilder
            {
                Title = "Speedrun.com",
                Description = $"Links for the vanilla SRC board",
                Color = DiscordColor.Gold
            };
            linkList.AddField("Full-game board", "https://www.speedrun.com/srb2/full_game");
            linkList.AddField("IL board", "https://www.speedrun.com/srb2/individual_levels");

            await ctx.RespondAsync(embed: linkList);
        }

        [Command("records")]
        public async Task Records(CommandContext ctx, string level, string character)
        {
            try
            {
                bool gotLvl = SRB2Enums.levelsID.TryGetValue(level.ToUpper(), out SRB2Level srb2Level);

                bool gotCat = SRB2Enums.categoriesID.TryGetValue(character.ToLower(), out string categoryID);

                bool nights = false;
                if (gotLvl)
                {
                    int mapNumber = SRB2Level.GetMapNumber(srb2Level.MapName);
                    if (mapNumber > 49 && mapNumber < 74)
                    {
                        categoryID = "xd1g1j4d";
                        gotCat = true;
                        nights = true;
                    }
                }

                if (!gotCat)
                {
                    throw new ParsingException("Wrong / Missing parameter: Character");
                }

                if (!gotLvl)
                {
                    throw new ParsingException("Wrong / Missing parameter: Level");
                }

                if (gotCat && gotLvl)
                {
                    Leaderboard leaderboard;
                    leaderboard = Program.srcClient.Leaderboards.GetLeaderboardForLevel(
                    Program.srb2Game.ID,
                    srb2Level.SrcID,
                    categoryID,
                    5
                    );
                    
                    DiscordEmbedBuilder.EmbedThumbnail thumbnailUrl = new DiscordEmbedBuilder.EmbedThumbnail();
                    thumbnailUrl.Url = @"https://roborecords.org/lvlicons/" + srb2Level.FullName.Replace(" ", string.Empty) + ".png";
                    DiscordEmbedBuilder.EmbedFooter embedFooter = new DiscordEmbedBuilder.EmbedFooter();
                    embedFooter.Text = Program.s.RandomStat();
                    Random r = new Random();
                    int footerImgNum = r.Next(1, 21);
                    embedFooter.IconUrl = $"https://roborecords.org/footerimgs/{footerImgNum}.png";
                    var records = new DiscordEmbedBuilder
                    {
                        Title = srb2Level.FullName + " | " + leaderboard.Category.Name,
                        Thumbnail = thumbnailUrl,
                        Footer = embedFooter,
                        Url = leaderboard.WebLink.AbsoluteUri,
                        Color = nights ? DiscordColor.Magenta : CharacterColor(leaderboard)
                    };
                    
                    for (int i = 0; i < leaderboard.Records.Count(); i++)
                    {
                        Record currentRecord = leaderboard.Records[i];
                        string playerName = currentRecord.Player.Name;
                        string displayedTimeFormat = currentRecord.Times.Primary.Value.Minutes != 0 ? Program.timeFormatWithMinutes : Program.timeFormat;

                        string runTime = currentRecord.Times.GameTimeISO.Value.ToString(displayedTimeFormat).TrimStart(new Char[] { '0' });

                        records.AddField($"{i + 1}. {playerName} | {runTime}",
                        "Submitted on " + currentRecord.DateSubmitted.Value.ToString("d") +
                        ", " +
                        DSharpPlus.Formatter.MaskedUrl("Link", currentRecord.WebLink, "Submission Link")
                        );
                    }
                    await ctx.RespondAsync(embed: records);
                }
            }
            catch (ParsingException p)
            {
                await ctx.RespondAsync(p.Message);
                await ctx.RespondAsync("Type !help for more info");
            }
            catch (Exception e)
            {
                await ctx.RespondAsync("Internal Error");
                await ctx.RespondAsync(e.Message);
            }
        }

        [Command("records")]
        public async Task RecordsOnlyLvl(CommandContext ctx, string level) => await Records(ctx, level, "sonic");


        [Command("fgrecords")]
        public async Task FgRecords(CommandContext ctx, string category, string character, string version)
        {
            try
            {
                bool gotCatFg = SRB2Enums.fgCategoriesID.TryGetValue(character.ToLower(), out string categoryFgID); //checking for the character to get the ID off of it

                if (!gotCatFg)
                {
                    if (!SRB2Enums.fgCategoriesID.TryGetValue(category.ToLower(), out categoryFgID)) { categoryFgID = "ndx46012"; } // checking for all emblems / srb1 (since they're as the first argument when typing the command), if not defaulting to sonic
                }

                string goal;
                if (categoryFgID == "9d8pmg3k") //all emblems
                {
                    goal = SRB2Enums.GetGoal(categoryFgID);
                }
                else //everything else
                {
                    goal = SRB2Enums.GetGoal(category);
                }

                bool gotVer = false;

                string originalVer = "";
                string processedVer = "";
                if (categoryFgID == "9d8pmg3k" || categoryFgID == "9d8pm0qk")
                {
                    originalVer = string.Join(" ", character + version).ToLower();
                    gotVer = SRB2Enums.versions.TryGetValue(originalVer, out processedVer);
                }
                else
                {
                    originalVer = version;
                    gotVer = SRB2Enums.versions.TryGetValue(originalVer, out processedVer);
                }

                if (!gotVer && originalVer != "")
                {
                    throw new ParsingException("Wrong / Missing version");
                }

                Leaderboard leaderboard = FullGameLeaderboard(goal, categoryFgID, processedVer, originalVer);
                DiscordEmbedBuilder.EmbedFooter embedFooter = new DiscordEmbedBuilder.EmbedFooter();
                DiscordEmbedBuilder.EmbedThumbnail thumbnailUrl = new DiscordEmbedBuilder.EmbedThumbnail();
                embedFooter.Text = Program.s.RandomStat();
                Random r = new Random();
                int footerImgNum = r.Next(1, 21);
                embedFooter.IconUrl = $"https://roborecords.org/footerimgs/{footerImgNum}.png";
                var records = new DiscordEmbedBuilder
                {
                    Title = $"{goal} | ",
                    Thumbnail = thumbnailUrl,
                    Footer = embedFooter,
                    Url = leaderboard.WebLink.AbsoluteUri,
                    Color = CharacterColor(leaderboard),
                };
                if (categoryFgID == "9d8pmg3k" || categoryFgID == "9d8pm0qk")
                {
                    string formattedCat = leaderboard.Category.Name.Replace(" ", string.Empty);
                    string url = $"https://roborecords.org/fgicons/{formattedCat}.png";
                    thumbnailUrl.Url = url;
                    records.Title += finalVersion;
                }
                else
                {
                    string formattedGoal = goal.Replace(" ", string.Empty).Replace("%", string.Empty);
                    string url = $"https://roborecords.org/fgicons/{formattedGoal}.png";
                    thumbnailUrl.Url = url;
                    records.Title += leaderboard.Category.Name + " | " + finalVersion;
                }

                string displayedTimeFormat = Program.timeFormatWithMinutes;
                if (leaderboard.Records.Count != 0)
                {
                    if (leaderboard.Records.Any(x => x.Times.PrimaryISO.Value.Hours != 0))
                    {
                        displayedTimeFormat = Program.timeFormatWithHours;
                    }
                }

                for (int i = 0; i < leaderboard.Records.Count(); i++)
                {
                    Record currentRecord = leaderboard.Records[i];
                    string playerName = currentRecord.Player.Name;
                    string runTime = currentRecord.Times.PrimaryISO.Value.ToString(displayedTimeFormat).TrimStart(new Char[] { '0' });
                    records.AddField($"{i + 1}. {playerName} | {runTime}",
                    "Submitted on " + currentRecord.DateSubmitted.Value.ToString("d") +
                    ", " +
                    DSharpPlus.Formatter.MaskedUrl("Link", currentRecord.WebLink, "Submission Link")
                    );
                }

                await ctx.RespondAsync(embed: records);
            }
            catch (ParsingException p)
            {
                await ctx.RespondAsync(p.Message);
                await ctx.RespondAsync("Type !help for more info");
            }
            catch (Exception e)
            {
                await ctx.RespondAsync($"Internal Error \n{e.Message} \n{e.Source} \n{e.StackTrace}");
            }
        }

        [Command("fgrecords")]
        public async Task FGRecordsNoVersion(CommandContext ctx, string category, string character) => await FgRecords(ctx, category, character, "");
        

        [Command("fgrecords")]
        public async Task FGRecordsNoVersionNoCharacter(CommandContext ctx, string category) => await FgRecords(ctx, category, "", "");
        

        [Command("fgrecords")]
        public async Task FGRecordsNoParams(CommandContext ctx) => await ctx.RespondAsync("No parameters given\nType !help for more info");
        

        [Command("records")]
        public async Task ILRecordsNoParams(CommandContext ctx) => await ctx.RespondAsync("No parameters given\nType !help for more info");


        [Command("host")]
        public async Task HostReplay(CommandContext ctx)
        {
            
            string username = Regex.Replace(ctx.User.Username, @"[^0-9a-zA-Z]+", "");
            string fileName = ctx.Message.Attachments.First().FileName;
            
            if (!Directory.Exists("/var/www/html/replays/" + username))
            {
                Directory.CreateDirectory("/var/www/html/replays/" + username);
            }
            string[] allReplays = Directory.GetFiles("/var/www/html/replays/" + username);
            
            int fileNum = 1;
            foreach (string replay in allReplays)
            {
                
                if (replay.Contains(fileName.Substring(0,fileName.Length-4)))
                {
                    fileNum++;
                }
            }


            if (ctx.Message.Attachments.Count != 0)
            {
                if (ctx.Message.Attachments.First().FileName
                    .Substring(ctx.Message.Attachments.First().FileName.Length - 3) != "lmp")
                {
                    await ctx.RespondAsync("Not a demo file.");
                    return;
                }
                
                if (ctx.Message.Attachments.First().FileSize > 500000)
                {
                    await ctx.RespondAsync("File is too big.");
                    return;
                }

                if (fileNum != 1)
                {
                    fileName = fileName.Insert(fileName.Length - 4, fileNum.ToString());
                }

                using (WebClient wwwClient = new WebClient())
                {
                    wwwClient.DownloadFile(ctx.Message.Attachments.First().Url,
                        $"/var/www/html/replays/{username}/{fileName}");
                }
            }

            await ctx.RespondAsync($"https://roborecords.org/replays/{username}/{fileName}");
        }
        
        private static DiscordColor CharacterColor(Leaderboard leaderboard)
        {
            var charColor = DiscordColor.Black;
            switch (leaderboard.Category.Name)
            {
                case "Sonic":
                    charColor = DiscordColor.Blue;
                    break;

                case "Tails":
                    charColor = DiscordColor.Orange;
                    break;

                case "Knuckles":
                    charColor = DiscordColor.Red;
                    break;

                case "Amy":
                    charColor = DiscordColor.HotPink;
                    break;

                case "Fang":
                    charColor = DiscordColor.Purple;
                    break;

                case "Metal Sonic":
                    charColor = DiscordColor.DarkBlue;
                    break;

                case "All Emblems":
                    charColor = DiscordColor.Goldenrod;
                    break;

                case "SRB1 Remake":
                    charColor = DiscordColor.CornflowerBlue;
                    break;
            }

            return charColor;
        }
        
        private static Leaderboard FullGameLeaderboard(string goal, string categoryId, string version, string originalVer)
        {
            Console.WriteLine(string.Join('\n', goal, categoryId, version, originalVer));

            IEnumerable<Variable> fgvariables = Program.srb2Game.FullGameCategories.First(x => x.ID == categoryId).Variables;
            //List<VariableValue> value = fgvariables[0].Values.ToList();
            if (categoryId != "9d8pm0qk" && categoryId != "9d8pmg3k") // Not SRB1 or All Emblems
            {
                if (originalVer == "")
                {
                    version = "2.2 Current";
                }
                IEnumerable<VariableValue> value = fgvariables.First(x => x.Name == "Goal").Values;
                VariableValue cat = value.First(x => x.Value == goal);

                IEnumerable<VariableValue> vervalue = fgvariables.First(x => x.Name == "Version").Values;
                VariableValue ver = vervalue.First(x => x.Value == version);

                finalVersion = ver.Value;
                IEnumerable<VariableValue> varValues = new VariableValue[] { cat, ver };
                return Program.srcClient.Leaderboards.GetLeaderboardForFullGameCategory(Program.gameId, categoryId, 5, variableFilters: varValues);
            }
            else if (categoryId == "9d8pm0qk") // Srb1 Remake
            {
                if (originalVer == "")
                {
                    version = "2.1.X";
                }
                IEnumerable<VariableValue> vervalue = fgvariables.First(x => x.Name == "Version").Values;
                VariableValue ver = vervalue.First(x => x.Value == version);
                finalVersion = ver.Value;
                IEnumerable<VariableValue> varValues = new VariableValue[] { ver };
                return Program.srcClient.Leaderboards.GetLeaderboardForFullGameCategory(Program.gameId, categoryId, 5, variableFilters: varValues);
            }
            else if (categoryId == "9d8pmg3k") // Emblems
            {
                if (originalVer == "")
                {
                    version = "2.2 Current";
                }
                IEnumerable<VariableValue> vervalue = fgvariables.First(x => x.Name == "Version").Values;
                VariableValue ver = vervalue.First(x => x.Value == version);
                finalVersion = ver.Value;
                IEnumerable<VariableValue> varValues = new VariableValue[] { ver };
                return Program.srcClient.Leaderboards.GetLeaderboardForFullGameCategory(Program.gameId, categoryId, 5, variableFilters: varValues);
            }

            return null;
        }
    }
}