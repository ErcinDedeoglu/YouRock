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
        }

        public static void ExecuteThread<T1,T2>(Action<T1, T2> action, T1 t1, T2 t2)
        {
            Thread bigStackThread = new Thread(() => action(t1, t2), 1024 * 1024);
            bigStackThread.Start();
        }

        public static void ExecuteThread<T1, T2, T3>(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3)
        {
            Thread bigStackThread = new Thread(() => action(t1, t2, t3), 1024 * 1024);
            bigStackThread.Start();
        }

        public static void ExecuteThread(Action action)
        {
            try
            {
                Thread bigStackThread = new Thread(() => action(), 1024 * 1024);
                bigStackThread.Start();
            }
            catch
            {
                // ignored
            }
        }
    }
}