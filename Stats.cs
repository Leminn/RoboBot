﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using SpeedrunComSharp;

namespace RoboBot
{
    public class Stats
    {
        private Random r = new Random();
        public string RandomStat()
        {
            switch (r.Next(1, 6))
            {
                case 1:
                    return $"There have been {TotalRunsCount} runs submitted to SRC.";

                case 2:
                    return $"There have been {FullRunsCount} full-game runs submitted to SRC.";

                case 3:
                    return $"There have been {LevelRunsCount} individual-level runs submitted to SRC.";

                case 4:
                    return $"{(int)TotalTime.TotalHours}:{TotalTime.ToString(Program.timeFormatWithMinutes)} is the combined time of all records for SRB2.";

                case 5:
                    return $"{LatestRun}";
            }
            return "";
        }

        private SpeedrunComClient Client;
        private Timer timer = new Timer(216000000) { AutoReset = true, Enabled = true }; //an hour

        private int LevelRunsCount { get; set; }
        private int FullRunsCount { get; set; }
        private int TotalRunsCount { get; set; }
        private TimeSpan TotalTime { get; set; }
        private string LatestRun { get; set; }

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

            Run latest = runs[runs.Count - 1];

            string displayedTimeFormat = latest.Times.Primary.Value.Hours != 0 ? Program.timeFormatWithHours : (latest.Times.Primary.Value.Minutes != 0 ? Program.timeFormatWithMinutes : Program.timeFormat);

            LatestRun = $"{latest.Times.Primary.Value.ToString(displayedTimeFormat)} on {latest.Level.Name} by {latest.Player.Name} is the latest record!";
            TotalTime = totalTime;
            FullRunsCount = fullRunsCount;
            LevelRunsCount = levelRunsCount;
            TotalRunsCount = fullRunsCount + levelRunsCount;

            Console.WriteLine(string.Join('\n', TotalTime.Days*24 + TotalTime.Hours + ":" + TotalTime.ToString(@"mm\:ss\.ff"), FullRunsCount, LevelRunsCount, TotalRunsCount));
        }
    }
}