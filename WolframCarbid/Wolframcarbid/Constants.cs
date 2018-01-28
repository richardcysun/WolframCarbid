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
        public const string CMD_INST_SVC_USAGE = "Usgae: WolframCarbid.exe -wc=inst -act=[install | uninstall]";

        public const string CMD_CTRL_SVC_NAME = "ctrl";
        public const string CMD_CTRL_SVC_USAGE = "Usgae: WolframCarbid.exe -wc=ctrl -act=[start | stop] -svc={name}";

        public const string CMD_CTRL_SLAVE_SVC_NAME = "ctrlslv";
        public const string CMD_CTRL_SLAVE_SVC_USAGE = "Usgae: WolframCarbid.exe -c=ctrlslv -act=[start | stop] -svc={name}";

        public const string CMD_BUS_NAME = "bus";
        public const string CMD_BUS_USAGE = "Usgae: WolframCarbid.exe -wc=bus -rt=892 -stop=\"Taipei Station\" -bound=[in | out]";

        public const string CMD_PM25_NAME = "pm25";
        public const string CMD_PM25_USAGE = "Usgae: WolframCarbid.exe -wc=pm25 -loc=Songshan";

        public const string CMD_PARAM_ACT = "-act";
        public const string CMD_PARAM_SVC = "-svc";
        public const string CMD_PARAM_ROUTE = "-rt";
        public const string CMD_PARAM_STOP = "-stop";
        public const string CMD_PARAM_BOUND = "-bound";
        public const string CMD_PARAM_LOCATION = "-loc";

        public const string CMD_VALUE_INSTALL = "install";
        public const string CMD_VALUE_UNINSTALL = "uninstall";
        public const string CMD_VALUE_START = "start";
        public const string CMD_VALUE_STOP = "stop";
        public const string CMD_VALUE_IN = "in";
        public const string CMD_VALUE_OUT = "out";
    }

    static class CDataBaseConstants
    {
        public const string MSATER_DB = "master";
        public const string WOLFRAMCARBID_DB = "WolframCarbid";
    }

    public enum ErrorCodes : int
    {
        SUCCESS = 0,
        UNABLE_TO_LAUNCH_PROC = 1,
        UNABLE_TO_GET_DATA = 2,
        UNABLE_TO_PARSE_DATA = 3,
        UNABLE_TO_FIND_DATA = 4,

        ERR_NOT_IMP = 555,
        ERR_UNDEFINED = 999
    }
}
