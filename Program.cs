using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks; 
using System.Configuration;
using System.Collections.Generic;
using System.Net;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.Configuration;
using SpeedrunComSharp;

namespace RoboBot
{
    #region Level and Characters Enums
    public static class SRB2Enums
    {
        public static Dictionary<string, string> levelsID = new Dictionary<string, string>()
        {
            {"GFZ1", "ywejpqld"},
            {"GFZ2", "69zq08x9"},
            {"GFZ3", "r9gj87j9"},

            {"THZ1", "o9xz0x3w"},
            {"THZ2", "495rnmmd"},
            {"THZ3", "rdq3qnow"},

            {"DSZ1", "5d7er2qw"},
            {"DSZ2", "kwj8rv09"},
            {"DSZ3", "owoex3jd"},

            {"CEZ1", "xd1314z9"},
            {"CEZ2", "ewpz58y9"},
            {"CEZ3", "y9mrz2zw"},

            {"ACZ1", "5wknlqvw"},
            {"ACZ2", "5928p479"},
            {"ACZ3", "29vlpzqw"},

            {"RVZ1", "xd4y8kqd"},

            {"ERZ1", "xd02o4m9"},
            {"ERZ2", "rw6lympw"},

            {"BCZ1", "z98zl5r9"},
            {"BCZ2", "rdnlgx5w"},
            {"BCZ3", "ldy50kpw"},

            {"FHZ", "gdr87oe9"},

            {"PTZ", "nwlyezo9"},

            {"FFZ", "ywejp3ld"},

            {"TLZ", "69zq06x9"},

            {"HHZ", "r9gj8nj9"},

            {"AGZ", "o9xz0o3w"},

            {"ATZ", "495rnzmd"},

            {"SS1", "rdq3qoow"},

            {"SS2", "5d7erxqw"},

            {"SS3", "kwj8r609"},

            {"SS4", "owoex5jd"},

            {"SS5", "xd131yz9"},

            {"SS6", "ewpz5oy9"},

            {"SS7", "y9mrzgzw"},

            {"BHZ", "5wknlevw"},

            {"CCZ", "5928pr79"},

            {"DHZ", "29vlp0qw"},

            {"APZ1", "xd4y8eqd"},

            {"APZ2", "xd02onm9"}
        };
        public static Dictionary<string, string> categoriesID = new Dictionary<string, string>()
        {
            {"sonic", "xd1g1j4d"},
            {"tails", "zd3wvjvk"},
            {"knuckles", "9kvoq882"},
            {"amy", "rkll95nk"},
            {"fang", "ndx9e3rd"},
            {"metal", "w20g8q5k"}
        };
    }
    #endregion
    class Program
    {
        public static string timeFormat = @"mm\:ss\.ff";
        public static DiscordClient discord;
        static CommandsNextModule commands;
        
        public static Game srb2Game;
    
        public static SpeedrunComClient srcClient = new SpeedrunComClient() { AccessToken = ConfigurationManager.AppSettings["SRC_APIKey"] };

        static void Main(string[] args)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            srb2Game = srcClient.Games.GetGame("76ryx418");
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
                StringPrefix = "!"
            });

            commands.RegisterCommands<MyCommands>();


            await discord.ConnectAsync();
            await Task.Delay(-1);

            
        }

    }
}
