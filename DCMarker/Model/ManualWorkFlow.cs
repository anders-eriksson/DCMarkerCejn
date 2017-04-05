using Configuration;
using Contracts;
using DCTcpServer;
using LaserWrapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DCMarker.Model
{
    public class ManualWorkFlow : IWorkFlow
    {
#pragma warning disable CS0067 // The event 'ManualWorkFlow.ErrorEvent' is never used

        public event EventHandler<ErrorArgs> ErrorEvent;

#pragma warning restore CS0067 // The event 'ManualWorkFlow.ErrorEvent' is never used

#pragma warning disable CS0067 // The event 'ManualWorkFlow.StatusEvent' is never used

        public event EventHandler<StatusArgs> StatusEvent;

#pragma warning restore CS0067 // The event 'ManualWorkFlow.StatusEvent' is never used

#pragma warning disable CS0067 // The event 'ManualWorkFlow.UpdateMainViewModelEvent' is never used

        public event EventHandler<UpdateMainViewModelArgs> UpdateMainViewModelEvent;

        public event EventHandler<StatusArgs> ErrorMsgEvent;

#pragma warning restore CS0067 // The event 'ManualWorkFlow.UpdateMainViewModelEvent' is never used

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Execute()
        {
            throw new NotImplementedException();
        }

        public bool Initialize()
        {
            throw new NotImplementedException();
        }

        public void ResetAllIoSignals()
        {
            throw new NotImplementedException();
        }

        public void SimulateItemInPlace()
        {
            throw new NotImplementedException();
        }
    }
}