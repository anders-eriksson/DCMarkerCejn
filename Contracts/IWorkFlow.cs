namespace DCMarker.Model
{
    using Contracts;
    using System;
    using System.Collections.Generic;

    public interface IWorkFlow
    {
        event EventHandler<ErrorArgs> ErrorEvent;

        event EventHandler<StatusArgs> StatusEvent;

        event EventHandler<UpdateMainViewModelArgs> UpdateMainViewModelEvent;

        event EventHandler<StatusArgs> ErrorMsgEvent;

        void Close();

        bool Initialize();

        void SimulateItemInPlace();

        void Execute();

        void ResetAllIoSignals();

        List<Article> GetArticle(string articleNumber);
    }
}