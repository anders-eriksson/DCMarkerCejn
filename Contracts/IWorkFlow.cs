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

        event EventHandler<ArticleHasToNumberArgs> ArticleHasToNumberEvent;

        void Close();

        bool Initialize();

        void SimulateItemInPlace(int seq);

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

        void LoadArticleNumber(string _articleNumber);

        void UpdateTOnumber(string onr);

#if DEBUG

        void _laser_ItemInPositionEvent();

#endif
    }
}