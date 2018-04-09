using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public abstract class Command
    {
        public ICommunicationModule _comm;
        public CommandTypes Type { get; set; }

        public abstract void Run();
    }
}