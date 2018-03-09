using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaserWrapper
{
    public class IoFix
    {
        private static int _currentMask;

        public static void Init()
        {
            _currentMask = 0;
        }

        public static int Add(int mask)
        {
            _currentMask |= mask;

            return _currentMask;
        }

        public static int Delete(int mask)
        {
            _currentMask &= ~mask;

            return _currentMask;
        }
    }
}