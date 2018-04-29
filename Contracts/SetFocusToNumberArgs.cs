using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public class SetFocusToNumberArgs : EventArgs
    {
        public SetFocusToNumberArgs(bool mode)
        {
            Mode = mode;
        }

        public bool Mode { get; private set; }
    }
}