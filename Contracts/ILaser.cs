using System.Collections.Generic;

namespace Contracts
{
    public interface ILaser
    {
        int ErrorCode { get; set; }

        bool Execute();

        bool Load(string layout);

        void Release();

        bool Update(List<LaserObjectData> objectList);
    }
}
