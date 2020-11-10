using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Threading.Tasks;
using System.Configuration;
using System.Linq;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Net;
using SpeedrunComSharp;
using Enums = RoboBot.SRB2Enums;

namespace RoboBot
{
    public class MyCommands
    {

        [Command("levels")]
        public async Task HelpSheet(CommandContext ctx)
        {
            var commandList = new DiscordEmbedBuilder
            {
                Title = "Level List",
                Color = DiscordColor.Green
            };
            List<string> allLevels = Enums.levelsID.Keys.ToList();
            for(int i = 0; i < 3; i++){
                switch(i){
                    case 0:
                        commandList.AddField("Greenflower Zone", allLevels[0] + ", " + 
                        allLevels[1] + ", " +
                        allLevels[2]
                        );
                        break;
                    
                    case 1:
                        commandList.AddField("Techno Hill Zone", allLevels[3] + ", " +
                        allLevels[4] + ", " +
                        allLevels[5]
                        );
                        break;
                    
                    case 2:
                        commandList.AddField("Deep Sea Zone", allLevels[6] + ", " +
                        allLevels[7] + ", " +
                        allLevels[8]
                        );
                        break;
                }
            }
            
            /*foreach(string levelName in allLevels.ToList()){
                try{
                commandList.AddField(levelName,"------");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            } */

            await ctx.RespondAsync(embed: commandList);
        } 

        [Command("records")]
        public async Task Greenflower1Records(CommandContext ctx, string level,  string character){
            bool gotLvl = Enums.levelsID.TryGetValue(level.ToUpper(), out string levelID);
            
            bool gotCat = Enums.categoriesID.TryGetValue(character.ToLower(), out string categoryID);

            if(gotCat == false || gotLvl == false)
            {
                await ctx.RespondAsync(ctx.Command.Arguments[0].Description);
                await ctx.RespondAsync("commandName: " + ctx.Command.Name);
                await ctx.RespondAsync("raw argument string: " + ctx.RawArgumentString);
                await ctx.RespondAsync("oof");
            }
            else
            {
                
                Leaderboard leaderboard = Program.srcClient.Leaderboards.GetLeaderboardForLevel(
                    Program.srb2Game.ID,
                    levelID, // GF1
                    categoryID, // Sonic
                    5
                );
                /*Level level = Program.srb2Game.Levels.ToArray()[1];
                IEnumerable<Run> characterRuns = level.Runs.Where(run => run.Category.Name.ToLower() == character.ToLower()); //tolower is to make it case insensitive
                Run[] verifiedRuns = characterRuns.Where(run => run.Status.Type == RunStatusType.Verified).OrderBy(run => run.Times.GameTime.Value).ToArray();
                for (int i = 0; i < leaderboard.Records.Count(); i++)
                {
                    await ctx.RespondAsync($"{i + 1}. {leaderboard.Records[i].Player.Name} | {leaderboard.Records[i].Times.GameTime.Value.ToString(Program.timeFormat)} : {leaderboard.Records[i].WebLink.AbsoluteUri}");
                }
                */

                var records = new DiscordEmbedBuilder
                {
                    Title = leaderboard.Level.Name + " | " + leaderboard.Category.Name,

                    Url = leaderboard.WebLink.AbsoluteUri
                };

                switch(character.ToLower()) {
                    case "sonic":
                        records.Color = DiscordColor.Blue;
                        break;

                    case "tails":
                        records.Color = DiscordColor.Yellow;
                        break;

                    case "knuckles":
                        records.Color = DiscordColor.Red;
                        break;

                    case "amy":
                        records.Color = DiscordColor.HotPink;
                        break;

                    case "fang":
                        records.Color = DiscordColor.Purple;
                        break;

                    case "metal":
                        records.Color = DiscordColor.DarkBlue;
                        break;
                }
                for (int i = 0; i < leaderboard.Records.Count(); i++)
                {
                    records.AddField($"{i + 1}. {leaderboard.Records[i].Player.Name} | {leaderboard.Records[i].Times.GameTime.Value.ToString(Program.timeFormat)} ", 
                    leaderboard.Records[i].WebLink.AbsoluteUri);

                   
                }

                await ctx.RespondAsync(embed: records);

            }

        }

    }
}