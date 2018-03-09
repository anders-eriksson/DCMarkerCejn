using System;

namespace Contracts
{

    public class LaserBusyEventArgs : EventArgs
    {
        public LaserBusyEventArgs(bool busy)
        {
            Busy = busy;
        }

        public bool Busy { get; private set; } // readonly
    }

}