using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using SpeedrunComSharp;

namespace RoboBot
{
    public class Stats
    {
        private SpeedrunComClient Client;
        private Timer timer = new Timer(216000000) { AutoReset = true, Enabled = true }; //an hour

        public int LevelRunsCount { get; private set; }
        public int FullRunsCount { get; private set; }
        public int TotalRunsCount { get; private set; }
        public TimeSpan TotalTime { get; private set; }

        public Stats(ref SpeedrunComClient client)
        {
            Client = client;
            AnHourHasPassed(null, null);
            timer.Elapsed += AnHourHasPassed;
        }

        private void AnHourHasPassed(object sender, ElapsedEventArgs e)
        {
            List<Run> runs = Client.Runs.GetRuns(gameId: Program.gameId, elementsPerPage: 200, status:RunStatusType.Verified, orderBy:RunsOrdering.Date).ToList();

            int fullRunsCount = 0, levelRunsCount = 0;
            TimeSpan totalTime = new TimeSpan();
            Console.WriteLine(runs[runs.Count - 1].WebLink);
            foreach (Run run in runs)
            {
                if(run.Times.RealTime.HasValue)
                {
                    totalTime += run.Times.RealTime.Value;
                }
                else
                {
                    totalTime += run.Times.GameTime.Value;
                }

                if (run.LevelID == null)
                {
                    fullRunsCount++;
                }
                else
                {
                    levelRunsCount++;
                }
            }

            TotalTime = totalTime;
            FullRunsCount = fullRunsCount;
            LevelRunsCount = levelRunsCount;
            TotalRunsCount = fullRunsCount + levelRunsCount;

            Console.WriteLine(string.Join('\n', TotalTime.Days*24 + TotalTime.Hours + ":" + TotalTime.ToString(@"mm\:ss\.ff"), FullRunsCount, LevelRunsCount, TotalRunsCount));
        }
    }
}