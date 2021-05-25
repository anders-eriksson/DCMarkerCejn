using System;

namespace Contracts
{
    public class UpdateSerialNumberArgs : EventArgs
    {
        public UpdateSerialNumberArgs(string msg)
        {
            Text = msg;
        }

        public string Text { get; private set; }
    }
}