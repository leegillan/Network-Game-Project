using System;
using System.Data;
using System.Threading;

namespace CMP303Game
{
    class Program
    {
        private static bool isRunning = false;

        static void Main(string[] args)
        {
            Console.Title = "CMP303 Network";

            isRunning = true;

            Thread mainThread = new Thread(new ThreadStart(MainThread));

            mainThread.Start();

            Server.Start(5, 26950);
        }

        private static void MainThread()
        {
            Console.WriteLine($"Main Thread has started. Running at {Constants.TICKS_PER_SEC} ticks per second.");

            DateTime nextLoop = DateTime.Now;

            while(isRunning)
            {
                while(nextLoop < DateTime.Now)
                {
                    GameLogic.Update();

                    nextLoop = nextLoop.AddMilliseconds(Constants.MS_PER_TICK);
        
                    //CPU usage was high due to inbetween ticks the thread has nothin gto do which causes it to use unexpected high amount of usage power
                    if(nextLoop > DateTime.Now)
                    {
                        Thread.Sleep(nextLoop - DateTime.Now);
                    }
                
                }
            }
        }
    }
}
