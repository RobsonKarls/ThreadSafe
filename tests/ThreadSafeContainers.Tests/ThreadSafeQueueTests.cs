using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace ThreadSafeContainers.Tests
{
    public sealed class ThreadSafeQueueTests
    {
        [Fact]
        public void ShouldCreateNewInstance()
        {
            var queue = new ThreadSafeQueue<string>();
            Assert.NotNull(queue);
        }

        [Fact]
        public void ShouldDefaultToEmpty()
        {
            var queue = new ThreadSafeQueue<int>();

            Assert.Equal(0, queue.Count);
            Assert.Empty(queue.DequeueAll());
        }

        [Fact]
        public void ShouldUpdateCountWhenAddingToQueue()
        {
            var queue = new ThreadSafeQueue<long>();

            queue.Enqueue(5);
            queue.Enqueue(42);

            Assert.Equal(2, queue.Count);
        }

        [Fact]
        public void ShouldDequeueAllAfterAdded()
        {
            var expected = new[] {
                "hello",
                "world"
            };

            var queue = new ThreadSafeQueue<string>();

            queue.Enqueue("hello");
            queue.Enqueue("world");

            var actual = queue.DequeueAll();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ShouldEnqueueColletionOfItems()
        {
            var expected = new[] {
                "hello",
                "world"
            };

            var queue = new ThreadSafeQueue<string>();

            queue.Enqueue(expected);

            var actual = queue.DequeueAll();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ShouldBlockDequeUntilTimoutOccurs()
        {
            var dummy = new[] {
                "hello",
                "world",
                "TripStack"
            };

            string result = string.Empty;
            string result2 = string.Empty;

            string expected = dummy[0];

            int timeout = 500;
            int biggerTimeout = 1000;

            int waitTime = 800;

            List<string> resultSet = new List<string>();

            var queue = new ThreadSafeQueue<string>();


            var loadThread = new Thread(() =>
            {
                queue.Enqueue(dummy);
            });

            var smallTimeoutThread = new Thread(() =>
            {
                result = queue.Dequeue(timeout);
            });

            var bigTimeoutThread = new Thread(() =>
            {
                result2 = queue.Dequeue(biggerTimeout);
            });

            // should try to consume the empty queue, block thread for short time
            smallTimeoutThread.Start();
            
            // testing the timeout 
            await Task.Delay(waitTime);

            // load data into the queue
            loadThread.Start();
            
            // start to consume a queue with data, this thread has a 1sec timeout
            bigTimeoutThread.Start();

            // wait for this thread to finish, in order to test properly
            bigTimeoutThread.Join();

            Assert.Equal(expected, result2);
            Assert.Null(result);
        }

        [Fact]
        public async Task ShouldDoNotBlockWhenDequeueingNonEmptyQueue()
        {
            var data = new[] { "value1", "value2" };
            string expected = data[0];
            var queue = new ThreadSafeQueue<string>();

            queue.Enqueue(data);

            var result = await queue.DequeueAsync();

            Assert.NotNull(result);
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ShouldDoNotBlockWhenDequeueingAnEmptyQueue()
        {
            var queue = new ThreadSafeQueue<string>();

            var result = await queue.DequeueAsync();

            Assert.Null(result);
        }

        [Fact]
        public void ShouldBlockThreadUntilAnItemBecamesAvailableThenDequeueIt()
        {
            var dummy = new[] {
                "hello",
                "world",
                "TripStack"
            };

            List<string> resultSet = new List<string>();

            var queue = new ThreadSafeQueue<string>();

            var resultThread1 = new Thread(() =>
            {
                var result = queue.DequeueWithWait();
                resultSet.Add(result);
            });

            var resultThread2 = new Thread(() =>
            {
                var result = queue.DequeueWithWait();
                resultSet.Add(result);
            });

            var resultThread3 = new Thread(() =>
            {
                var result = queue.DequeueWithWait();
                resultSet.Add(result);
            });

            // start dequeing  data from the empty queue
            resultThread1.Name = "DQ1";
            resultThread1.Start();

            resultThread2.Name = "DQ2";
            resultThread2.Start();

            resultThread3.Name = "DQ3";
            resultThread3.Start();

            var loadThreads = LoadThreads(dummy, 5, queue);

            // start threads that load data into the queue's
            foreach (var t in loadThreads)
            {
                t.Start();
            }

            // block main thread until the consuming threads finish their work
            resultThread1.Join();
            resultThread2.Join();
            resultThread3.Join();

            Assert.True(resultSet.Count == 3);
        }

        private Thread[] LoadThreads(string[] dummyData, int totalThreads, ThreadSafeQueue<string> queue)
        {
            Thread[] loadThreads = new Thread[totalThreads];

            for (int i = 0; i < totalThreads; i++)
            {
                loadThreads[i] = new Thread(() =>
                {
                    queue.Enqueue(dummyData);
                });
            }
            return loadThreads;
        }

        [Fact]
        public async Task ShouldDequeueInstantlyIfValueExistsInQueue()
        {
            var queue = new ThreadSafeQueue<string>();

            queue.Enqueue("hello");
            queue.Enqueue("world");

            var actual = await queue.DequeueAsync();

            Assert.Equal("hello", actual);
        }
    }
}
