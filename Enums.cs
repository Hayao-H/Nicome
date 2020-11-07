using System;
using System.Collections.Generic;
using System.Text;

namespace Nicome.Enums
{
    enum GenelicErrorCode
    {
        ERROR,
        SAFE,
        CONTINUE,
        EXIT,
        OK
    }

    enum Option
    {
        LOGLEVEL,
        NICOID,
        FOLDER,
        USER,
        PASS,
        COM_LOG
    }

    enum LOGLEVEL
    {
        Quiet,
        Error,
        Warn,
        Log,
        Info,
        Debug
    }
}
