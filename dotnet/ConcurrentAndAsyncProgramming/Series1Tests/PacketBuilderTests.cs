namespace Series1Tests
{
    using Series1._2_PacketBuilder;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using NUnit.Framework;

    [TestFixture()]
    public class PacketBuilderTests
    {

        [Test()]
        public void RandomProducersAndConsumers()
        {
            //Arrange
            int packetSize = 20;
            int numberOfConsumers = 1000;
            int numberOfProducers = numberOfConsumers * packetSize;
            int timeout = int.MaxValue;
            var builder = new PacketBuilder<string>(packetSize);
            var monitor = new object();

            List<Thread> producers = new List<Thread>(numberOfProducers);
            for (int i = 0; i < numberOfProducers; i++)
            {
                var ii = i;
                producers.Add(new Thread(() => builder.PutMessage(ii.ToString())));
            }

            List<Thread> consumers = new List<Thread>(numberOfConsumers);
            List<List<string>> results = new List<List<string>>(numberOfConsumers);
            for (int i = 0; i < numberOfConsumers; i++)
            {
                var consumerResults = new List<string>(packetSize);
                consumers.Add(new Thread(() =>
                {
                    consumerResults = builder.TakeMessagePacket(timeout);

                    lock (monitor)
                    {
                        results.Add(consumerResults);

                        Console.WriteLine("Added number: {0}", results.Count);
                        if (results.Count == numberOfConsumers)
                        {
                            Console.WriteLine("Pulse");

                            Monitor.Pulse(monitor);
                        }
                    }

                }));
            }

            var totalThreads = numberOfConsumers + numberOfProducers;
            var randomizer = new Random();
            for (int i = 0; i < totalThreads; i++)
            {
                var randomNumber = randomizer.Next(0, 2);
                if (randomNumber == 0 && consumers.Count > 0 || producers.Count == 0)
                {
                    StartThread(consumers);
                }
                else
                {
                    StartThread(producers);
                }
            }

            lock (monitor)
            {
                if (results.Count != numberOfConsumers)
                {
                    Monitor.Wait(monitor);
                }
            }

            var a = results
                    .SelectMany(r => r)
                    .Select(str => Int32.Parse(str))
                    .ToList();

            a.Sort();

            for (int i = 0; i < results.Count; i++)
            {
                Assert.AreEqual(i, a[i]);
            }
        }

        private void StartThread(List<Thread> workers)
        {
            var worker = workers.First();
            workers.Remove(worker);
            worker.Start();
        }
    }
}

