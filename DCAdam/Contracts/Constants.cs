using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public class Constants
    {
        #region ADAM 6052

        public const ushort DIstartAddress = 1;
        public const ushort DItotalPoints = 8;

        public const ushort DOstartAddress = 17;
        public const ushort DOtotalPoints = 8;

        #endregion ADAM 6052

        #region ADAM Protocol

        // End of command
        public const byte ETX = 3;

        // Start of command
        public const byte STX = 2;

        // Acknowledgement of signal
        public const byte ACK = 255;

        /* Codes from PLC to PC */
        public const byte ArtNrCode = 10;
        public const byte ProvbitCode = 11;
        public const byte OkCode = 12;
        public const byte ItemInPlaceCode = 13;
        public const byte StartMarkingCode = 14;
        public const byte EndMarkingCode = 15;
        public const byte RestartCode = 16;

        /* Codes from PC to PLC */
        public const byte SetKantCode = 20;
        public const byte BatchNotReadyCode = 21;
        public const byte ReadyToMarkCode = 22;
        public const byte ErrorCode = 29;

        /* Error Codes */
        public const byte ArticleNotFound = 1;
        public const byte LayoutNotFound = 2;
        public const byte LayoutNotDefined = 3;
        public const byte UnknownCommand = 4;

        #endregion ADAM Protocol
    }
}