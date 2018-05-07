using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public enum CommandTypes
    {
        Undefined = -1,
        None = 0,

        /* IN */
        ArtNo = 10,
        Provbit = 11,
        OK = 12,
        ItemInPlace = 13,
        StartMarking = 14,
        EndMarking = 15,
        Restart = 16,

        /* OUT */
        SetKant = 20,
        BatchNotReady = 31,
        ReadyToMark = 22,
        Error = 29,
        StartMarking2 = 32,
    }
}