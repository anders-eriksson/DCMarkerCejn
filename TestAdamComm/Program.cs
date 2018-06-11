using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        private static void Main(string[] args)
        {
            Thread.Sleep(0);
            //DisplayPartial();
            TestRead(48);
        }

        private static void TestRead(byte expectedValue)
        {
            SetByteValue(255);
            _idx = 0;
            byte data = ReadValue();
        }

        private static void SetByteValue(byte v)
        {
            byteArr = ConvertByteToBitArray(v);
        }

        private static byte ReadValue()
        {
            byte result = 0;
            byte data;
            byte idx = 0;
            do
            {
                data = ConvertBitArrayToByte(byteArr);
                idx = CalcPos(data);
                Dec(idx);
                if (IsAllowedData(data))
                {
                    result = data;
                }
                else
                {
                    result = 0;
                }
            } while (result == 0);

            return result;
        }

        private static void Dec(byte idx)
        {
            byteArr[idx] = 0;
        }

        private static byte CalcPos(byte data)
        {
            byte result = 0;

            if (data > 127)
                result = 0;
            else if (data > 63)
                result = 1;
            else if (data > 31)
                result = 2;
            else if (data > 15)
                result = 3;
            else if (data > 7)
                result = 4;
            else if (data > 3)
                result = 5;
            else if (data > 1)
                result = 6;
            else if (data == 0)
                result = 7;

            return result;
        }

        private static bool IsAllowedData(byte data)
        {
            bool result = _allowedData.Contains(data);
            if (result && _allowedParamReReadArray.Contains(data))
            {
                if (_reread < 4)
                {
                    if (_lastData != data)
                        result = false;
                }

                _lastData = data;
                _reread++;
            }

            return result;
        }

        private static void DisplayPartial()
        {
            SetValue(255);
            DisplayArray(boolArr);
            SetValue(newValue);
            DisplayArray(boolArr);
            for (int i = 0; i < 8; i++)
            {
                DisplayFromTo(255, newValue++);
            }
            Console.ReadLine();
        }

        private static void DisplayFromTo(byte start, byte wanted)
        {
            using (StreamWriter sw = new StreamWriter(@"c:\temp\adamparam.txt", true))
            {
                sw.WriteLine("=======================================");
                sw.WriteLine(string.Format("{0} ==> {1}", start, wanted));
            }
            bool[] endArr = ConvertByteToBoolArray(wanted);
            SetValue(start);
            DisplayArray(boolArr);
            _idx = 0;
            do
            {
                Dec(boolArr, endArr);
                DisplayArray(boolArr);
                _idx++;
            } while (_idx < 8);
        }

        private static void Dec(bool[] byteArr, bool[] endArr)
        {
            byteArr[_idx] = endArr[_idx];
        }

        private static void DisplayArray(bool[] bArr)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 8; i++)
            {
                sb.Append(string.Format("{0}", bArr[i].ToString().PadRight(5)));
                sb.Append(" - ");
            }
            Console.WriteLine(string.Format("{0} {1}", sb.ToString(), ConvertBoolArrayToByte(bArr)));
            using (StreamWriter sw = new StreamWriter(@"c:\temp\adamparam.txt", true))
            {
                sw.WriteLine(string.Format("{0} {1}", sb.ToString(), ConvertBoolArrayToByte(bArr)));
            }
        }

        private static void SetValue(byte v)
        {
            boolArr = ConvertByteToBoolArray(v);
        }

        private static bool[] ConvertByteToBoolArray(byte b)
        {
            // prepare the return result
            bool[] result = new bool[8];

            // check each bit in the byte. if 1 set to true, if 0 set to false
            for (int i = 0; i < 8; i++)
                result[i] = (b & (1 << i)) == 0 ? false : true;

            // reverse the array
            Array.Reverse(result);

            return result;
        }

        private static byte ConvertBoolArrayToByte(bool[] source)
        {
            byte result = 0;
            // This assumes the array never contains more than 8 elements!
            int index = 8 - source.Length;

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

        private static byte[] ConvertByteToBitArray(byte b)
        {
            // prepare the return result
            byte[] result = new byte[8];

            // check each bit in the byte. if 1 set to true, if 0 set to false
            for (int i = 0; i < 8; i++)
                result[i] = (b & (1 << i)) == 0 ? (byte)0 : (byte)1;

            // reverse the array
            Array.Reverse(result);

            return result;
        }

        private static byte ConvertBitArrayToByte(byte[] source)
        {
            byte result = 0;
            // This assumes the array never contains more than 8 elements!
            int index = 8 - source.Length;

            // Loop through the array
            foreach (byte b in source)
            {
                // if the element is 'true' set the bit at that position
                if (b == 1)
                    result |= (byte)(1 << (7 - index));

                index++;
            }

            return result;
        }
    }
}