using System;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;

namespace RoboBot
{
    public enum ReplayStatus
    {
        Success,
        BadDemo,
        NoMap,
        UnhandledException
    }
    
    public class ReplayWorker
    {
        private static Process GameProcess = new Process();

        public readonly static string HomeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        public readonly static string ExecutableFolder22 = Path.Combine(HomeDir, "SRB2");
        public readonly static string ExecutableFolder21 = Path.Combine(HomeDir, "SRB21");
        public static string ExecutableFolder = ExecutableFolder22;

        public readonly static string DataFolder22 = Path.Combine(HomeDir, ".srb2");
        public readonly static string DataFolder21 = Path.Combine(HomeDir, ".srb2", ".srb21");
        public static string DataFolder = DataFolder22;

        public readonly static string LogFilePath22 = Path.Combine(DataFolder, "latest-log.txt");
        public readonly static string LogFilePath21 = Path.Combine(DataFolder21, "log.txt");
        public static string LogFilePath = LogFilePath22;

        public static string[] CurrentGifFiles;
        public static string[] PreviousGifFiles;
        public static DirectoryInfo GifDir = new DirectoryInfo(Path.Combine(DataFolder22, "movies"));

        public const int JobInfoLength = 202;
        
        private static Tuple<ReplayStatus, string> RecordReplay(JobInfo replayInfo, string replayPath)
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
                    foreach (var addonfname in replayInfo.AddonsFileNames)
                        addons += System.Text.Encoding.ASCII.GetString(addonfname) + " ";
                }

                GameProcess.StartInfo.Arguments = "./reptovid" + " -home /root" + " -playdemo " + "replay/downloaded.lmp " + addons + "-- :1";
                //GameProcess.StartInfo.FileName = Path.Combine(ExecutableFolder, "reptovid");
                GameProcess.StartInfo.FileName = "xinit";
                GameProcess.StartInfo.WorkingDirectory = ExecutableFolder;
                GameProcess.Start();

                bool nomap = false;
                do
                {
                    Thread.Sleep(2000);
                    if (File.ReadAllText(LogFilePath).Contains("I_Error()"))
                    {
                        nomap = true;
                        File.WriteAllText(LogFilePath, "a");
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
                    
                    return GetReturnValueAndCleanup(ReplayStatus.Success, replayPath, gifPath);
                }
                else
                {
                    if(nomap)
                    {
                        return GetReturnValueAndCleanup(ReplayStatus.NoMap, replayPath);
                    }
                    else
                    {
                        return GetReturnValueAndCleanup(ReplayStatus.BadDemo, replayPath);
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                
                return GetReturnValueAndCleanup(ReplayStatus.UnhandledException, replayPath);
            }
        }

        public static Tuple<ReplayStatus, string> ProcessReplay(JobInfo jobInfo, string replayPath, string outputDir)
        {
            FileInfo finfo = new FileInfo(replayPath);
            if (finfo.Extension.ToLower() == ".lmp")
            {
                return RecordReplay(jobInfo, replayPath);
            }
            else
            {
                return GetReturnValueAndCleanup(ReplayStatus.BadDemo, replayPath);
            }
        }

        private static Tuple<ReplayStatus, string> GetReturnValueAndCleanup(ReplayStatus status, string replayPath = "", string gifPath = "", string outputPath = "")
        {
            if(String.IsNullOrEmpty(replayPath) && File.Exists(replayPath))
                File.Delete(replayPath);
            /*if(String.IsNullOrEmpty(gifPath) && File.Exists(gifPath))
                File.Delete(gifPath);*/
            
            return Tuple.Create(status, outputPath);
        }
    }
}