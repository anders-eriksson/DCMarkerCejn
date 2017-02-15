namespace DCMarker.Model
{
    using System;
    using Contracts;

    public interface IWorkFlow
    {
        event EventHandler<ErrorArgs> ErrorEvent;

        event EventHandler<StatusArgs> StatusEvent;

        event EventHandler<UpdateMainViewModelArgs> UpdateMainViewModelEvent;

        void Close();

        bool Initialize();

        void TestFunction();

        void ResetAllIoSignals();
    }
}