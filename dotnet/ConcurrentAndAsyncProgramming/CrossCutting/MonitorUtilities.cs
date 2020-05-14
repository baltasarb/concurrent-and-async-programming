namespace CrossCutting
{
    using System.Threading;

    public static class MonitorUtilities
    {
        public static void EnterUninterruptedly(object monitor, out bool interrupted)
        {
            var lockTaken = interrupted = false;

            while (!lockTaken)
            {
                try
                {
                    Monitor.Enter(monitor, ref lockTaken);
                }
                catch (ThreadInterruptedException)
                {
                    interrupted = true;
                }
            }
        }

        private static void LockImp(object monitor)
        {
            bool lockTaken = false;

            try
            {
                Monitor.Enter(monitor, ref lockTaken);
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(monitor);
                }
            }
        }
    }
}
