using DeCraftLauncher.UIControls.Popup;
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
        DateTime initialTime;

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
                    Start = (long)initialTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds
                }
            };
        }

        public void Init(MainWindow caller)
        {
            if (inited || !Process.GetProcessesByName("discord").Any()) {
                return;
            }
            initialTime = DateTime.UtcNow;
            try
            {
                discord = new Discord.Discord(1180408357782298624, (UInt64)Discord.CreateFlags.NoRequireDiscord);
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
            } catch (Discord.ResultException)
            {
                //don't fucking care lmao
            }
        }

        public void Close()
        {
            inited = false;
            if (rpcThread != null && rpcThread.IsAlive)
            {
                rpcThread.Abort();
            }
            if (discord != null)
            {
                if (activityManager != null)
                {
                    activityManager.ClearActivity((d) => { });
                }
                //this SDK sucks
                discord.RunCallbacks();
                discord.Dispose();
                discord = null;
            }
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
                var firstInstance = caller.runningInstances.First();
                act.Details = $"Ingame{(firstInstance.playerName != null ? $" - {caller.runningInstances.First().playerName}" : "")}";
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
