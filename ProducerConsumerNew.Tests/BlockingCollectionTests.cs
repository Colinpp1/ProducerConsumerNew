using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ProducerConsumerNew.Tests
{
    public class BlockingCollectionTests
    {
        [Fact]
        public void SingleProducer_SingleConsumer_AllItemsConsumed()
        {
            // Arrange
            var queue = new BlockingCollection<int>(boundedCapacity: 10);
            var producedItems = new List<int>();
            var consumedItems = new ConcurrentBag<int>();
            int itemCount = 20;

            // Act
            var producer = Task.Run(() =>
            {
                for (int i = 0; i < itemCount; i++)
                {
                    queue.Add(i);
                    producedItems.Add(i);
                }
                queue.CompleteAdding();
            });

            var consumer = Task.Run(() =>
            {
                foreach (var item in queue.GetConsumingEnumerable())
                {
                    consumedItems.Add(item);
                }
            });

            Task.WaitAll(producer, consumer);

            // Assert
            Assert.Equal(itemCount, producedItems.Count);
            Assert.Equal(itemCount, consumedItems.Count);
            Assert.Equal(producedItems.OrderBy(x => x), consumedItems.OrderBy(x => x));
        }

        [Fact]
        public void MultipleProducers_MultipleConsumers_AllItemsConsumed()
        {
            // Arrange
            var queue = new BlockingCollection<int>(boundedCapacity: 10);
            int producerCount = 3;
            int consumerCount = 2;
            int itemsPerProducer = 15;
            int totalItems = producerCount * itemsPerProducer;
            var consumedItems = new ConcurrentBag<int>();

            // Act
            var producers = Enumerable.Range(0, producerCount)
                .Select(id => Task.Run(() =>
                {
                    for (int i = 0; i < itemsPerProducer; i++)
                    {
                        queue.Add(id * 1000 + i);
                    }
                }))
                .ToArray();

            var consumers = Enumerable.Range(0, consumerCount)
                .Select(id => Task.Run(() =>
                {
                    foreach (var item in queue.GetConsumingEnumerable())
                    {
                        consumedItems.Add(item);
                    }
                }))
                .ToArray();

            Task.WaitAll(producers);
            queue.CompleteAdding();
            Task.WaitAll(consumers);

            // Assert
            Assert.Equal(totalItems, consumedItems.Count);
            Assert.Empty(queue);
        }

        [Fact]
        public void ThreadSafety_NoItemsLost_UnderConcurrentAccess()
        {
            // Arrange
            var queue = new BlockingCollection<int>(boundedCapacity: 50);
            int producerCount = 5;
            int itemsPerProducer = 100;
            int totalItems = producerCount * itemsPerProducer;
            var consumedItems = new ConcurrentBag<int>();

            // Act
            var producers = Enumerable.Range(0, producerCount)
                .Select(id => Task.Run(() =>
                {
                    for (int i = 0; i < itemsPerProducer; i++)
                    {
                        queue.Add(id * 10000 + i);
                        Thread.Sleep(1);
                    }
                }))
                .ToArray();

            var consumers = Enumerable.Range(0, 3)
                .Select(id => Task.Run(() =>
                {
                    foreach (var item in queue.GetConsumingEnumerable())
                    {
                        consumedItems.Add(item);
                        Thread.Sleep(1);
                    }
                }))
                .ToArray();

            Task.WaitAll(producers);
            queue.CompleteAdding();
            Task.WaitAll(consumers);

            // Assert
            Assert.Equal(totalItems, consumedItems.Count);
            Assert.Equal(totalItems, consumedItems.Distinct().Count());
        }
    }
}
