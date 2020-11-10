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
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.Configuration;
using SpeedrunComSharp;

namespace RoboBot
{
    public class SRB2Level
    {
        public string SrcID { get; set; }
        public string FullName { get; set; }
        public string MapName { get; set; }

        public SRB2Level(string srcID, string fullName, string mapName)
        {
            FullName = fullName;
            SrcID = srcID;
            MapName = mapName;
        }

        public static int GetMapNumber(string mapName)
        {
            return int.Parse(mapName.Remove(0, 3));
        }
    }
    public static class SRB2Enums
    {
        public static Dictionary<string, SRB2Level> levelsID = new Dictionary<string, SRB2Level>()
        {
            {"GFZ1", new SRB2Level("ywejpqld", "Greenflower Zone Act 1", "MAP01")},
            {"GFZ2", new SRB2Level("69zq08x9", "Greenflower Zone Act 2", "MAP02")},
            {"GFZ3", new SRB2Level("r9gj87j9", "Greenflower Zone Act 3", "MAP03")},
            
            {"THZ1", new SRB2Level("o9xz0x3w", "Techno Hill Zone Act 1", "MAP04")},
            {"THZ2", new SRB2Level("495rnmmd", "Techno Hill Zone Act 2", "MAP05")},
            {"THZ3", new SRB2Level("rdq3qnow", "Techno Hill Zone Act 3", "MAP06")},

            {"DSZ1", new SRB2Level("5d7er2qw", "Deep Sea Zone Act 1", "MAP07")},
            {"DSZ2", new SRB2Level("kwj8rv09", "Deep Sea Zone Act 2", "MAP08")},
            {"DSZ3", new SRB2Level("owoex3jd", "Deep Sea Zone Act 3", "MAP09")},

            {"CEZ1", new SRB2Level("xd1314z9", "Castle Eggman Zone Act 1", "MAP10")},
            {"CEZ2", new SRB2Level("ewpz58y9", "Castle Eggman Zone Act 2", "MAP11")},
            {"CEZ3", new SRB2Level("y9mrz2zw", "Castle Eggman Zone Act 3", "MAP12")},

            {"ACZ1", new SRB2Level("5wknlqvw", "Arid Canyon Zone Act 1", "MAP13")},
            {"ACZ2", new SRB2Level("5928p479", "Arid Canyon Zone Act 2", "MAP14")},
            {"ACZ3", new SRB2Level("29vlpzqw", "Arid Canyon Zone Act 3", "MAP15")},

            {"RVZ1", new SRB2Level("xd4y8kqd", "Red Volcano Zone Act 1", "MAP16")},

            {"ERZ1", new SRB2Level("xd02o4m9", "Egg Rock Zone Act 1", "MAP22")},
            {"ERZ2", new SRB2Level("rw6lympw", "Egg Rock Zone Act 2", "MAP23")},

            {"BCZ1", new SRB2Level("z98zl5r9", "Black Core Zone Act 1", "MAP25")},
            {"BCZ2", new SRB2Level("rdnlgx5w", "Black Core Zone Act 2", "MAP26")},
            {"BCZ3", new SRB2Level("ldy50kpw", "Black Core Zone Act 3", "MAP27")},


            {"FHZ", new SRB2Level("gdr87oe9", "Frozen Hillside Zone", "MAP30")},
            {"PTZ", new SRB2Level("nwlyezo9", "Pipe Towers Zone", "MAP31")},
            {"FFZ", new SRB2Level("ywejp3ld", "Forest Fortress Zone", "MAP32")},
            {"TLZ", new SRB2Level("69zq06x9", "Techno Legacy Zone", "MAP33")},


            {"HHZ", new SRB2Level("r9gj8nj9", "Haunted Heights Zone", "MAP40")},
            {"AGZ", new SRB2Level("o9xz0o3w", "Aerial Garden Zone", "MAP41")},
            {"ATZ", new SRB2Level("495rnzmd", "Azure Temple Zone", "MAP42")},


            {"SS1", new SRB2Level("rdq3qoow", "Floral Field Zone", "MAP50")},
            {"SS2", new SRB2Level("5d7erxqw", "Toxic Plateau Zone", "MAP51")},
            {"SS3", new SRB2Level("kwj8r609", "Flooded Cove Zone", "MAP52")},
            {"SS4", new SRB2Level("owoex5jd", "Cavern Fortress Zone", "MAP53")},
            {"SS5", new SRB2Level("xd131yz9", "Dusty Wasteland Zone", "MAP54")},
            {"SS6", new SRB2Level("ewpz5oy9", "Magma Caves Zone", "MAP55")},
            {"SS7", new SRB2Level("y9mrzgzw", "Egg Satellite Zone", "MAP56")},
            {"BHZ", new SRB2Level("5wknlevw", "Black Hole Zone", "MAP57")},

            {"CCZ", new SRB2Level("5928pr79", "Christmas Chime Zone", "MAP70")},
            {"DHZ", new SRB2Level("29vlp0qw", "Dream Hill Zone", "MAP71")},
            {"APZ1", new SRB2Level("xd4y8eqd", "Alpine Paradise Zone Act 1", "MAP72")},
            {"APZ2", new SRB2Level("xd02onm9", "Alpine Paradise Zone Act 2", "MAP73")}
        };
        public static Dictionary<string, string> categoriesID = new Dictionary<string, string>()
        {
            {"sonic", "xd1g1j4d"},
            {"tails", "zd3wvjvk"},
            {"knuckles", "9kvoq882"},
            {"knux", "9kvoq882"},
            {"amy", "rkll95nk"},
            {"metal", "w20g8q5k"}
        };
    }
    class Program
    {
        public static string timeFormat = @"mm\:ss\.ff";
        public static DiscordClient discord;
        static CommandsNextExtension commands;
        
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
            TokenType = TokenType.Bot
            }); 

        

            commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { "!" },
                EnableDefaultHelp = false 
            });

            commands.RegisterCommands<MyCommands>();


            await discord.ConnectAsync();
            await Task.Delay(-1);
            
        }

    }
}
