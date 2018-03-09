namespace Contracts
{
    public interface ICommunicationModule
    {
        bool Connect();

        bool Initialize();

        byte Read(ushort startAddress, ushort totalPoints);

        bool Write(ushort startAddress, byte data);
    }
}