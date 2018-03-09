using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Contracts;
using CommunicationService;
using Configuration;
using System.Timers;

namespace ConsoleApplication2
{
    public class Program
    {
        private static CommService _commService;
        private static Timer _timer;

        private static void Main(string[] args)
        {
            ICommunicationModule _communication = CreateCommunicationModule(2);
        }

        public static ICommunicationModule CreateCommunicationModule(int module)
        {
            ICommunicationModule result = null;

            switch (module)
            {
                case 1:
                    result = new DCAdam.Adam();
                    break;

                default:
                    break;
            }

            return result;
        }
    }
}