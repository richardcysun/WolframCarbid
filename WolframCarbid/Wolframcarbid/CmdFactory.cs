using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Wolframcarbid
{
    class CWcCmdFactory
    {
        private Dictionary<string, CAbstractCmdHandler> m_dictCmdHandlers;

        public CWcCmdFactory()
        {
            m_dictCmdHandlers = new Dictionary<string, CAbstractCmdHandler>();
        }

        public void RegisterHandler(string cmdName, CAbstractCmdHandler cmdHandler)
        {
            if (!m_dictCmdHandlers.ContainsKey(cmdName))
            {
                string strMsg = string.Format("Register handler for command({0})", cmdName);
                Trace.WriteLine(strMsg);
                m_dictCmdHandlers.Add(cmdName, cmdHandler);
            }
        }

        public CAbstractCmdHandler GetCmdHndler(CWcCommand wcCmd)
        {
            CAbstractCmdHandler retCmdHndler = null;

            string strCmdName = wcCmd.GetCmdName();

            if( m_dictCmdHandlers.ContainsKey(strCmdName))
            {
                retCmdHndler = m_dictCmdHandlers[strCmdName];
                retCmdHndler.SetCommand(wcCmd);
            }

            return retCmdHndler;
        }
    }

    static class CWcCmdInvoker
    {
        private static CWcCmdFactory m_cmdFactory;

        public static void Init(CWcCmdFactory cmdFactory)
        {
            Trace.WriteLine("CWcCmdInvoker::Init >>>");
            m_cmdFactory = cmdFactory;
        }

        public static bool CheckCommand(CWcCommand wcCmd)
        {
            bool bRet = false;

            CAbstractCmdHandler cmdHndler = m_cmdFactory.GetCmdHndler(wcCmd);

            if (cmdHndler != null)
            {
                bRet = cmdHndler.IsCmdValid();
            }
            return bRet;
        }

        public static bool IsSelfSusatinedCommand(CWcCommand wcCmd)
        {
            bool bSelfSustained = false;
            CAbstractCmdHandler cmdHndler = m_cmdFactory.GetCmdHndler(wcCmd);

            if (cmdHndler != null)
            {
                bSelfSustained = cmdHndler.IsSelfSustainedCmd();
            }

            return bSelfSustained;
        }

        public static string FowardCmdToMasterService(CWcCommand wcCmd)
        {
            CWcfCommunication wcfClient = new CWcfCommunication();
            return wcfClient.SendAndRecv(wcCmd.GetRawCmd());
        }

        public static void IssueSelfSustainedCmdToHandler(CWcCommand wcCmd, ref ErrorCodes nRetCode)
        {
            CAbstractCmdHandler cmdHndler = m_cmdFactory.GetCmdHndler(wcCmd);

            if (cmdHndler != null)
            {
                nRetCode = cmdHndler.ProcessSelfSustainedCmd();
            }
        }

        public static void IssueSlaveryCmdToCmdHandler(CWcCommand wcCmd, ref string strResMsg, ref ErrorCodes nRetCode)
        {
            CAbstractCmdHandler cmdHndler = m_cmdFactory.GetCmdHndler(wcCmd);

            if (cmdHndler != null)
            {
                nRetCode = cmdHndler.ProcessSlaveryCmd(wcCmd, ref strResMsg);
            }
        }

        public static string GetUsage(CWcCommand wcCmd)
        {
            CAbstractCmdHandler cmdHndler = m_cmdFactory.GetCmdHndler(wcCmd);
            string strUsage ="";

            if (cmdHndler != null)
            {
                strUsage = cmdHndler.GetUsage();
            }
            return strUsage;
        }
    }
}
