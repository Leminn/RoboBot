using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using FluentFTP;
using RoboBot_SRB2;
using SpeedrunComSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace RoboBot
{
    public class ParsingException : Exception
    {
        public ParsingException(string? message) : base(message)
        {
        }
    }

    public class MyCommands : BaseCommandModule
    {
        public static DiscordChannel loool;
        public static string finalVersion = "";

        [Command("addons")]
        public async Task AddonsInfo(CommandContext ctx)
        {
            string hostAddress = ConfigurationManager.AppSettings["FTPAddress"];
            string hostName = ConfigurationManager.AppSettings["FTPName"];
            string hostPassword = ConfigurationManager.AppSettings["FTPPassword"];
            FtpClient client = new FtpClient(hostAddress, hostName, hostPassword);

            client.Connect();
            FtpListItem[] addons = client.GetListing("/addons");
            var addonList = new DiscordEmbedBuilder
            {
                Title = "Addons for Replay2Gif",
                Description = "Here are the addons available for use with the converter.",
                Color = DiscordColor.Gold
            };
            for (int i = 0; i < addons.Length; i++)
            {
                addonList.AddField($"{i + 1}.", addons[i].Name);
            }
            await ctx.RespondAsync(embed: addonList);

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
        public async Task ReplayToGifNoAddons(CommandContext ctx)
        {
            await ReplayToGifWithAddons(ctx, null);
        }

        [Command("reptogif")]
        public async Task ReplayToGifWithAddons(CommandContext ctx, params string[] addons)
        {
            try
            {
                await ctx.TriggerTypingAsync();
                JobInfo thejoblol = addons != null ? JobInfo.CreateFromStrings((byte)addons.Length, addons) : JobInfo.NoAddons;
                byte[] thebyteslol = thejoblol.ToBytes();

                loool = ctx.Channel;

                string hostAddress = ConfigurationManager.AppSettings["FTPAddress"];
                string hostName = ConfigurationManager.AppSettings["FTPName"];
                string hostPassword = ConfigurationManager.AppSettings["FTPPassword"];
                FtpClient client = new FtpClient(hostAddress, hostName, hostPassword);

                client.Connect();
                Console.WriteLine(client.ServerType);

                if (ctx.Message.Attachments != null)
                {
                    if (ctx.Message.Attachments.First().FileSize > 100000)
                    {
                        await ctx.RespondAsync("too big");
                    }
                    using (WebClient wwwClient = new WebClient())
                    {
                        wwwClient.DownloadFile(ctx.Message.Attachments.First().Url, ctx.Message.Attachments.First().FileName);

                    }
                    try
                    {

                        FileInfo replay = new FileInfo(ctx.Message.Attachments.First().FileName);
                        byte[] fileBytes = File.ReadAllBytes(replay.FullName).Concat(thebyteslol).ToArray();
                        client.Upload(fileBytes, $"/replaystogif/{replay.Name}.part", FtpRemoteExists.Skip);
                        client.MoveFile($"/replaystogif/{replay.Name}.part", $"/replaystogif/{replay.Name}"); // renames on linux
                        File.Delete(replay.FullName);
                        // client.Rename($"/replaystogif/{replay.Name}.part", replay.Name); this bad on linux

                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        {
                            await ctx.RespondAsync("Processing replay...");
                            Program.convertQueue.Add(ctx);
                        }
                    }
                    catch (Exception e)
                    {
                        await ctx.RespondAsync(e.Message);
                        await ctx.RespondAsync(e.StackTrace);
                        await ctx.RespondAsync(e.Source);
                        await ctx.RespondAsync(e.InnerException.Message);
                    }
                }
                else
                {
                    await ctx.RespondAsync("no file attached");
                }
            }
            catch (Exception e)
            {
                await ctx.RespondAsync(e.Message);
                await ctx.RespondAsync(e.StackTrace);
                await ctx.RespondAsync(e.Source);
                await ctx.RespondAsync(e.InnerException.Message);
            }
        }

        [Command("help")]
        public async Task Help(CommandContext ctx)
        {
            await HelpSheet(ctx, " ");
        }

        [Command("help")]
        public async Task HelpSheet(CommandContext ctx, string command)
        {
            DiscordEmbedBuilder commandList;
            switch (command)
            {
                case "records":
                    commandList = new DiscordEmbedBuilder
                    {
                        Title = "Help (!records)",
                        Description = "The !records command can be used for ILs while !fgrecords is used for Full-game Runs\n\n IL: !records (level) (character) \n FG: !fgrecords (category) (character) (version) \n\n For SRB1 Remake and All Emblems you don't need to put the character.",
                        Color = DiscordColor.Gold
                    };
                    commandList.AddField("IL Example", "!records GFZ1 sonic = Greenflower Zone Act 1 Sonic");
                    commandList.AddField("Full-game Example", "!fgrecords any% knuckles 2.1 = Knuckles Any% 2.1");
                    commandList.AddField("Full-game Example 2", "!fgrecords emblems 2.1 = All Emblems 2.1");
                    await ctx.RespondAsync(embed: commandList);
                    break;

                case "reptogif":
                    commandList = new DiscordEmbedBuilder
                    {
                        Title = "Help (!reptogif)",
                        Description = "Use !reptogif and attach a file to convert your replay to a gif file. \n\n You can add addons by first looking at the addons available with !addons and then put !reptogif (addonname.pk3/wad) \n\n Lastly, you can use !queue to see when your replay will be converted when there are multiple replays being converted.",
                        Color = DiscordColor.Gold
                    };
                    await ctx.RespondAsync(embed: commandList);
                    break;

                default:
                    await ctx.RespondAsync("Enter either !help records or !help reptogif");
                    break;


            }


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

                if (gotCat == true && gotLvl == true)
                {
                    Leaderboard leaderboard;
                    leaderboard = Program.srcClient.Leaderboards.GetLeaderboardForLevel(
                    Program.srb2Game.ID,
                    srb2Level.SrcID,
                    categoryID,
                    5
                    );

                    DiscordEmbedBuilder.EmbedThumbnail thumbnailUrl = new DiscordEmbedBuilder.EmbedThumbnail();
                    thumbnailUrl.Url = "http://77.68.95.193/lvlicons/" + srb2Level.FullName.Replace(" ", string.Empty) + ".png";
                    DiscordEmbedBuilder.EmbedFooter embedFooter = new DiscordEmbedBuilder.EmbedFooter();
                    embedFooter.Text = Program.s.RandomStat();
                    Random r = new Random();
                    int footerImgNum = r.Next(1, 21);
                    embedFooter.IconUrl = $"http://77.68.95.193/footerimgs/{footerImgNum}.png";
                    var records = new DiscordEmbedBuilder
                    {
                        Title = srb2Level.FullName,
                        Thumbnail = thumbnailUrl,
                        Footer = embedFooter,
                        Url = leaderboard.WebLink.AbsoluteUri
                    };
                    CharacterColor(leaderboard, records);

                    switch (nights)
                    {
                        case true:
                            records.Color = DiscordColor.Magenta;
                            break;

                        case false:
                            records.Title += " | " + leaderboard.Category.Name;
                            break;
                    }

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
        public async Task RecordsOnlyLvl(CommandContext ctx, string level)
        {
            await Records(ctx, level, "sonic");
        }

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
                embedFooter.IconUrl = $"http://77.68.95.193/footerimgs/{footerImgNum}.png";
                var records = new DiscordEmbedBuilder
                {
                    Title = $"{goal} | ",
                    Thumbnail = thumbnailUrl,
                    Footer = embedFooter,
                    Url = leaderboard.WebLink.AbsoluteUri
                };
                CharacterColor(leaderboard, records);
                if (categoryFgID == "9d8pmg3k" || categoryFgID == "9d8pm0qk")
                {
                    string formattedCat = leaderboard.Category.Name.Replace(" ", string.Empty);
                    string url = $"http://77.68.95.193/fgicons/{formattedCat}.png";
                    thumbnailUrl.Url = url;
                    records.Title += finalVersion;
                }
                else
                {
                    string formattedGoal = goal.Replace(" ", string.Empty).Replace("%", string.Empty);
                    string url = $"http://77.68.95.193/fgicons/{formattedGoal}.png";
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
        public async Task FGRecordsNoVersion(CommandContext ctx, string category, string character)
        {
            await FgRecords(ctx, category, character, "");
        }

        [Command("fgrecords")]
        public async Task FGRecordsNoVersionNoCharacter(CommandContext ctx, string category)
        {
            await FgRecords(ctx, category, "", "");
        }

        [Command("fgrecords")]
        public async Task Recordsoof2(CommandContext ctx)
        {
            await ctx.RespondAsync("No parameters given\nType !help for more info");
        }

        [Command("records")]
        public async Task Recordsoof(CommandContext ctx)
        {
            await ctx.RespondAsync("No parameters given\nType !help for more info");
        }

        private static void CharacterColor(Leaderboard leaderboard, DiscordEmbedBuilder records)
        {
            switch (leaderboard.Category.Name)
            {
                case "Sonic":
                    records.Color = DiscordColor.Blue;
                    break;

                case "Tails":
                    records.Color = DiscordColor.Orange;
                    break;

                case "Knuckles":
                    records.Color = DiscordColor.Red;
                    break;

                case "Amy":
                    records.Color = DiscordColor.HotPink;
                    break;

                case "Fang":
                    records.Color = DiscordColor.Purple;
                    break;

                case "Metal Sonic":
                    records.Color = DiscordColor.DarkBlue;
                    break;

                case "All Emblems":
                    records.Color = DiscordColor.Goldenrod;
                    break;

                case "SRB1 Remake":
                    records.Color = DiscordColor.CornflowerBlue;
                    break;
            }
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