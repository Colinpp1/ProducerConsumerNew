using System;
using System.Collections.Generic;
using System.Threading;

namespace ProducerConsumerNew
{
    class Program
    {
        // Shared queue between producer and consumer
        private static readonly Queue<int> sharedQueue = new Queue<int>();
        
        // Lock object for queue synchronization
        private static readonly object queueLock = new object();
        
        // Lock for console output
        private static readonly object consoleLock = new object();
        
        // Configuration
        private const int MAX_QUEUE_SIZE = 10;
        private const int PRODUCER_COUNT = 3;
        private const int CONSUMER_COUNT = 2;
        private const int ITEMS_PER_PRODUCER = 20;
        
        // Flag to signal when production is complete
        private static bool productionComplete = false;
        private static int totalProduced = 0;
        private static int totalConsumed = 0;

        static void Main(string[] args)
        {
            Console.WriteLine("=== Producer-Consumer Pattern Demo ===\n");
            Console.WriteLine($"Configuration:");
            Console.WriteLine($"  Producers: {PRODUCER_COUNT}");
            Console.WriteLine($"  Consumers: {CONSUMER_COUNT}");
            Console.WriteLine($"  Items per Producer: {ITEMS_PER_PRODUCER}");
            Console.WriteLine($"  Max Queue Size: {MAX_QUEUE_SIZE}");
            Console.WriteLine($"  Total Items to Produce: {PRODUCER_COUNT * ITEMS_PER_PRODUCER}\n");
            Console.WriteLine("Starting...\n");

            // Create producer threads
            Thread[] producers = new Thread[PRODUCER_COUNT];
            for (int i = 0; i < PRODUCER_COUNT; i++)
            {
                int producerId = i + 1;
                producers[i] = new Thread(() => Producer(producerId))
                {
                    Name = $"Producer-{producerId}"
                };
                producers[i].Start();
            }

            // Create consumer threads
            Thread[] consumers = new Thread[CONSUMER_COUNT];
            for (int i = 0; i < CONSUMER_COUNT; i++)
            {
                int consumerId = i + 1;
                consumers[i] = new Thread(() => Consumer(consumerId))
                {
                    Name = $"Consumer-{consumerId}"
                };
                consumers[i].Start();
            }

            // Wait for all producers to finish
            foreach (Thread producer in producers)
            {
                producer.Join();
            }

            // Signal that production is complete
            lock (queueLock)
            {
                productionComplete = true;
                Monitor.PulseAll(queueLock); // Wake up all waiting consumers
            }

            // Wait for all consumers to finish
            foreach (Thread consumer in consumers)
            {
                consumer.Join();
            }

            // Display final statistics
            Console.WriteLine("\n=== Final Statistics ===");
            Console.WriteLine($"Total Produced: {totalProduced}");
            Console.WriteLine($"Total Consumed: {totalConsumed}");
            Console.WriteLine($"Queue Size: {sharedQueue.Count}");
            Console.WriteLine("\nAll threads completed successfully!");
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// Producer thread - generates random numbers and adds them to the queue
        /// </summary>
        static void Producer(int id)
        {
            Random random = new Random(id * 1000); // Seed based on ID for different sequences
            
            for (int i = 0; i < ITEMS_PER_PRODUCER; i++)
            {
                int number = random.Next(1, 1000);
                
                lock (queueLock)
                {
                    // Wait if queue is full
                    while (sharedQueue.Count >= MAX_QUEUE_SIZE)
                    {
                        LogMessage($"Producer-{id}", $"Queue full ({sharedQueue.Count}), waiting...", ConsoleColor.Yellow);
                        Monitor.Wait(queueLock); // Release lock and wait
                    }

                    // Add item to queue
                    sharedQueue.Enqueue(number);
                    Interlocked.Increment(ref totalProduced);
                    
                    LogMessage($"Producer-{id}", $"Produced: {number} (Queue: {sharedQueue.Count}/{MAX_QUEUE_SIZE})", ConsoleColor.Green);
                    
                    // Notify waiting consumers
                    Monitor.Pulse(queueLock);
                }

                // Simulate production time (fast producers)
                Thread.Sleep(random.Next(50, 100));
            }

            LogMessage($"Producer-{id}", "Finished producing", ConsoleColor.Cyan);
        }

        /// <summary>
        /// Consumer thread - reads and prints numbers from the queue
        /// </summary>
        static void Consumer(int id)
        {
            while (true)
            {
                int number;
                bool hasItem = false;

                lock (queueLock)
                {
                    // Wait if queue is empty and production is not complete
                    while (sharedQueue.Count == 0 && !productionComplete)
                    {
                        LogMessage($"Consumer-{id}", "Queue empty, waiting...", ConsoleColor.Yellow);
                        Monitor.Wait(queueLock); // Release lock and wait
                    }

                    // Exit if queue is empty and production is complete
                    if (sharedQueue.Count == 0 && productionComplete)
                    {
                        LogMessage($"Consumer-{id}", "No more items, exiting", ConsoleColor.Cyan);
                        break;
                    }

                    // Consume item from queue
                    if (sharedQueue.Count > 0)
                    {
                        number = sharedQueue.Dequeue();
                        hasItem = true;
                        Interlocked.Increment(ref totalConsumed);
                        
                        LogMessage($"Consumer-{id}", $"Consumed: {number} (Queue: {sharedQueue.Count}/{MAX_QUEUE_SIZE})", ConsoleColor.Magenta);
                        
                        // Notify waiting producers
                        Monitor.Pulse(queueLock);
                    }
                }

                if (hasItem)
                {
                    // Simulate consumption/processing time (slow consumers)
                    Thread.Sleep(new Random().Next(200, 500));
                }
            }
        }

        /// <summary>
        /// Thread-safe console logging with color
        /// </summary>
        static void LogMessage(string source, string message, ConsoleColor color)
        {
            lock (consoleLock)
            {
                Console.ForegroundColor = color;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [{source,-12}] {message}");
                Console.ResetColor();
            }
        }
    }
}
