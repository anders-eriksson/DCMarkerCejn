namespace DCMarker.Model
{
    using Contracts;
    using System;

    public interface IWorkFlow
    {
        event EventHandler<ErrorArgs> ErrorEvent;

        event EventHandler<StatusArgs> ErrorMsgEvent;

        event EventHandler<StatusArgs> StatusEvent;

        event EventHandler<UpdateMainViewModelArgs> UpdateMainViewModelEvent;

        bool AcknowledgeTONumber(string articleNumber, string kant, string toNumber);

        void Close();

        //void CreateHistoryData(string toNumber);

        void Execute();

        bool Initialize();

        void ResetAllIoSignals();

        void SimulateItemInPlace();
    }
}