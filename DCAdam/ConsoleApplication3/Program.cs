using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DCAdam;
using Contracts;

namespace ConsoleApplication3
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Adam adam = new Adam("10.0.0.100", 502);
            adam.Initialize();
            adam.Connect();

            byte data = adam.Read(Constants.DIstartAddress, Constants.DItotalPoints);
            byte outData = default(byte);
            bool brd = adam.SetDIConfig(outData);
            data = adam.Read(Constants.DIstartAddress, Constants.DItotalPoints);
        }
    }
}