using System;

namespace DCMarkerEF
{
    public interface IModificationHistory
    {
        DateTime DateCreated { get; set; }
        DateTime DateModified { get; set; }
        bool IsDirty { get; set; }
    }
}