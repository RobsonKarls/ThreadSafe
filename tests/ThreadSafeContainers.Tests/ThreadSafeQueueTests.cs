using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace ThreadSafeContainers.Tests {
    public sealed class ThreadSafeQueueTests {
        [Fact]
        public void ShouldCreateNewInstance() {
            var queue = new ThreadSafeQueue<string>();
            Assert.NotNull(queue);
        }

        [Fact]
        public void ShouldDefaultToEmpty() {
            var queue = new ThreadSafeQueue<int>();

            Assert.Equal(0, queue.Count);
            Assert.Empty(queue.DequeueAll());
        }

        [Fact]
        public void ShouldUpdateCountWhenAddingToQueue() {
            var queue = new ThreadSafeQueue<long>();

            queue.Enqueue(5);
            queue.Enqueue(42);

            Assert.Equal(2, queue.Count);
        }

        [Fact]
        public void ShouldDequeueAllAfterAdded() {
            var expected = new [] {
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
        public void ShouldEnqueueColletionOfItem() {
            var expected = new [] {
                "hello",
                "world"
            };

            var queue = new ThreadSafeQueue<string>();

            queue.EnqueueAll(expected);

            var actual = queue.DequeueAll();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void test1() {
            this.ShouldBlockThreadUntilAnItemBecamesAvailable().Wait();
        }

        public async Task ShouldBlockThreadUntilAnItemBecamesAvailable() {
            var expected = new [] {
                "hello",
                "world",
                "TripStack"
            };

            var expectedCount = 9;

            var queue = new ThreadSafeQueue<string>();

            var result = await Task.Run(() => {
                var dq = queue.DequeueAll();
                return dq;
            });

            var loadTasks = Task.Run(() => {
                Parallel.Invoke(
                    () => { queue.EnqueueAll(expected); },
                    () => { queue.EnqueueAll(expected); },
                    () => { queue.EnqueueAll(expected); }
                );
            });

            await loadTasks;

            Assert.Equal(expectedCount, result.Count());

        }

        [Fact]
        public async Task ShouldDequeueInstantlyIfValueExistsInQueue() {
            var queue = new ThreadSafeQueue<string>();

            queue.Enqueue("hello");
            queue.Enqueue("world");

            var actual = await queue.DequeueAsync();

            Assert.Equal("hello", actual);
        }
    }
}
