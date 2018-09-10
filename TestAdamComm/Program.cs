using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Advantech.Adam;
using Advantech.Common;
using System.Net.Sockets;
using System.Diagnostics;

namespace TestAdamComm
{
    internal class Program
    {
        private static bool[] boolArr = new bool[8];
        private static byte[] byteArr = new byte[8];
        private static byte[] _allowedData = new byte[] { 48, 49, 50, 51, 52, 53, 54 };
        private static byte[] _allowedParamReReadArray = new byte[] { 49, 51, 53 };
        private static Int16 _idx = 7;
        private static byte newValue = 10;

        private static int _reread = 0;
        private static byte _lastData = 0;
        private static AdamSocket adamModbus;

        private static void Main(string[] args)
        {
            adamModbus = new AdamSocket();
            bool result = adamModbus.Connect(AdamType.Adam6000, "192.168.1.100", ProtocolType.Tcp);

            //Write(255);
            for (int i = 0; i < 100; i++)
            {
                DateTime dt = new DateTime();
                Write(255);
                //WaitUntimAdamHasUpdated(255);
                Debug.WriteLine(string.Format("255 - {0}", (DateTime.Now - dt).Milliseconds.ToString()));
                Write(0);
                //WaitUntimAdamHasUpdated(0);
                Debug.WriteLine(string.Format("0 - {0}", (DateTime.Now - dt).Milliseconds.ToString()));
            }

            //Thread.Sleep(0);
            ////DisplayPartial();
            //TestRead(48);
        }

        private static void WaitUntimAdamHasUpdated(byte value)
        {
            byte tmp;
            do
            {
                tmp = Read(17, 8);
                Thread.Sleep(1);
            } while (tmp != value);
        }

        private static void Write(byte v)
        {
            Write(17, v);
        }

        public static byte Read(ushort startAddress, ushort totalPoints)
        {
            byte result = 0;

            bool[] data;
            try
            {
                data = ReadCoils(startAddress, totalPoints);
                result = ConvertBoolArrayToByte(data);
            }
            catch (Exception ex)
            {
                throw;
            }

            return result;
        }

        private static bool[] ReadCoils(ushort startAddress, ushort numberOfPoints)
        {
            bool[] result = null;
            bool brc = adamModbus.Modbus().ReadCoilStatus(startAddress, numberOfPoints, out result);

            return result;
        }

        public static bool Write(ushort startAddress, byte data)
        {
            bool result = true;
            try
            {
                bool[] dataArr = ConvertByteToBoolArray(data);
                result = WriteCoils(startAddress, dataArr);
            }
            catch (SocketException ex)
            {
                result = false;
            }

            return result;
        }

        private static bool WriteCoils(ushort startAddress, bool[] values)
        {
            bool result = false;

            result = adamModbus.Modbus().ForceMultiCoils(startAddress, values);

            return result;
        }

        private static bool[] ConvertByteToBoolArray(byte b)
        {
            // prepare the return result
            bool[] result = new bool[8];

            // check each bit in the byte. if 1 set to true, if 0 set to false
            for (int i = 0; i < 8; i++)
                result[i] = (b & (1 << i)) == 0 ? false : true;

            // reverse the array ?
            // Array.Reverse(result);

            return result;
        }

        private static byte ConvertBoolArrayToByte(bool[] source)
        {
            byte result = 0;
            // This assumes the array never contains more than 8 elements!
            int index = 8 - source.Length;

            Array.Reverse(source);
            // Loop through the array
            foreach (bool b in source)
            {
                // if the element is 'true' set the bit at that position
                if (b)
                    result |= (byte)(1 << (7 - index));

                index++;
            }

            return result;
        }
    }
}