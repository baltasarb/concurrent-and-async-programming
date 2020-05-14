namespace CrossCutting
{
    using System;

    public class Timer
    {
        private readonly DateTime expirationTime;

        public Timer(int duration)
        {
            this.expirationTime = DateTime.UtcNow.AddMilliseconds(duration);
        }

        public bool IsExpired()
        {
            var currentTime = DateTime.UtcNow;

            return currentTime.CompareTo(expirationTime) >= 0;
        }

        public int GetTimeToWait()
        {
            var timeSpan = expirationTime - DateTime.UtcNow;
            return (int)Math.Floor(timeSpan.TotalMilliseconds);
        }
    }
}
