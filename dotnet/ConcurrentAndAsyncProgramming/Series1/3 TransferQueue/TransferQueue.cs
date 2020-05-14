namespace Series1._3_TransferQueue
{
    using CrossCutting;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    class TransferQueue<E> where E : class
    {
        private readonly object monitor;
        private readonly IList<Message<E>> consumers;
        private readonly IList<Message<E>> producers;

        public TransferQueue()
        {
            this.monitor = new object();
            this.consumers = new List<Message<E>>();
            this.producers = new List<Message<E>>();
        }
        public void Put(E body)
        {
            MonitorUtilities.EnterUninterruptedly(this.monitor, out _);
            var message = new Message<E>(body);
            producers.Add(message); ;
            Monitor.Exit(this.monitor);
        }

        public bool Transfer(E body, int timeout)
        {
            Message<E> message = default;

            try
            {
                MonitorUtilities.EnterUninterruptedly(monitor, out _);

                message = this.consumers.First();

                if (message != default)
                {
                    this.consumers.Remove(message);
                    Monitor.Exit(this.monitor);
                    message.body = body;
                    message.NotifyWaiter();
                    return true;
                }

                message = new Message<E>(body, condition: new object());
                this.producers.Add(message);

                var timer = new CrossCutting.Timer(timeout);

                while (true)
                {
                    Monitor.Wait(timeout);

                    if (message.body != default)
                    {
                        Monitor.Exit(this.monitor);
                        return true;
                    }

                    if (timer.IsExpired())
                    {
                        this.producers.Remove(message);
                        Monitor.Exit(this.monitor);
                        return false;
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
                if (message?.body != default)
                {
                    Monitor.Exit(this.monitor);
                    Thread.CurrentThread.Interrupt();
                    return true;
                }
                Monitor.Exit(this.monitor);
                throw;
            }
        }

        public E Take(int timeout)
        {
            Message<E> message = default;
            try
            {
                MonitorUtilities.EnterUninterruptedly(monitor, out _);

                message = this.producers.First();

                if (message != default)
                {
                    this.producers.Remove(message);
                    Monitor.Exit(this.monitor);

                    //condition might be default if it was added to the producers queue by Put()
                    if (message.condition != default)
                    {
                        message.NotifyWaiter();
                    }
                    return message.body;
                }

                message = new Message<E>(default, condition: new object());
                this.consumers.Add(message);

                var timer = new CrossCutting.Timer(timeout);

                while (true)
                {
                    Monitor.Wait(timeout);

                    if (message.body != default)
                    {
                        Monitor.Exit(this.monitor);
                        return message.body;
                    }

                    if (timer.IsExpired())
                    {
                        this.consumers.Remove(message);
                        Monitor.Exit(this.monitor);
                        return null;
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
                if (message?.body != default)
                {
                    Monitor.Exit(this.monitor);
                    Thread.CurrentThread.Interrupt();
                    return message.body;
                }
                Monitor.Exit(this.monitor);
                throw;
            }
        }
    }
}
