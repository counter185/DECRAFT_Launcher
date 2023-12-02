using System;
using System.Collections.Generic;
using System.Data;
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

        public void Init(MainWindow caller)
        {
            discord = new Discord.Discord(1180408357782298624, (UInt64)Discord.CreateFlags.Default);
            activityManager = discord.GetActivityManager();
            rpcThread = new Thread (() =>
            {
                while (true)
                {
                    //activityMutex.WaitOne();
                    //activityMutex.ReleaseMutex();

                    //RunCallbacks has to be done on main thread
                    caller.Dispatcher.Invoke(delegate
                    {
                        discord.RunCallbacks();
                    });
                    Thread.Sleep(200);
                }
            });
            
            rpcThread.Start();
        }

        public void Close()
        {
            if (rpcThread.IsAlive)
            {
                rpcThread.Abort();
            }
        }

        public void ActivityIdle()
        {
            //activityMutex.WaitOne();
            Discord.Activity act = new Discord.Activity();
            act.Name = "DECRAFT";
            act.ApplicationId = 1180408357782298624;
            act.Details = "i am DECRAFTing so hard rn";
            activityManager.UpdateActivity(act, (a) =>
            {
                Console.WriteLine(a);
            });

            //activityMutex.ReleaseMutex();

        }
    }
}
