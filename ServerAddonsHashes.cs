using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace RoboBot;

public static class ServerAddonHashes
{
    private const string AddonsDirectory = AddonsCommands.AddonsRootPath;

    private static DateTime _lastRefreshTime = DateTime.Now;

    private static FileSystemWatcher _fsWatcher;
    
    public static readonly Dictionary<string, FileInfo> Addons = new();

    public static void StartWatchingAddonChanges()
    {
        _fsWatcher = new FileSystemWatcher(AddonsDirectory)
        {
            EnableRaisingEvents = true,
            IncludeSubdirectories = true,
            InternalBufferSize = 64000
        };
        
        _fsWatcher.Changed += AddonsWereModified;
        _fsWatcher.Created += AddonsWereModified;
        _fsWatcher.Deleted += AddonsWereModified;
        _fsWatcher.Renamed += AddonsWereModified;
        _fsWatcher.Error += AddonsWatcherErrored;

        Console.WriteLine("Addon changes watcher started");
        
        Task.Run(RefreshAddons);
    }

    public static void StopWatchingAddonChanges()
    {
        _fsWatcher.Changed -= AddonsWereModified;
        _fsWatcher.Created -= AddonsWereModified;
        _fsWatcher.Deleted -= AddonsWereModified;
        _fsWatcher.Renamed -= AddonsWereModified;
        _fsWatcher.Error -= AddonsWatcherErrored;
        
        _fsWatcher.Dispose();
    }
    
    public static string GetFileMD5(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        
        return string.Concat(MD5.Create().ComputeHash(stream).Select(x => x.ToString("X")));
    }
    
    private static void AddonsWatcherErrored(object sender, ErrorEventArgs args)
    {
        Console.WriteLine("Addon changes watcher has errored, restarting it...");
        _fsWatcher.Dispose();
        Task.Run(StartWatchingAddonChanges);
    }

    private static void AddonsWereModified(object sender, FileSystemEventArgs e)
    {
        if (_lastRefreshTime.Add(TimeSpan.FromSeconds(5)) < DateTime.Now)
            RefreshAddons();
    }

    private static void RefreshAddons()
    {
        Addons.Clear();
        
        foreach (string addonFile in Directory.EnumerateFiles(AddonsDirectory, "*", SearchOption.AllDirectories))
        {
            FileInfo info = new FileInfo(addonFile);
            string md5 = GetFileMD5(info.FullName);
            
            if (!Addons.TryAdd(md5, info))
                Console.WriteLine("Warning: couldn't add " + info + " to the addons hashes list, is it a duplicate?");
        }

        Console.WriteLine("Addons refreshed");
        _lastRefreshTime = DateTime.Now;
    }
}