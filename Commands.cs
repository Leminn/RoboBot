using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Net;
using SpeedrunComSharp;


namespace RoboBot
{
    public class MyCommands
    {


        [Command("random")]
        public async Task Random(CommandContext ctx, int min, int max)
        {
            var rnd = new Random();
            await ctx.RespondAsync($"🎲 Your random number is: {rnd.Next(min, max)}");
        }

        [Command("test")]
        public async Task test(CommandContext ctx)
        {
            try
            {
                var client = new SpeedrunComClient();
                string gameID = "76ryx418";
                var srb2Game = client.Games.GetGame(gameID);
                await ctx.RespondAsync(srb2Game.Name);
                foreach( var category in srb2Game.Categories)
                {
                    await ctx.RespondAsync(category.Name);
                }
            }
            catch (Exception e)
            {
                 Console.WriteLine(e.Message);
            }
        } 
    }
}