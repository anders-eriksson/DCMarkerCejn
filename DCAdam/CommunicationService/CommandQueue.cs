using System;
using Contracts;
using System.Collections.Concurrent;
using Ardalis.GuardClauses;

namespace CommunicationService
{
    internal class CommandQueue : ICommandQueue
    {
        private ConcurrentQueue<Command> _queue;

        public CommandQueue()
        {
            _queue = new ConcurrentQueue<Command>();
        }

        public Command Dequeue()
        {
            Command result = null;

            bool brc = _queue.TryDequeue(out result);
            if (!brc)
            {
                result = null;
            }
            return result;
        }

        public void Enqueue(Command item)
        {
            Guard.Against.Null(item, nameof(item));
            _queue.Enqueue(item);
        }

        public bool IsEmpty()
        {
            bool result = false;
            Command qresult = null;

            result = _queue.TryPeek(out qresult);

            return result;
        }
    }
}