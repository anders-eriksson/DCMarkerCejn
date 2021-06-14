using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public interface IWorkFlowDebug
    {
#if DEBUG
        void _laser_ItemInPositionEvent();

        void ArtNo(string artno);

        void StartOk();

        void Execute();

        void Execute2();
#endif
    }
}