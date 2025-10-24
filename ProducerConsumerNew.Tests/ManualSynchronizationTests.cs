using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace ProducerConsumerNew.Tests
{
    public class ManualSynchronizationTests
    {
        private readonly Queue<int> sharedQueue = new Queue<int>();
        private readonly object queueLock = new object();

        [Fact]
        public void Lock_ProvidesMutualExclusion()
        {
            // Arrange
            int counter = 0;
            int iterations = 1000;
            var threads = new Thread[5];

            // Act
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(() =>
                {
                    for (int j = 0; j < iterations; j++)
                    {
                        lock (queueLock)
                        {
                            counter++;
                        }
                    }
                });
                threads[i].Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            // Assert
            Assert.Equal(threads.Length * iterations, counter);
        }

        [Fact]
        public void Interlocked_ProvidesAtomicIncrement()
        {
            // Arrange
            int counter = 0;
            int iterations = 10000;
            var threads = new Thread[10];

            // Act
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(() =>
                {
                    for (int j = 0; j < iterations; j++)
                    {
                        Interlocked.Increment(ref counter);
                    }
                });
                threads[i].Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            // Assert
            Assert.Equal(threads.Length * iterations, counter);
        }
    }
}
