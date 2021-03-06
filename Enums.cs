﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Nicome.Enums
{
    enum GenelicErrorCode
    {
        ERROR,
        SAFE,
        CONTINUE,
        SKIP,
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
        COM_LOG,
        NG_BY_TIME,
        NG_FROM_POST_DATETIME,
        NG_DELAY,
        NG_MAIL,
        NG_UID,
        MAX_COM,
        OVERWRITE,
        DONT_OVERWRITE,
        CHANNEL
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
