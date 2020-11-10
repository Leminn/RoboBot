using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Threading.Tasks;
using System.Configuration;
using System.Linq;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Net;
using SpeedrunComSharp;
using Enums = RoboBot.SRB2Enums;

namespace RoboBot
{
    public class MyCommands : BaseCommandModule
    {
        
        
        [Command("help")]
        public async Task HelpSheet(CommandContext ctx)
        {
            var commandList = new DiscordEmbedBuilder
            {
                Title = "Help",
                Description = "Syntax : !records (level abreviation) (character)",
                Color = DiscordColor.Gold
            };
            commandList.AddField("Example","!records GFZ1 sonic = Greenflower Zone Act 1 Sonic");

            await ctx.RespondAsync(embed: commandList);
        } 

        [Command("records")]
        public async Task Records(CommandContext ctx, string level,  string character){
            bool gotLvl = Enums.levelsID.TryGetValue(level.ToUpper(), out SRB2Level srb2Level);

            bool gotCat = Enums.categoriesID.TryGetValue(character.ToLower(), out string categoryID);
            
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

            if(!gotCat)
            {
                    await ctx.RespondAsync("Wrong / Missing parameter: Character");
            }

            if(!gotLvl)
            {
                    await ctx.RespondAsync("Wrong / Missing parameter: Level");
            }

            if(gotCat == true && gotLvl == true)
            {
                Leaderboard leaderboard = Program.srcClient.Leaderboards.GetLeaderboardForLevel(
                    Program.srb2Game.ID,
                    srb2Level.SrcID,
                    categoryID,
                    5
                );

                var records = new DiscordEmbedBuilder
                {
                    Title = leaderboard.Level.Name,

                    Url = leaderboard.WebLink.AbsoluteUri
                };

                switch(leaderboard.Category.Name){
                    
                    case "Sonic":
                        records.Color = DiscordColor.Blue;
                        break;
                    case "Tails":
                        records.Color = DiscordColor.Yellow;
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
                }
                
                switch(nights)
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
                    records.AddField($"{i + 1}. {leaderboard.Records[i].Player.Name} | {leaderboard.Records[i].Times.GameTime.Value.ToString(Program.timeFormat)} ", 
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
        public async Task RecordsOnlyLevel(CommandContext ctx, string level)
        {
            await Records(ctx, level, "");
        }
        
        [Command("records")]
        public async Task Recordsoof(CommandContext ctx)
        {
            await ctx.RespondAsync("No parameters given\nType !help for more info");
        }
    }
}