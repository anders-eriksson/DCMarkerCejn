namespace DCMarker.Model
{
    using Contracts;
    using System;
    using System.Collections.Generic;

    public interface IWorkFlow
    {
        bool FirstMarkingResetZ { get; set; }

        event EventHandler<ErrorArgs> ErrorEvent;

        event EventHandler<StatusArgs> StatusEvent;

        event EventHandler<UpdateMainViewModelArgs> UpdateMainViewModelEvent;

        event EventHandler<StatusArgs> ErrorMsgEvent;

        event EventHandler<LaserBusyEventArgs> LaserBusyEvent;

        void Close();

        bool Initialize();

        void SimulateItemInPlace();

        void Execute();

        void ResetAllIoSignals();

        void ResetArticleData();

        void SetNextToLast();

        void ResetNextToLast();

        List<Article> GetArticle(string articleNumber);

        void UpdateWorkflow(Article article);

        bool ResetZAxis();

        void ResetArticleReady();

        bool StartPoll(int pollInterval, int errorTimeout);

#if DEBUG

        void _laser_ItemInPositionEvent();

#endif
    }
}