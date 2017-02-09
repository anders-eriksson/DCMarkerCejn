using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public class StatusArgs : EventArgs
    {
        public StatusArgs(string s)
        {
            Text = s;
        }

        public string Text { get; private set; } // readonly
    }
}