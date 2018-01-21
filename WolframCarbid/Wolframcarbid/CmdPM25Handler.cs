using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.IO;

namespace Wolframcarbid
{
    class CmdPM25CmdHandler : CAbstractCmdHandler
    {
        private string m_strLocation;
        private string m_strDlFile;

        public CmdPM25CmdHandler()
        {
            //This command can do task by itself
            m_bSelfSustainedCmd = true;
            m_strUsage = CCmdConstants.CMD_PM25_USAGE;
            m_strDlFile = "epa.json";
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
            strTemp = m_wcCmd.GetValueByParam(CCmdConstants.CMD_PARAM_LOCATION);
            if (strTemp.Length == 0)
                return;

            m_strLocation = strTemp;

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
            ErrorCodes nRetCode = ErrorCodes.SUCCESS;

            try
            {
                string strRouteId_URI = "https://pm25.lass-net.org/data/last-all-epa.json";
                WebClient webClient = new WebClient();
                webClient.DownloadFile(strRouteId_URI, m_strDlFile);

                StreamReader stmReader = new StreamReader(m_strDlFile);
                string json = stmReader.ReadToEnd();

                dynamic epaData = JsonConvert.DeserializeObject(json);
                dynamic array = epaData.feeds;

                float NO = 0 , NO2 = 0, NOx = 0, AQI = 0;
                float O3 = 0, CO = 0, SO2 = 0;
                float PM2_5 = 0, PM10 = 0;
                float WindSpeed = 0, WindDirec = 0;
                string strTimeStamp;
                foreach (var item in array)
                {
                    string strSite = item.SiteEngName;
                    if (m_strLocation.CompareTo(strSite) == 0)
                    {
                        if (item.NO != null)
                            NO = item.NO;
                        if (item.NO2 != null)
                            NO2 = item.NO2;
                        if (item.NOx != null)
                            NOx = item.NOx;
                        if (item.AQI != null)
                            AQI = item.AQI;
                        if (item.CO != null)
                            CO = item.CO;
                        if (item.SO2 != null)
                            SO2 = item.SO2;
                        if (item.O3 != null)
                            O3 = item.O3;
                        if (item.PM2_5 != null)
                            PM2_5 = item.PM2_5;
                        if (item.PM10 != null)
                            PM10 = item.PM10;
                        if (item.WindSpeed != null)
                            WindSpeed = item.WindSpeed;
                        if (item.WindDirec != null)
                            WindDirec = item.WindDirec;
                        if (item.timestamp != null)
                            strTimeStamp = item.timestamp;
                        break;
                    }
                }

            }
            catch (Exception e)
            {
                nRetCode = ErrorCodes.UNABLE_TO_PARSE_DATA;
                Console.WriteLine("An exception was thrown during json parsing:\n" + e.ToString());
            }

            return nRetCode;
        }

        public override string GetUsage()
        {
            return m_strUsage;
        }
    }
}
