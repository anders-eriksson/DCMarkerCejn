using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public interface ICommandQueue
    {
        void Enqueue(Command item);

        Command Dequeue();

        bool IsEmpty();
    }
}