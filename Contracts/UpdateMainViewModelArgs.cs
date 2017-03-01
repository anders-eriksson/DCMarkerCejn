using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public class UpdateMainViewModelArgs : EventArgs
    {
        public UpdateMainViewModelArgs(UpdateViewModelData data)
        {
            Data = data;
        }

        public UpdateViewModelData Data { get; private set; } // readonly
    }
}
