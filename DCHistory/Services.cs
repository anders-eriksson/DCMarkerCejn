using Jot;

namespace DCHistory
{
    //this class can be replaced by the use of an IOC container
    internal static class Services
    {
        public static StateTracker Tracker = new StateTracker();
    }
}