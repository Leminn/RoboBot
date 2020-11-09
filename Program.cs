using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks; 
using System.Configuration;
using System.Net;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.Configuration;
using SpeedrunComSharp;

namespace RoboBot
{
    class Program
    {
        static DiscordClient discord;
        static CommandsNextModule commands;

    

        static void Main(string[] args)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            
            discord = new DiscordClient(new DiscordConfiguration 
            { 

            Token = ConfigurationManager.AppSettings["APIKey"],
            TokenType = TokenType.Bot,
            UseInternalLogHandler = true,
            LogLevel = LogLevel.Debug
            }); 

            discord.MessageCreated += async e =>
            {
                if (e.Message.Content.ToLower().StartsWith("ping"))
                    await e.Message.RespondAsync("pong!");
                else if(e.Message.Content.ToLower().StartsWith("peas"))
                    await e.Message.RespondAsync(":duck:");
            };

            commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefix = ";;"
            });

            commands.RegisterCommands<MyCommands>();


            await discord.ConnectAsync();
            await Task.Delay(-1);

            
        }

    }
}
