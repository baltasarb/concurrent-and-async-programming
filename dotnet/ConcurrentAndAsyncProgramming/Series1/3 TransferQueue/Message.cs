namespace Series1._3_TransferQueue
{
    using CrossCutting;
    using System.Threading;

    class Message<E>
    {
        public readonly object condition;

        public E body;

        public Message(E body = default, object condition = default)
        {
            this.body = body;
            this.condition = condition;
        }
        public void NotifyWaiter()
        {
            MonitorUtilities.EnterUninterruptedly(condition, out _);
            Monitor.Pulse(condition);
            Monitor.Exit(condition);
        }
    }
}
