using System;

namespace Contracts
{
    public class ErrorArgs : EventArgs
    {
        public ErrorArgs(string s)
        {
            Text = s;
        }

        public string Text { get; private set; }
    }
}
