using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ServiceProcess;
using System.Configuration.Install;
using System.Reflection;
using System.Timers;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;

namespace Wolframcarbid
{
    public class CWcService : ServiceBase
    {
        CWcfCommunication m_wcfServer;
        Timer m_Timer;

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
                Trace.WriteLine("An exception was thrown during service uninstallation: " + e.ToString());
            }
        }

        public CWcService()
        {
            //nothing here, just an polymorphism constructor to be different from CWcService(bool bInstall)
        }

        private void LaunchProcess(string strArgument)
        {
            var process = Process.GetCurrentProcess();
            string strProcessPath = process.MainModule.FileName;
            strProcessPath = strProcessPath.Replace(".vshost.", ".");

            ProcessStartInfo processInfo = new ProcessStartInfo(strProcessPath);
            processInfo.Arguments = strArgument;

            Trace.WriteLine(strProcessPath);
            Trace.WriteLine(strArgument);

            try
            {
                Process.Start(processInfo);
            }
            catch (Exception e)
            {
                Trace.WriteLine("An exception was thrown during service installation: " + e.ToString());
            }
        }

        private void ParseTasks()
        {
            Trace.WriteLine("ParseTasks >>>");

            try
            {
                string strXmlPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\" + CServiceConstants.CONFIG_FILE;
                Trace.WriteLine(strXmlPath);

                XmlDocument xmlWC = new XmlDocument();
                xmlWC.Load(strXmlPath);
                XmlNode nodeTask = xmlWC.DocumentElement.SelectSingleNode("/Wolframcarbid/Tasks/Task");

                while (nodeTask != null)
                {
                    string strPeriod = "", strCmd = "", strLatestExecution = "";
                    double fPeriodInSec = 0.0;
                    DateTime localDate = DateTime.Now;
                    DateTime latestExec = new DateTime(2010, 1, 1);
                    if (nodeTask[CTaskConstants.PERIOD] != null)
                    {
                        strPeriod = nodeTask[CTaskConstants.PERIOD].InnerText;
                        fPeriodInSec = Convert.ToDouble(strPeriod);
                    }
                    if (nodeTask[CTaskConstants.COMMAND] != null)
                        strCmd = nodeTask[CTaskConstants.COMMAND].InnerText;

                    if (nodeTask[CTaskConstants.LATEST_EXEC] != null)
                    {
                        strLatestExecution = nodeTask[CTaskConstants.LATEST_EXEC].InnerText;
                        if (strLatestExecution.Length > 0)
                            latestExec = Convert.ToDateTime(strLatestExecution);
                    }

                    TimeSpan tsDiff = localDate.Subtract(latestExec);

                    double fSecs = tsDiff.TotalSeconds;

                    if (fSecs > fPeriodInSec)
                    {
                        Trace.WriteLine("Time is up!!");
                        LaunchProcess(strCmd);

                        CultureInfo culture = new CultureInfo("en-US");
                        if (nodeTask[CTaskConstants.LATEST_EXEC] != null)
                        {
                            nodeTask[CTaskConstants.LATEST_EXEC].InnerText = localDate.ToString(culture);
                        }
                        else
                        {
                            XmlElement elem;
                            elem = xmlWC.CreateElement(CTaskConstants.LATEST_EXEC);
                            elem.InnerText = localDate.ToString(culture);
                            nodeTask.AppendChild(elem);
                        }
                        xmlWC.Save(strXmlPath);
                    }

                    nodeTask = nodeTask.NextSibling;
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine("An exception was thrown: " + e.ToString());
            }
            Trace.WriteLine("ParseTasks <<<");
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Trace.WriteLine("OnTimedEvent >>>");
            ParseTasks();
            Trace.WriteLine("OnTimedEvent <<<");
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

            m_Timer = new Timer(30000);
            m_Timer.Elapsed += OnTimedEvent;
            m_Timer.AutoReset = true;
            m_Timer.Enabled = true;

            Trace.WriteLine("OnStart <<<");
        }

        protected override void OnStop()
        {
            base.OnStop();

            m_Timer.Close();
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
                "      -wc=dbm (normal user privilege, set database manifest)",
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
            cmdFactory.RegisterHandler(CCmdConstants.CMD_DB_MANIFEST_NAME, new CDbManifestHandler());
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
