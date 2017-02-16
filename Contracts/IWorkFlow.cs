namespace DCMarker.Model
{
    using Contracts;
    using System;

    public interface IWorkFlow
    {
        event EventHandler<ErrorArgs> ErrorEvent;

        event EventHandler<StatusArgs> StatusEvent;

        event EventHandler<UpdateMainViewModelArgs> UpdateMainViewModelEvent;

        void Close();

        bool Initialize();

        void SimulateItemInPlace();

        void Execute();

        void ResetAllIoSignals();
    }
}