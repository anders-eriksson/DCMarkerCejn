using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public class MachineTypes
    {
        public MachineTypes()
        {
            RobotAF = 1;
            Manual = 2;
        }

        public int RobotAF { get; private set; }
        public int Manual { get; private set; }
    }
}