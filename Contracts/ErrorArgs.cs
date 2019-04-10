using System;

namespace Contracts
{
    public class ErrorArgs : EventArgs
    {
        public ErrorArgs(string s, bool abort = false)
        {
            Text = s;
            Abort = abort;
        }

        public string Text { get; private set; }
        public bool Abort { get; set; }
    }
}