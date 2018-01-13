using System;
using System.Threading;

namespace YouRock
{
    public class ThreadHelper
    {
        public static void ExecuteThread<T>(Action<T> action, T parameterObject)
        {
            Thread bigStackThread = new Thread(() => action(parameterObject), 1024 * 1024);
            bigStackThread.Start();
            bigStackThread.Join();
        }

        public static void ExecuteThread(Action action)
        {
            Thread bigStackThread = new Thread(() => action(), 1024 * 1024);
            bigStackThread.Start();
            //bigStackThread.Join();
        }
    }
}