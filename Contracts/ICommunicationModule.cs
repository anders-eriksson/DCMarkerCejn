using System;

namespace Contracts
{
    public interface ICommunicationModule
    {
        bool Connect();

        bool Initialize();

        void LoadCommands(string commandFile);

        byte Read(ushort startAddress, ushort totalPoints);

        void ReadCommand(byte command, byte _currentEdge, int _totalEdges);

        bool Write(ushort startAddress, byte? data);
        void ReadCommand(byte command, string artno);
    }
}