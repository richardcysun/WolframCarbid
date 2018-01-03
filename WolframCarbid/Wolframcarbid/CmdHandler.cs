using System;
//using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ServiceProcess;
using System.Configuration.Install;
using System.Collections;

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
        public abstract ErroCodes ProcessSelfSustainedCmd();
        public abstract string GetUsage();

        protected abstract void RetrieveCmdParam(string cmdName);
    }

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
            //neither insatll, nor uninstall
            if (strTemp.CompareTo(CCmdConstants.CMD_VALUE_INSTALL) == 0)
                m_bCmdInstall = true;
            else if (strTemp.CompareTo(CCmdConstants.CMD_VALUE_UNINSTALL) == 0)
                m_bCmdInstall = false;
            else
                return;

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

        public override ErroCodes ProcessSelfSustainedCmd()
        {
            ErroCodes nRetCode = ErroCodes.ERR_NOT_IMP;

            Process curProcess = Process.GetCurrentProcess();
            string strPath = curProcess.MainModule.FileName;

            //This CWcService can trigger entire install or uninstall sequence

            CWcService wcService = new CWcService(m_bCmdInstall);

            return nRetCode;
        }

        public override string GetUsage()
        {
            return m_strUsage;
        }
    }
}
