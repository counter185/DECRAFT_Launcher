using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DECRAFTUpdater
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            try
            {
                string[] releaseInfo = { "-1" };
                Directory.CreateDirectory("./config");
                if (File.Exists("./config/_launcher_release_info"))
                {
                    releaseInfo = File.ReadAllLines("./config/_launcher_release_info");
                }
                int revisionNumber = int.Parse(releaseInfo[0]);
                WebClient client = new WebClient();
                string[] releaseData = client.DownloadString("https://raw.githubusercontent.com/counter185/DECRAFT_Launcher/main/release-latest").Split('\n');
                if (int.Parse(releaseData[0]) > revisionNumber || args.Any(x=>x == "-force"))
                {
                    Console.WriteLine("New version available! Downloading...");
                    client.DownloadFile(releaseData[1], "./decraft-latest.zip");
                    
                    using (ZipArchive arc = ZipFile.Open("decraft-latest.zip", ZipArchiveMode.Read))
                    {
                        foreach (ZipArchiveEntry entry in arc.Entries)
                        {
                            if (entry.CompressedLength == 0)
                            {
                                Directory.CreateDirectory(entry.FullName);
                            }
                            if (File.Exists(entry.FullName))
                            {
                                if (entry.Name.EndsWith(".exe") && entry.Name.ToLower().Contains("decraft") && !entry.Name.ToLower().Contains("updater"))
                                {
                                    Console.WriteLine($"Killing {entry.Name.Substring(0, entry.Name.LastIndexOf('.'))}...");
                                    foreach (Process p in Process.GetProcessesByName(entry.Name.Substring(0, entry.Name.LastIndexOf('.'))))
                                    {
                                        p.Kill();
                                    }
                                    Thread.Sleep(500);
                                }
                                File.Delete(entry.FullName);
                            }
                            try
                            {
                                entry.ExtractToFile(entry.FullName);
                            } catch (Exception e)
                            {
                            }
                        }
                        //arc.ExtractToDirectory("./");
                    }
                    Console.WriteLine("Update complete.");
                    File.Delete("./config/_launcher_release_info");
                    File.WriteAllLines("./config/_launcher_release_info", new string[] { releaseData[0] });
                    Process.Start("DeCraftLauncher.exe");
                    Environment.Exit(0);
                }
                else
                {
                    Console.WriteLine("No new version available.");
                    Environment.Exit(2);
                }
            } catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                Environment.Exit(1);
            }
        }
    }
}
