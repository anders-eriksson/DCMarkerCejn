using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Contracts;
using CommunicationService;
using Configuration;
using System.Timers;

namespace ConsoleApplication1
{
    internal class Program
    {
        private static CommService _commService;
        private static Timer _timer;

        private static void Main(string[] args)
        {
            ICommunicationModule _communication = CreateCommunicationModule(DCConfig.Instance.CommunicationModule);
            try
            {
                _commService = new CommService(_communication);
                bool brc = _commService.Initialize();
                if (brc)
                {
                    byte input;

                    brc = _commService.Connect();
                    if (brc)
                    {
                        _timer = new Timer();
                        _timer.Elapsed += _timer_Elapsed;
                        _timer.Interval = 500;
                        _timer.Start();

                        _commService.Write(Constants.DIstartAddress, 255);
                        Console.Write("1: ");
                        Console.Read();

                        _commService.Write(Constants.DIstartAddress, 3);
                        Console.Write("2: ");
                        Console.Read();
                    }
                    else
                    {
                        Console.WriteLine("Error: Cant connect to Communication module");
                    }
                }
                else
                {
                    Console.WriteLine("Error: Cant Initialize Communication module");
                }
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

#if DEBUG
            Console.Read();
#endif
        }

        private static void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();
            byte input = _commService.Read(Constants.DIstartAddress, Constants.DItotalPoints);

            Console.WriteLine(string.Format("DI - {0} - {1}", input, string.Join(", ", Helper.ConvertByteToBoolArray(input))));

            _timer.Start();
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