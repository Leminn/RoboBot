using System.Collections.Generic;

namespace RoboBot_SRB2
{
    public class SRB2Level
    {
        public string SrcID { get; set; }
        public string FullName { get; set; }
        public string MapName { get; set; }

        public SRB2Level(string srcID, string fullName, string mapName)
        {
            this.FullName = fullName;
            this.SrcID = srcID;
            this.MapName = mapName;
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
            {"APZ", new SRB2Level("xd4y8eqd", "Alpine Paradise Zone Act 1", "MAP72")},
            {"APZ1", new SRB2Level("xd4y8eqd", "Alpine Paradise Zone Act 1", "MAP72")},
            {"APZ2", new SRB2Level("xd02onm9", "Alpine Paradise Zone Act 2", "MAP73")}
        };

        public static Dictionary<string, string> categoriesID = new Dictionary<string, string>()
        {
            {"sonic", "xd1g1j4d"},
            {"s", "xd1g1j4d"},

            {"tails", "zd3wvjvk"},
            {"t", "zd3wvjvk"},

            {"knuckles", "9kvoq882"},
            {"knux", "9kvoq882"},
            {"k", "9kvoq882"},

            {"amy", "rkll95nk"},
            {"a", "rkll95nk"},

            {"fang", "ndx9e3rd"},
            {"f", "ndx9e3rd"},

            {"metal", "w20g8q5k"},
            {"m", "w20g8q5k"}
        };

        public static Dictionary<string, string> fgCategoriesID = new Dictionary<string, string>()
        {
            {"sonic", "ndx46012"},
            {"s", "ndx46012"},

            {"tails", "w20375v2"},
            {"t", "w20375v2"},

            {"knuckles", "wdm637xk"},
            {"knux", "wdm637xk"},
            {"k", "wdm637xk"},

            {"amy", "vdo1ln12"},
            {"a", "vdo1ln12"},

            {"fang", "wkp3xr02"},
            {"f", "wkp3xr02"},

            {"metal", "7dg6j7xk"},
            {"m", "7dg6j7xk"},

            {"srb1", "9d8pm0qk"},
            {"srb1remake", "9d8pm0qk"},

            {"100%", "9d8pmg3k"},
            {"200", "9d8pmg3k"},
            {"emblem", "9d8pmg3k"},
            {"emblems", "9d8pmg3k"},
            {"allemblems", "9d8pmg3k"}
        };

        public static Dictionary<string, string> versions = new Dictionary<string, string>()
        {
            {"current", "2.2 Current" },
            {"2.2 current", "2.2 Current"},
            {"2.2", "2.2 Current"},
            {"2.2.5", "2.2 Current"},
            {"2.2.6", "2.2 Current"},
            {"2.2.7", "2.2 Current"},
            {"2.2.8", "2.2 Current"},
            {"2.2.9", "2.2 Current"},
            {"2.2.10", "2.2 Current"},

            {"2.2 legacy", "2.2.4"},
            {"2.2.0", "2.2.4"},
            {"2.2.1", "2.2.4"},
            {"2.2.2", "2.2.4"},
            {"2.2.3", "2.2.4"},
            {"2.2.4", "2.2.4"},

            {"2.1", "2.1.X"},
            {"2.1.X", "2.1.X"},
            {"2.1.25", "2.1.X"}
        };

        public static string GetGoal(string parameter)
        {
            if (parameter == null)
            {
                return "Any%";
            }

            string goal = parameter.ToLower();

            switch (goal)
            {
                case "any":
                    return "Any%";

                case "any%":
                    return "Any%";

                case "emeralds":
                    return "All Emeralds";

                case "all":
                    return "All Emeralds";

                case "allemeralds":
                    return "All Emeralds";

                case "ultimate":
                    return "Ultimate";

                case "ult":
                    return "Ultimate";

                case "srb1":
                    return "SRB1 Remake";

                case "remake":
                    return "SRB1 Remake";

                case "srb1remake":
                    return "SRB1 Remake";

                case "9d8pmg3k":
                    return "All Emblems";

                default:
                    return "Any%";
            }
        }
    }
}