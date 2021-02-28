using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using RoboBot_SRB2;
using SpeedrunComSharp;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;

namespace RoboBot
{
    public class MyCommands : BaseCommandModule
    {
        public static string finalVersion = "";
        [Command("help")]
        public async Task HelpSheet(CommandContext ctx)
        {
            var commandList = new DiscordEmbedBuilder
            {
                Title = "Help",
                Description = "The !records command can be used for both ILs and Full-game Runs\n\n IL: !records (level) (character) \n FG: !records (category) (character) (version) \n\n For SRB1 Remake and All Emblems you don't need to put the character.",
                Color = DiscordColor.Gold
            };
            commandList.AddField("IL Example", "!records GFZ1 sonic = Greenflower Zone Act 1 Sonic");
            commandList.AddField("Full-game Example", "!records any% knuckles 2.1 = Knuckles Any% 2.1");
            commandList.AddField("Full-game Example 2", "!records emblems 2.1 = All Emblems 2.1");

            await ctx.RespondAsync(embed: commandList);
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
                    await ctx.RespondAsync("Wrong / Missing parameter: Character");
                }

                if (!gotLvl)
                {
                    await ctx.RespondAsync("Wrong / Missing parameter: Level");
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
                DiscordEmoji pog = DiscordEmoji.FromName(Program.discord, ":pog:");
                embedFooter.Text = $"{pog.GetDiscordName()}";
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
                    string playerName = leaderboard.Records[i].Player.Name;
                    string runTime = leaderboard.Records[i].Times.GameTimeISO.Value.ToString(Program.timeFormat);
                    records.AddField($"{i + 1}. {playerName} | {runTime}",
                    leaderboard.Records[i].WebLink.AbsoluteUri);

                }
                await ctx.RespondAsync(embed: records);
                }
                else
                {
                    await ctx.RespondAsync("Type !help for more info");
                }
            }

        [Command("records")]
        public async Task RecordsOnlyLvl(CommandContext ctx, string level)
        {
            await Records(ctx, level, "sonic");
        }



        [Command("fgrecords")]
        public async Task FgRecords(CommandContext ctx, string level, string character, string version)
        {
            try
            {
                bool gotCatFg = SRB2Enums.fgCategoriesID.TryGetValue(character.ToLower(), out string categoryFgID);



                if (!gotCatFg)
                {
                    gotCatFg = SRB2Enums.fgCategoriesID.TryGetValue(level.ToLower(), out categoryFgID);

                }

                if (gotCatFg)
                {
                    string goal;
                    if (categoryFgID == "9d8pmg3k")
                    {
                        goal = SRB2Enums.GetGoal(categoryFgID);
                    }
                    else
                    {
                        goal = SRB2Enums.GetGoal(level);
                    }
                    if (goal != "notfound")
                    {

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
                            throw new Exception("Wrong / Missing version");
                        }

                        Leaderboard leaderboard = FullGameLeaderboard(goal, categoryFgID, processedVer, originalVer);
                        DiscordEmbedBuilder.EmbedFooter embedFooter = new DiscordEmbedBuilder.EmbedFooter();
                        DiscordEmbedBuilder.EmbedThumbnail thumbnailUrl = new DiscordEmbedBuilder.EmbedThumbnail();
                        embedFooter.Text = $"";
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

                        string displayedTimeFormat = Program.timeFormat;
                        if (leaderboard.Records.Count != 0)
                        {
                            if (leaderboard.Records.Any(x => x.Times.PrimaryISO.Value.Hours != 0))
                            {
                                displayedTimeFormat = Program.timeFormatWithHours;
                            }
                        }

                        for (int i = 0; i < leaderboard.Records.Count(); i++)
                        {
                            string playerName = leaderboard.Records[i].Player.Name;
                            string runTime = leaderboard.Records[i].Times.PrimaryISO.Value.ToString(displayedTimeFormat);
                            records.AddField($"{i + 1}. {playerName} | {runTime}",
                            leaderboard.Records[i].WebLink.AbsoluteUri);
                        }

                        await ctx.RespondAsync(embed: records);
                    }
                }
            }


            catch (Exception e)
            {
                await ctx.RespondAsync("Type !help for more info");

                /* await ctx.RespondAsync(e.Source);
                 await ctx.RespondAsync(e.Message);
                 await ctx.RespondAsync(e.StackTrace);
                 */
            }
        }
        [Command("fgrecords")]
        public async Task FGRecordsNoVersion(CommandContext ctx, string category, string character)
        {
            await FgRecords(ctx, category, character, "");
        }

        [Command("fgrecords")]
        public async Task FGRecordsNoVersionCharacter(CommandContext ctx, string category)
        {
            await FgRecords(ctx, category, "sonic", "");
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
        {//
            IEnumerable<Variable> fgvariables = Program.srb2Game.FullGameCategories.First(x => x.ID == categoryId).Variables;
            //List<VariableValue> value = fgvariables[0].Values.ToList();  
            if (categoryId != "9d8pm0qk" && categoryId != "9d8pmg3k") // Not SRB1 or All Emblems
            {
                if (originalVer == "")
                {
                    version = "2.2 Current";
                }
                IEnumerable<VariableValue> value = fgvariables.First(x => x.Name == "Goal").Values;
                VariableValue categoryyyy = value.First(x => x.Value == goal);
                IEnumerable<VariableValue> vervalue = fgvariables.First(x => x.Name == "Version").Values;
                VariableValue verrrr = vervalue.First(x => x.Value == version);
                finalVersion = verrrr.Value;
                IEnumerable<VariableValue> varValues = new VariableValue[] { categoryyyy, verrrr };
                return Program.srcClient.Leaderboards.GetLeaderboardForFullGameCategory(Program.gameId, categoryId, 5, variableFilters: varValues);
            }
            else if(categoryId == "9d8pm0qk") // Srb1 Remake
            {
                if(originalVer == "")
                {
                    version = "2.1.X";
                }
                IEnumerable<VariableValue> vervalue = fgvariables.First(x => x.Name == "Version").Values;
                VariableValue verrrr = vervalue.First(x => x.Value == version);
                finalVersion = verrrr.Value;
                IEnumerable<VariableValue> varValues = new VariableValue[] { verrrr };
                return Program.srcClient.Leaderboards.GetLeaderboardForFullGameCategory(Program.gameId, categoryId, 5, variableFilters: varValues);
            }
            else if (categoryId == "9d8pmg3k") // Emblems
            {
                if (originalVer == "")
                {
                    version = "2.2 Current";
                }
                IEnumerable<VariableValue> vervalue = fgvariables.First(x => x.Name == "Version").Values;
                VariableValue verrrr = vervalue.First(x => x.Value == version);
                finalVersion = verrrr.Value;
                IEnumerable<VariableValue> varValues = new VariableValue[] { verrrr };
                return Program.srcClient.Leaderboards.GetLeaderboardForFullGameCategory(Program.gameId, categoryId, 5, variableFilters: varValues);
            }

            return null;
        }


    }
}
