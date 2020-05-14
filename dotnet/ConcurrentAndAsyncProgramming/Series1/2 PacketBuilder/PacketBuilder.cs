namespace Series1._2_PacketBuilder
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Linq;
    using CrossCutting;

    public class PacketBuilder<T>
    {
        private readonly int packetSize;
        private readonly object monitor;
        private readonly List<Packet<T>> packets;

        public PacketBuilder(int packetSize)
        {
            this.packetSize = packetSize;
            this.monitor = new object();
            this.packets = new List<Packet<T>>();
        }

        public void PutMessage(T message)
        {
            MonitorUtilities.EnterUninterruptedly(this.monitor, out _);
            var packet = this.packets.FirstOrDefault(p => !p.IsComplete());

            if (packet == default)
            {
                packet = new Packet<T>(this.packetSize);
                this.packets.Add(packet);
            }

            packet.messages.Add(message);

            if (packet.hasWaiter && packet.IsComplete())
            {
                this.packets.Remove(packet);
                packet.NotifyWaiter();
            }
            Monitor.Exit(this.monitor);
        }

        public List<T> TakeMessagePacket(int timeout)
        {
            Packet<T> packet = default;

            try
            {
                MonitorUtilities.EnterUninterruptedly(this.monitor, out var interrupted);

                packet = this.packets.FirstOrDefault(existingPacket => existingPacket.IsComplete() && !existingPacket.hasWaiter);

                if (packet == default)
                {
                    packet = new Packet<T>(packetSize);
                    this.packets.Add(packet);
                }

                packet.hasWaiter = true;

                var timer = new CrossCutting.Timer(timeout);

                while (true)
                {
                    if (packet.IsComplete())
                    {
                        this.packets.Remove(packet);
                        return packet.messages;
                    }

                    if (timer.IsExpired())
                    {
                        packet.hasWaiter = false;
                        return null;
                    }

                    packet.Await(monitor, timer.GetTimeToWait());
                }
            }
            catch (ThreadInterruptedException)
            {
                if (packet.IsComplete())
                {
                    this.packets.Remove(packet);
                    Thread.CurrentThread.Interrupt();
                    return packet.messages;
                }

                packet.hasWaiter = false;

                throw;
            }
            finally
            {
                Monitor.Exit(monitor);
            }
        }
    }
}
