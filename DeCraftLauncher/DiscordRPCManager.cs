using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using static Discord.ActivityManager.FFIMethods;

namespace DeCraftLauncher
{
    public class DiscordRPCManager
    {

        Discord.Discord discord;
        Discord.ActivityManager activityManager;
        Thread rpcThread;
        bool inited = false;

        public void Init(MainWindow caller)
        {
            if (inited || !Process.GetProcessesByName("discord").Any()) {
                return;
            }
            discord = new Discord.Discord(1180408357782298624, (UInt64)Discord.CreateFlags.Default);
            activityManager = discord.GetActivityManager();
            rpcThread = new Thread(() =>
            {
                while (true)
                {
                    //activityMutex.WaitOne();
                    //activityMutex.ReleaseMutex();

                    //RunCallbacks has to be done on main thread
                    caller.Dispatcher.Invoke(delegate
                    {
                        if (discord != null)
                        {
                            discord.RunCallbacks();
                        }
                    });
                    Thread.Sleep(200);
                }
            });

            rpcThread.Start();
            inited = true;
        }

        public void Close()
        {
            inited = false;
            if (rpcThread.IsAlive)
            {
                rpcThread.Abort();
            }
            if (discord != null)
            {
                discord.Dispose();
                discord = null;
            }
        }

        Discord.Activity BaseActivity
        {
            get => new Discord.Activity
        {
            Name = "DECRAFT",
            ApplicationId = 1180408357782298624,
            Assets = new Discord.ActivityAssets
            {
                LargeImage = "applogo"
            },
            Timestamps = new Discord.ActivityTimestamps
            {
                Start = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds
            }
        };
        }


        public void UpdateActivity(MainWindow caller)
        {
            if (!inited)
            {
                return;
            }
            Discord.Activity act = BaseActivity;
            if (caller.runningInstances.Count == 0)
            {
                act.State = "Idle";
            }
            else if (caller.runningInstances.Count == 1)
            {
                act.Details = "Ingame";
                act.State = $"Playing {caller.runningInstances.First().InstanceName}";
            }
            else
            {
                act.Details = "Ingame";
                act.State = $"{caller.runningInstances.Count} running instances";
            }

            activityManager.UpdateActivity(act, (a) =>
            {
                Console.WriteLine(a);
            });
        }
    }
}
