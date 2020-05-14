namespace Series1._2_PacketBuilder
{
    using CrossCutting;
    using System.Collections.Generic;
    using System.Threading;

    class Packet<T>
    {
        public readonly object condition;

        public readonly int packetSize;
        public readonly List<T> messages;
        public bool hasWaiter;

        public Packet(int packetSize)
        {
            this.packetSize = packetSize;
            this.condition = new object();
            this.messages = new List<T>();
            this.hasWaiter = false;
        }

        public bool IsComplete()
        {
            return this.messages.Count == this.packetSize;
        }

        public void Await(object monitor, int timeToWait)
        {
            MonitorUtilities.EnterUninterruptedly(this.condition, out _);

            Monitor.Exit(monitor);

            Monitor.Wait(this.condition, timeToWait);

            MonitorUtilities.EnterUninterruptedly(monitor, out _);
        }

        public void NotifyWaiter()
        {
            MonitorUtilities.EnterUninterruptedly(this.condition, out _);
            Monitor.Pulse(this.condition);
            Monitor.Exit(this.condition);
        }
    }
}
