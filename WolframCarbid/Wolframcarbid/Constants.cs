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
        public const string CMD_PREFIX = "-c";

        public const string CMD_INST_SVC_NAME = "inst";
        public const string CMD_INST_SVC_USAGE = "WolframCarbid.exe -c=inst -act={install | uninstall}";

        public const string CMD_PARAM_ACT = "-act";
        public const string CMD_VALUE_INSTALL = "install";
        public const string CMD_VALUE_UNINSTALL = "uninstall";
    }

    public enum ErroCodes : int
    {
        SUCCESS = 0,
        ERR_BAD_DRIVE = 1,

        ERR_NOT_IMP = 555,
        ERR_UNDEFINED = 999
    }
}
