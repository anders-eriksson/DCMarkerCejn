namespace Contracts
{
    public interface IDigitalIo
    {
        bool SetReady(bool OnOff);

        bool SetPort(int port, int mask);

        bool ResetPort(int port, int mask);

        int GetPort(int port);
    }
}
