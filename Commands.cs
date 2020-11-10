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

        [Command("test")]
        public async Task test(CommandContext ctx)
        {
            try
            { 
                await ctx.RespondAsync(Program.srb2Game.Name);
             /*   foreach(var category in srb2Game.Categories)
                {
                    if(prevCategory == category){

                    }
                } */
            }
            catch (Exception e)
            {
                 Console.WriteLine(e.Message);
            }
        } //var lol = Program.srcClient.Games.something

/*
*/
        [Command("GFZ1")]
        public async Task Greenflower1Records(CommandContext ctx, string character){
            bool gotLvl = Enums.levelsID.TryGetValue(ctx.Command.Name, out string levelID);
            
            bool gotCat = Enums.categoriesID.TryGetValue(character.ToLower(), out string categoryID);

            if(gotCat == false || gotLvl == false)
            {
                await ctx.RespondAsync(ctx.Command.Arguments[0].Name);
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
                    Color = DiscordColor.Green,
                    Url = leaderboard.WebLink.AbsoluteUri
                };
                for (int i = 0; i < leaderboard.Records.Count(); i++)
                {
                    records.AddField($"{i + 1}. {leaderboard.Records[i].Player.Name} | {leaderboard.Records[i].Times.GameTime.Value.ToString(Program.timeFormat)} ", 
                    leaderboard.Records[i].WebLink.AbsoluteUri);

                   
                }

                await ctx.RespondAsync(embed: records);

            }

        }

        [Command("GFZ2")]
        public async Task Greenflower2Records(CommandContext ctx, string character)
        { 

            Leaderboard leaderboard = Program.srcClient.Leaderboards.GetLeaderboardForLevel(
                Program.srb2Game.ID,
                "69zq08x9", // GF2
                "xd1g1j4d", // Sonic
                5                         
            );
            /*Level level = Program.srb2Game.Levels.ToArray()[1];
            IEnumerable<Run> characterRuns = level.Runs.Where(run => run.Category.Name.ToLower() == character.ToLower()); //tolower is to make it case insensitive
            Run[] verifiedRuns = characterRuns.Where(run => run.Status.Type == RunStatusType.Verified).OrderBy(run => run.Times.GameTime.Value).ToArray();*/
            for (int i = 0; i < leaderboard.Records.Count(); i++)
            {
                await ctx.RespondAsync($"{i + 1}. {leaderboard.Records[i].Player.Name} | {leaderboard.Records[i].Times.GameTime.Value.ToString(Program.timeFormat)} : {leaderboard.Records[i].WebLink.AbsoluteUri}");
            }
        }

    }
}