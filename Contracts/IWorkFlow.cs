namespace Contracts
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

        event EventHandler<SetupItemStatusArgs> SetupItemStatusEvent;

        event EventHandler<StatusArgs> ErrorMsgEvent;

        event EventHandler<LaserBusyEventArgs> LaserBusyEvent;

        event EventHandler<ArticleHasToNumberArgs> ArticleHasToNumberEvent;

        event EventHandler<ItemDoneArgs> ItemDoneEvent;

        event EventHandler<UpdateItemStatusArgs> UpdateItemStatusEvent;

        void Close();

        bool Initialize();

        void SimulateItemInPlace(int seq);

        void SimulateItemInPlace(string articlenumber);

        void ResetAllIoSignals();

        void ResetArticleData();

        void ResetCareful();

        void ResetItemsDone();

        void SetNextToLast();

        void ResetNextToLast();

        List<Article> GetArticle(string articleNumber);

        void UpdateWorkflow(Article article);

        bool ResetZAxis();

        void ResetArticleReady();

        bool StartPoll(int pollInterval, int errorTimeout);

        void LoadArticleNumber(string _articleNumber);

        void LoadUpdateLayout();

        void UpdateTOnumber(string onr);

        void UpdateLayout();

        void UserHasApprovedTOnumber(bool state);

        void Execute();

#if DEBUG

        void _laser_ItemInPositionEvent();

        void ArtNo(string artno);

        void StartOk();

        void Execute2();

#endif
    }
}