using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ServiceProcess;
using System.Configuration.Install;
using System.Reflection;

namespace Wolframcarbid
{
    public class CWcService : ServiceBase
    {
        CWcfCommunication m_wcfServer;

        //bInstall: true
        //          Self-installing Wolframcarbid
        //bInstall: false
        //          Self-uninstalling Wolframcarbid
        public CWcService(bool bInstall)
        {
            try
            {
                if (bInstall)
                {
                    Trace.WriteLine("Prepare to install");
                    ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                }
                else
                {

                    Trace.WriteLine("Prepare to uninstall");
                    ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception was thrown during service uninstallation:\n" + e.ToString());
            }
        }

        public CWcService()
        {
            //nothing here, just an polymorphism constructor to be different from CWcService(bool bInstall)
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
            Trace.WriteLine("OnStart >>>");

            CWcCmdFactory cmdFactory = new CWcCmdFactory();
            Program.AddCmdHandlerToFactory(cmdFactory);

            m_wcfServer = new CWcfCommunication();
            m_wcfServer.InitWcfServer();

            CWcCmdInvoker.Init(cmdFactory);

            Trace.WriteLine("OnStart <<<");
        }

        protected override void OnStop()
        {
            base.OnStop();

            m_wcfServer.DeinitWcfServer();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Trace.WriteLine("No commands");
                PrintUsage();
            }
            else if (args[0].ToString() == CServiceConstants.SERVICE_MODE)
            {
                //service mode
                Trace.WriteLine("Process Service Mode");
                CWcService wcService = new CWcService();

                ServiceBase.Run(wcService);
            }
            else
            {
                Trace.WriteLine("Process User Input");
                CWcCommand wcCmd = new CWcCommand(args);

                if (wcCmd.IsWellFormed())
                {
                    ProcessUserInputs(wcCmd);
                }
                else
                    PrintUsage();
            }
        }

        private static void PrintUsage()
        {
            string[] szMsg = {
                "Usage:\n",
                "      example: \"Wolframcarbid.exe -wc=ctrl -svc=EFS \"\n",
                "      Self-Sustained Commands:",
                "      -wc=inst (Admin privilege, register tool as a Windows Service)",
                "      -wc=bus (normal user privilege, query bus status)",
                "      -wc=pm25 (normal user privilege, query particulate matter data)\n",
                "      Master-Slave Commands (must run \"inst\" command in the first place):",
                "      -wc=ctrl (normal user privilege, but can start/stop specific Service)\n" };

            foreach (string element in szMsg)
                Console.WriteLine(element);
        }

        public static void AddCmdHandlerToFactory(CWcCmdFactory cmdFactory)
        {
            cmdFactory.RegisterHandler(CCmdConstants.CMD_INST_SVC_NAME, new CInstSvcCmdHandler());
            cmdFactory.RegisterHandler(CCmdConstants.CMD_CTRL_SVC_NAME, new CCtrlSvcCmdHandler());
            cmdFactory.RegisterHandler(CCmdConstants.CMD_CTRL_SLAVE_SVC_NAME, new CCtrlSlaveSvcCmdHandler());
            cmdFactory.RegisterHandler(CCmdConstants.CMD_BUS_NAME, new CBusCmdHandler());
            cmdFactory.RegisterHandler(CCmdConstants.CMD_PM25_NAME, new CmdPM25CmdHandler());
        }

        //In C++, using call by reference should be a better approach in term of performance.
        //However, in the CLR, according to community forum, the call by reference seems to be
        //only used when you need changed value in called function.
        private static void ProcessUserInputs(CWcCommand wcCmd)
        {
            Trace.WriteLine("ProcessUserInputs >>>");

            CWcCmdFactory cmdFactory = new CWcCmdFactory();
            AddCmdHandlerToFactory(cmdFactory);

            CWcCmdInvoker.Init(cmdFactory);

            if (CWcCmdInvoker.CheckCommand(wcCmd))
            {
                if (CWcCmdInvoker.IsSelfSusatinedCommand(wcCmd))
                {
                    ErrorCodes nRetCode = ErrorCodes.ERR_UNDEFINED;
                    CWcCmdInvoker.IssueSelfSustainedCmdToHandler(wcCmd, ref nRetCode);

                    string strMsg = string.Format("The result of command({0}) is {1}", wcCmd.GetCmdName(), nRetCode);
                    Trace.WriteLine(strMsg);
                }
                else
                {
                    string strResMsg = CWcCmdInvoker.FowardCmdToMasterService(wcCmd);
                    Console.WriteLine(strResMsg);
                }
            }
            else
            {
                Trace.WriteLine("Invalid Command");
                string strUsgae = CWcCmdInvoker.GetUsage(wcCmd);

                Console.WriteLine(strUsgae);
            }
            Trace.WriteLine("ProcessUserInputs <<<");
        }
    }
}
