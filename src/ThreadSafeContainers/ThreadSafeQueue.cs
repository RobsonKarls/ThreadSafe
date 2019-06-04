using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadSafeContainers {
    /// <summary>
    /// Despite the name of the class, it is currently not thread-safe, and is missing use-cases as defined
    /// in the test outline.  Some methods may also be missing basic input and output validation that should
    /// also be taken into consideration
    /// </summary>
    public sealed class ThreadSafeQueue<T> {
        private readonly Queue<T> _queue = new Queue<T>();
        private object _lock = new object();

        public IEnumerable<T> DequeueAll() {
            var list = new List<T>();
            lock(_lock) {
                while (_queue.Count == 0) {
                    Monitor.Wait(_lock);
                }

                while (_queue.Count > 0) {
                    list.Add(_queue.Dequeue());
                }

                return list;
            }
        }

        public int Count {
            get {
                lock(_lock) {
                    return _queue.Count;
                }
            }
        }

        public void Enqueue(T value) {
            lock(_lock) {
                _queue.Enqueue(value);

                Monitor.PulseAll(_lock);
            }
        }

        public void EnqueueAll(IEnumerable<T> values) {
            lock(_lock) {
                foreach (T v in values) {
                    _queue.Enqueue(v);
                }

                Monitor.PulseAll(_lock);
            }
        }

        public Task<T> DequeueAsync() {
            var value = _queue.Dequeue();
            return Task.FromResult(value);
        }
    }
}
