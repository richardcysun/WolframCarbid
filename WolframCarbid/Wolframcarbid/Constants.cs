using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wolframcarbid
{
    static class CServiceConstants
    {
        public const string SERVICE_NAME = "WolframCarbid";
        public const string SERVICE_DISPLAY_NAME = "WolframCarbid Command-Line Interface";
        public const string SERVICE_MODE = "-s";

    }

    static class CCmdConstants
    {
        public const string CMD_PREFIX = "-wc";

        public const string CMD_INST_SVC_NAME = "inst";
        public const string CMD_INST_SVC_USAGE = "Usgae: WolframCarbid.exe -wc=inst -act={install | uninstall}";

        public const string CMD_CTRL_SVC_NAME = "ctrl";
        public const string CMD_CTRL_SVC_USAGE = "Usgae: WolframCarbid.exe -wc=ctrl -act={start | stop} -svc={name}";

        public const string CMD_CTRL_SLAVE_SVC_NAME = "ctrlslv";
        public const string CMD_CTRL_SLAVE_SVC_USAGE = "Usgae: WolframCarbid.exe -c=ctrlslv -act={start | stop} -svc={name}";

        public const string CMD_BUS_NAME = "bus";
        public const string CMD_BUS_USAGE = "Usgae: WolframCarbid.exe -wc=bus -rt=892 -bound={in | out}";

        public const string CMD_PARAM_ACT = "-act";
        public const string CMD_PARAM_SVC = "-svc";
        public const string CMD_PARAM_ROUTE = "-rt";
        public const string CMD_PARAM_STOP = "-stop";
        public const string CMD_PARAM_BOUND = "-bound";

        public const string CMD_VALUE_INSTALL = "install";
        public const string CMD_VALUE_UNINSTALL = "uninstall";
        public const string CMD_VALUE_START = "start";
        public const string CMD_VALUE_STOP = "stop";
        public const string CMD_VALUE_IN = "in";
        public const string CMD_VALUE_OUT = "out";
    }

    public enum ErrorCodes : int
    {
        SUCCESS = 0,
        UNABLE_TO_LAUNCH_PROC = 1,
        UNABLE_TO_PARSE_DATA = 2,
        UNABLE_TO_FIND_DATA = 3,

        ERR_NOT_IMP = 555,
        ERR_UNDEFINED = 999
    }
}
