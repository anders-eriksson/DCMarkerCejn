using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public enum Errors
    {
        NoError = 0,

        /// <summary>
        /// Article number not found in database
        /// </summary>
        ArticleNotFound = 1,

        /// <summary>
        /// Layout file not found
        /// </summary>
        LayoutNotFound = 2,

        /// <summary>
        /// Layout is not defined. Only occurs when PLC sends Start/Ok before ArtNo
        /// </summary>
        LayoutNotDefined = 3,

        /// <summary>
        /// Unknow command. The command is not defined in the command list
        /// </summary>
        UnknownCommand = 4,
    }
}