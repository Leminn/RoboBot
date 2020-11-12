using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Threading.Tasks;
using System.Configuration;
using System.Linq;
using System.Drawing;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Net;
using SpeedrunComSharp;
using RoboBot_SRB2;

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
            commandList.AddField("Special Stages", "Floral fields-Egg Satellite are abbreviated as ss1-ss7.");

            await ctx.RespondAsync(embed: commandList);
        } 

        [Command("records")]
        public async Task Records(CommandContext ctx, string level,  string character){
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
                    else if(!gotCat)
                    {
                        categoryID = "xd1g1j4d";
                        gotCat = true;
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

                    DiscordEmbedBuilder.EmbedThumbnail thumbnailUrl = new DiscordEmbedBuilder.EmbedThumbnail();
                    thumbnailUrl.Url = "http://77.68.95.193/lvlicons/" + srb2Level.FullName.Replace(" ", string.Empty)  + ".png";
                    DiscordEmbedBuilder.EmbedFooter embedFooter = new DiscordEmbedBuilder.EmbedFooter();
                    embedFooter.Text = "Last updated " + (DateTime.Now - Program.startedAt).Minutes + " minute(s) ago";
                    embedFooter.IconUrl = "https://wiki.srb2.org/w/images/f/f3/Emblem-Time.png";

                    var records = new DiscordEmbedBuilder
                    {
                        Title = srb2Level.FullName,
                        Thumbnail = thumbnailUrl,
                        Footer = embedFooter,
                        Url = leaderboard.WebLink.AbsoluteUri
                    };

                    switch(leaderboard.Category.Name){
                        
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
            catch(Exception e)
            {
                await ctx.RespondAsync(e.Source);
                await ctx.RespondAsync(e.Message);
                //await ctx.RespondAsync(e.InnerException.Message);
                await ctx.RespondAsync(e.StackTrace); 


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