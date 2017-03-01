using Configuration;
using Contracts;
using DCTcpServer;
using System;

namespace DCMarker.Model
{
    public class TcpArticleInput : IArticleInput
    {
        private DCConfig cfg;
        private Server _server;
        private object lockObject = new object();

        public TcpArticleInput()
        {
            cfg = DCConfig.Instance;
            cfg.ReadConfig();
            _server = new Server(cfg.TcpPort, cfg.BufferLength);
            _server.NewArticleNumberEvent += _server_NewArticleNumberEvent;
        }

        private void _server_NewArticleNumberEvent(string msg)
        {
            ArticleData data = new ArticleData
            {
                ArticleNumber = msg,
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
