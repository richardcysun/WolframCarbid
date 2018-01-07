using System;
//using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration.Install;
using System.Collections;
using System.ServiceProcess;
using System.Diagnostics;

namespace Wolframcarbid
{
    abstract class CAbstractCmdHandler
    {
        protected bool m_bValid;
        protected bool m_bSelfSustainedCmd;
        protected CWcCommand m_wcCmd;
        protected string m_strUsage;

        public CAbstractCmdHandler()
        {
            m_bValid = false;
            m_bSelfSustainedCmd = false;
        }

        public abstract void SetCommand(CWcCommand wcCmd);
        public abstract bool IsCmdValid();
        public abstract bool IsSelfSustainedCmd();
        public abstract ErrorCodes ProcessSlaveryCmd(CWcCommand wcCmd, ref string strResMsg);
        public abstract ErrorCodes ProcessSelfSustainedCmd();
        public abstract string GetUsage();

        protected abstract void RetrieveCmdParam(string cmdName);
    }

    //User -> WolCarbid.exe (admin)
    //           ^
    //           |
    //   CInstSvcCmdHandler runs here
    class CInstSvcCmdHandler : CAbstractCmdHandler
    {
        private bool m_bCmdInstall;

        public CInstSvcCmdHandler()
        {
            //This command can do task by itself
            m_bSelfSustainedCmd = true;
            m_bCmdInstall = false;
            m_strUsage = CCmdConstants.CMD_INST_SVC_USAGE;
        }

        public override void SetCommand(CWcCommand wcCmd)
        {
            m_wcCmd = wcCmd;
            RetrieveCmdParam(wcCmd.GetCmdName());
        }

        protected override void RetrieveCmdParam(string cmdName)
        {
            if (m_bValid)
            {
                Trace.WriteLine("Already has parameters.");
                return;
            }

            if((!m_wcCmd.IsWellFormed()) || (m_wcCmd.GetCmdName().CompareTo(cmdName) != 0))
            {
                Trace.WriteLine("Bad command inputs.");
                return;
            }

            string strTemp;
            strTemp = m_wcCmd.GetValueByParam(CCmdConstants.CMD_PARAM_ACT);
            if (strTemp.Length == 0)
                return;

            if (strTemp.CompareTo(CCmdConstants.CMD_VALUE_INSTALL) == 0)
                m_bCmdInstall = true;
            else if (strTemp.CompareTo(CCmdConstants.CMD_VALUE_UNINSTALL) == 0)
                m_bCmdInstall = false;
            else
                return;//abort if neither install nor uninstall

            m_bValid = true;
        }

        public override bool IsCmdValid()
        {
            return m_bValid;
        }

        public override bool IsSelfSustainedCmd()
        {
            return m_bSelfSustainedCmd;
        }

        public override ErrorCodes ProcessSlaveryCmd(CWcCommand wcCmd, ref string strResMsg)
        {
            return ErrorCodes.SUCCESS;
        }

        public override ErrorCodes ProcessSelfSustainedCmd()
        {
            ErrorCodes nRetCode = ErrorCodes.ERR_NOT_IMP;

            //This CWcService can trigger entire install or uninstall sequence
            CWcService wcService = new CWcService(m_bCmdInstall);

            return nRetCode;
        }

        public override string GetUsage()
        {
            return m_strUsage;
        }
    }

    //User -> WolCarbid.exe (invoker) -> WolCarbid.exe (master) -> WolCarbid.exe (slave)
    //                                      ^
    //                                      |
    //                              CCtrlSvcCmdHandler runs here
    //                              to fork slave
    class CCtrlSvcCmdHandler : CAbstractCmdHandler
    {
        public CCtrlSvcCmdHandler()
        {
            m_bSelfSustainedCmd = false;    //This command will ask master service's assistance
            m_strUsage = CCmdConstants.CMD_CTRL_SVC_USAGE;
        }

        public override void SetCommand(CWcCommand wcCmd)
        {
            m_wcCmd = wcCmd;
            RetrieveCmdParam(wcCmd.GetCmdName());
        }

        protected override void RetrieveCmdParam(string cmdName)
        {
            if (m_bValid)
            {
                Trace.WriteLine("Already has parameters.");
                return;
            }

            if ((!m_wcCmd.IsWellFormed()) || (m_wcCmd.GetCmdName().CompareTo(cmdName) != 0))
            {
                Trace.WriteLine("Bad command inputs.");
                return;
            }

            string strTemp;
            strTemp = m_wcCmd.GetValueByParam(CCmdConstants.CMD_PARAM_ACT);
            if (strTemp.Length == 0)
            {
                Trace.WriteLine("Can't find act.");
                return;
            }

            if ((strTemp.CompareTo(CCmdConstants.CMD_VALUE_START) != 0) && (strTemp.CompareTo(CCmdConstants.CMD_VALUE_STOP) != 0))
            {
                Trace.WriteLine("Neither start nor stop.");
                return; //abort if neither start nor stop
            }

            strTemp = m_wcCmd.GetValueByParam(CCmdConstants.CMD_PARAM_SVC);
            if (strTemp.Length == 0)
            {
                Trace.WriteLine("Can't find svc.");
                return;
            }

            m_bValid = true;
        }

        public override bool IsCmdValid()
        {
            return m_bValid;
        }

        public override bool IsSelfSustainedCmd()
        {
            return m_bSelfSustainedCmd;
        }

        private void ConstructCmdArgument(CWcCommand wcCmd, ref string strArguments)
        {
            strArguments = CCmdConstants.CMD_PREFIX + "=" + CCmdConstants.CMD_CTRL_SLAVE_SVC_NAME;
            //-act={start | stop}
            strArguments = strArguments + " " + CCmdConstants.CMD_PARAM_ACT + "=" + wcCmd.GetValueByParam(CCmdConstants.CMD_PARAM_ACT);
            //-svc={name}
            strArguments = strArguments + " " + CCmdConstants.CMD_PARAM_SVC + "=\"" + wcCmd.GetValueByParam(CCmdConstants.CMD_PARAM_SVC) + "\"";
        }

        public override ErrorCodes ProcessSlaveryCmd(CWcCommand wcCmd, ref string strResMsg)
        {
            ErrorCodes nRetCode = ErrorCodes.SUCCESS;
            var process = Process.GetCurrentProcess();
            string strProcessPath = process.MainModule.FileName;
            string strProcessArg = "";
            ProcessStartInfo processInfo = new ProcessStartInfo(strProcessPath);

            ConstructCmdArgument(wcCmd, ref strProcessArg);
            processInfo.Arguments = strProcessArg;

            Trace.WriteLine(strProcessPath);
            Trace.WriteLine(strProcessArg);

            try
            {
                Process.Start(processInfo);
                strResMsg = "WolframCarbid service has successfully launched slavery process.";
            }
            catch(Exception e)
            {
                nRetCode = ErrorCodes.UNABLE_TO_LAUNCH_PROC;
                Console.WriteLine("An exception was thrown during service installation:\n" + e.ToString());
                strResMsg = "WolframCarbid was unable to launch slavery process.";
            }
            return nRetCode;
        }

        public override ErrorCodes ProcessSelfSustainedCmd()
        {
            return ErrorCodes.SUCCESS;
        }

        public override string GetUsage()
        {
            return m_strUsage;
        }
    }

    //User -> WolCarbid.exe (invoker) -> WolCarbid.exe (master) -> WolCarbid.exe (slave)
    //                                                                ^
    //                                                                |
    //                                                        CCtrlSlaveSvcCmdHandler runs here
    //                                                        to start/stop service
    class CCtrlSlaveSvcCmdHandler : CAbstractCmdHandler
    {
        private bool m_bCmdStart;
        private string m_strSvcName;

        public CCtrlSlaveSvcCmdHandler()
        {
            m_bSelfSustainedCmd = true; //This command can do task by itself
            m_bCmdStart = false;
            m_strSvcName = "";
            m_strUsage = CCmdConstants.CMD_CTRL_SLAVE_SVC_USAGE;
        }

        public override void SetCommand(CWcCommand wcCmd)
        {
            m_wcCmd = wcCmd;
            RetrieveCmdParam(wcCmd.GetCmdName());
        }

        protected override void RetrieveCmdParam(string cmdName)
        {
            if (m_bValid)
            {
                Trace.WriteLine("Already has parameters.");
                return;
            }

            if ((!m_wcCmd.IsWellFormed()) || (m_wcCmd.GetCmdName().CompareTo(cmdName) != 0))
            {
                Trace.WriteLine("Bad command inputs.");
                return;
            }

            string strTemp;
            strTemp = m_wcCmd.GetValueByParam(CCmdConstants.CMD_PARAM_ACT);
            if (strTemp.Length == 0)
            {
                Trace.WriteLine("Can't find act.");
                return;
            }
            if (strTemp.CompareTo(CCmdConstants.CMD_VALUE_START) == 0)
                m_bCmdStart = true;
            else if (strTemp.CompareTo(CCmdConstants.CMD_VALUE_STOP) == 0)
                m_bCmdStart = false;
            else
            {
                Trace.WriteLine("Neither start nor stop.");
                return; //abort if neither start nor stop
            }

            strTemp = m_wcCmd.GetValueByParam(CCmdConstants.CMD_PARAM_SVC);
            if (strTemp.Length == 0)
            {
                Trace.WriteLine("Can't find svc.");
                return;
            }
            m_strSvcName = strTemp;

            m_bValid = true;
        }

        public override bool IsCmdValid()
        {
            return m_bValid;
        }

        public override bool IsSelfSustainedCmd()
        {
            return m_bSelfSustainedCmd;
        }

        public override ErrorCodes ProcessSlaveryCmd(CWcCommand wcCmd, ref string strResMsg)
        {
            return ErrorCodes.SUCCESS;
        }

        public override ErrorCodes ProcessSelfSustainedCmd()
        {
            Trace.WriteLine("ProcessSelfSustainedCmd");
            ServiceController service = new ServiceController(m_strSvcName);

            if (m_bCmdStart)
                service.Start();
            else
                service.Stop();

            return ErrorCodes.SUCCESS;
        }

        public override string GetUsage()
        {
            return m_strUsage;
        }
    }
}
