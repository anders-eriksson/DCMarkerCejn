using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Advantech.Adam;
using Configuration;
using DCLog;
using System.Net.Sockets;
using Contracts;

namespace DCAdam
{
    public class Adam : ICommunicationModule
    {
        private string _ipAddress;
        private int _ipPort = 502;
        private AdamSocket adamModbus;

        public Adam()
        {
            _ipAddress = DCConfig.Instance.AdamIpAddress;
            _ipPort = DCConfig.Instance.AdamIpPort;
        }

        public Adam(string ipAddress, ushort ipPort)
        {
            _ipAddress = ipAddress;
            _ipPort = ipPort;
        }

        public bool Connect()
        {
            bool result = false;
            try
            {
                //result = adamModbus.Connect(_ipAddress, ProtocolType.Tcp, _ipPort);
                result = adamModbus.Connect(AdamType.Adam6000, _ipAddress, ProtocolType.Tcp);
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        public bool Initialize()
        {
            bool result = true;

            try
            {
                adamModbus = new AdamSocket();
                adamModbus.SetTimeout(1000, 1000, 1000);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Error initializing ADAM module!");
                throw;
            }

            return result;
        }

        public byte Read(ushort startAddress, ushort totalPoints)
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
                Log.Error(ex, "Error reading data");
                throw;
            }

            return result;
        }

        public bool[] SetValue(byte v)
        {
            bool[] result;

            result = ConvertByteToBoolArray(v);

            return result;
        }

        public bool Write(ushort startAddress, byte data)
        {
            bool result = true;
            try
            {
                bool[] dataArr = ConvertByteToBoolArray(data);
#if DEBUG
                if (startAddress == Constants.DIstartAddress)
                {
                    bool brc;
                    brc = adamModbus.DigitalInput().SetInvertMask(dataArr);
                    //for (int i = 0; i < 8; i++)
                    //{
                    //    bool[] a = new bool[1] { dataArr[i] };
                    //    brc = adamModbus.DigitalInput().SetInvertMask(i, a);
                    //}
                }
                else
#endif
                {
                    WriteCoils(startAddress, dataArr);
                }
            }
            catch (Exception)
            {
                throw;
            }

            return result;
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

        private byte ConvertBoolArrayToByte(bool[] source)
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

        private bool[] ReadCoils(ushort startAddress, ushort numberOfPoints)
        {
            bool[] result;

            bool brc = adamModbus.Modbus().ReadCoilStatus(startAddress, numberOfPoints, out result);
            if (brc)
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        private uint[] ReadHoldingRegisters(int startAddress, int numberOfPoints)
        {
            uint[] result;
            try
            {
                bool brc = adamModbus.Modbus().ReadHoldingRegs(startAddress, numberOfPoints, out result);
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        private bool WriteCoil(ushort startAddress, bool value)
        {
            bool result = false;
            try
            {
                result = adamModbus.Modbus().ForceSingleCoil(startAddress, value);
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }

        private bool WriteCoils(ushort startAddress, bool[] values)
        {
            bool result = false;
            try
            {
                result = adamModbus.Modbus().ForceMultiCoils(startAddress, values);
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }
    }
}