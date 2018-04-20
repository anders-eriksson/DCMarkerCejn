using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlcSimulator
{
    public class CommandData
    {
        public CommandData()
        {
            Params = new List<byte>();
        }

        public CommandTypes Type { get; set; }
        public List<byte> Params { get; set; }
        public bool Provbit { get; set; }
    }
}