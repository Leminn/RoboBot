using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RoboBot
{
    public enum ReplayStatus
    {
        Success,
        BadDemo,
        NoMap,
        GameError,
        UnhandledException
    }

    public class ReplayEventArgs : EventArgs
    {
        public ReplayStatus Status { get; }
        public string OutputPath { get; }
        
        public string ErrorMessage { get; }

        public ReplayEventArgs(ReplayStatus status, string outputPath, string errorMessage)
        {
            Status = status;
            OutputPath = outputPath;
            ErrorMessage = errorMessage;
        }
    }

    public class ReplayWorker
    {
        private Process GameProcess = new Process();

        public static readonly string HomeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        public static readonly string ExecutableFolder22 = Path.Combine(HomeDir, "SRB2");
        public static readonly string ExecutableFolder21 = Path.Combine(HomeDir, "SRB21");
        public static string ExecutableFolder = ExecutableFolder22;

        public static readonly string DataFolder22 = Path.Combine(HomeDir, ".srb2");
        public static readonly string DataFolder21 = Path.Combine(HomeDir, ".srb2", ".srb21");
        public static string DataFolder = DataFolder22;

        public static readonly string LogFilePath22 = Path.Combine(DataFolder, "latest-log.txt");
        public static readonly string LogFilePath21 = Path.Combine(DataFolder21, "log.txt");
        public string LogFilePath = LogFilePath22;

        public string[] CurrentGifFiles;
        public string[] PreviousGifFiles;
        public DirectoryInfo GifDir = new DirectoryInfo(Path.Combine(DataFolder22, "movies"));

        public delegate void ReplayEventHandler(object sender, ReplayEventArgs args);
        
        public event ReplayEventHandler Processed;

        private List<Tuple<JobInfo, string, string>> queue;
        private bool isStarted;

        public void StartProcessing()
        {
            isStarted = true;
            queue = new List<Tuple<JobInfo, string, string>>();
            Task replayProcessingTask = new Task(ReplayProcesssingLoop);
            replayProcessingTask.Start();
        }

        public void StopProcessing()
        {
            isStarted = false;
        }

        public void AddToQueue(JobInfo jobInfo, string replayPath, string outputPath)
        {
            queue.Add(Tuple.Create(jobInfo, replayPath, outputPath));
        }

        private async void ReplayProcesssingLoop()
        {
            while (isStarted)
            {
                if (queue.Count != 0)
                {
                    Tuple<JobInfo, string, string> queueElement = queue.First();
                    try
                    {
                        ProcessReplay(queueElement.Item1, queueElement.Item2, queueElement.Item3);
                    }
                    finally
                    {
                        queue.Remove(queueElement);
                    }
                }
                
                await Task.Delay(1000);
            }
        }

        private void RecordReplay(JobInfo replayInfo, string replayPath, string outputPath)
        {
            FileInfo[] gifInfos = GifDir.GetFiles().OrderBy(file => file.LastWriteTime).ToArray();
            PreviousGifFiles = new string[gifInfos.Length];
            for (int i = 0; i < gifInfos.Length; i++)
            {
                PreviousGifFiles[i] = gifInfos[i].Name;
            }

            try
            {
                byte[] bytes = File.ReadAllBytes(replayPath);
                
                if(bytes[12] == 202)
                {
                    ExecutableFolder = ExecutableFolder22;
                    DataFolder = DataFolder22;
                    LogFilePath = LogFilePath22;
                }
                else
                {
                    ExecutableFolder = ExecutableFolder21;
                    DataFolder = DataFolder21;
                    LogFilePath = LogFilePath21;
                }

                File.WriteAllBytes(Path.Combine(DataFolder, "replay", "downloaded.lmp"), bytes);

                string addons = "";
                
                if(replayInfo.HasAddons)
                {
                    addons = "-file ";
                    List<string> charactersDirectoryFiles = new List<string>(Directory.GetFiles("/root/.srb2/addons/Characters")
                        .Select(Path.GetFileName)); 
                    
                    List<string> levelsDirectoryFiles = new List<string>(Directory.GetFiles("/root/.srb2/addons/Levels")
                        .Select(Path.GetFileName));
                    foreach (var addonfname in replayInfo.AddonsFileNames)
                    {
                        string addonName = System.Text.Encoding.ASCII.GetString(addonfname);
                        var charMatch = charactersDirectoryFiles.Where(stringToCheck => stringToCheck == addonName);
                        if (charMatch.Any())
                        {
                            addons += "Characters/";
                        };
                        var levelMatch = levelsDirectoryFiles.Where(stringToCheck => stringToCheck == addonName);
                        if (levelMatch.Any())
                        {
                            addons += "Levels/";
                        };
                        addons += addonName + ' ';
                    }
                }

                GameProcess.StartInfo.Arguments = "./reptovid -home /root -playdemo replay/downloaded.lmp  " + addons + " -force -- :1";
                GameProcess.StartInfo.FileName = "xinit";
                GameProcess.StartInfo.WorkingDirectory = ExecutableFolder;
                
                GameProcess.Start();

                bool nomap = false;
                string errorMessage = "";
                do
                {
                    Thread.Sleep(2000);

                    string logFile = File.ReadAllText(LogFilePath);  
                    
                    if (logFile.Contains("I_Error()"))
                    {
                        nomap = true;
                        //File.WriteAllText(LogFilePath, "a");
                        try { GameProcess.Kill(true); }
                        catch { }
                    }
                    
                    if (logFile.Contains("Process killed by signal: "))
                    {
                        errorMessage = logFile.Split("Process killed by signal: ")[1];
                       // File.WriteAllText(LogFilePath, "a");
                        try { GameProcess.Kill(true); }
                        catch { }
                    }
                    
                }
                while (!GameProcess.HasExited);

                gifInfos = GifDir.GetFiles().OrderBy(file => file.LastWriteTime).ToArray();
                CurrentGifFiles = new string[gifInfos.Length];
                for (int i = 0; i < gifInfos.Length; i++)
                {
                    CurrentGifFiles[i] = gifInfos[i].Name;
                }

                if (!CurrentGifFiles.SequenceEqual(PreviousGifFiles))
                {
                    string gifPath = gifInfos.Last().FullName;
                    
                    if(!string.IsNullOrEmpty(errorMessage))
                        InvokeEventAndCleanup(ReplayStatus.GameError, replayPath, gifPath, outputPath, errorMessage);
                    
                    InvokeEventAndCleanup(ReplayStatus.Success, replayPath, gifPath, outputPath);
                }
                else
                {
                    if(nomap)
                    {
                        InvokeEventAndCleanup(ReplayStatus.NoMap, replayPath);
                    }
                    else
                    {
                        InvokeEventAndCleanup(ReplayStatus.BadDemo, replayPath);
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                
                InvokeEventAndCleanup(ReplayStatus.UnhandledException, replayPath);
            }
        }

        private void ProcessReplay(JobInfo jobInfo, string replayPath, string outputPath)
        {
            FileInfo finfo = new FileInfo(replayPath);
            if (finfo.Extension.ToLower() == ".lmp")
            {
                RecordReplay(jobInfo, replayPath, outputPath);
            }
            else
            {
                InvokeEventAndCleanup(ReplayStatus.BadDemo, replayPath);
            }
        }

        private void InvokeEventAndCleanup(ReplayStatus status, string replayPath = "", string gifPath = "", string outputPath = "", string errorMessage = "")
        {
            if (!String.IsNullOrEmpty(replayPath) && File.Exists(replayPath))
                File.Delete(replayPath);
            if (!String.IsNullOrEmpty(gifPath) && File.Exists(gifPath))
            {
                if (status != ReplayStatus.GameError)
                    File.Move(gifPath, outputPath, true);
                File.Delete(gifPath);
            }

            Processed?.Invoke(this, new ReplayEventArgs(status, outputPath, errorMessage));
        }
    }
}