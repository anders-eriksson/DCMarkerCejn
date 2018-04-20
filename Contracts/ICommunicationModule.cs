﻿using System;

namespace Contracts
{
    public interface ICommunicationModule
    {
        bool Connect();

        bool Initialize();

        void LoadCommands(string commandFile);

        byte Read(ushort startAddress, ushort totalPoints);

        bool Write(ushort startAddress, byte? data);
    }
}