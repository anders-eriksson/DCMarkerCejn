using Configuration;
using Contracts;
using DCTcpServer;
using System;

namespace DCMarker.Model
{
    public class TcpArticleInputWithTOnr : IArticleInput
    {
        private DCConfig cfg;
        private ServerWithTOnr _server;
        private object lockObject = new object();

        public TcpArticleInputWithTOnr()
        {
            try
            {
                cfg = DCConfig.Instance;
                cfg.ReadConfig();
                _server = new ServerWithTOnr(cfg.TcpPort, cfg.BufferLength, cfg.ArticleNumberLength, cfg.ToNumberLength);
                _server.NewArticleNumberEvent += _server_NewArticleNumberEvent;
            }
            catch (Exception ex)
            {
                DCLog.Log.Error(ex, "Tcp Server Error");
                throw;
            }
        }

        private void _server_NewArticleNumberEvent(string articleNumber, string ToNumber)
        {
            ArticleData data = new ArticleData
            {
                ArticleNumber = articleNumber,
                TOnr = ToNumber,
                BatchSize = 1,
                TestItem = false
            };

            RaiseArticleEvent(null, data);
        }

        #region Article Event

        public event EventHandler<ArticleArgs> ArticleEvent;

        internal void RaiseArticleEvent(object sender, ArticleData data)
        {
            EventHandler<ArticleArgs> handler = ArticleEvent;
            if (handler != null)
            {
                ArticleArgs args = new ArticleArgs(data);
                handler(sender, args);
            }
        }

        public void Close()
        {
            if (_server != null)
            {
                _server.Abort();
            }
        }

        #endregion Article Event
    }
}