using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;

namespace Wolframcarbid
{
    //User -> WolCarbid.exe (normal user)
    //           ^
    //           |
    //   CBusCmdHandler runs here
    class CBusCmdHandler : CAbstractCmdHandler
    {
        private string m_strRouteName;
        private string m_strStopName;
        private bool m_bInBound;    //Inbound=Go back to bus terminal: true; otherwise: false

        public CBusCmdHandler()
        {
            //This command can do task by itself
            m_bSelfSustainedCmd = true;
            m_strUsage = CCmdConstants.CMD_BUS_USAGE;
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
            strTemp = m_wcCmd.GetValueByParam(CCmdConstants.CMD_PARAM_ROUTE);
            if (strTemp.Length == 0)
                return;

            m_strRouteName = strTemp;

            strTemp = m_wcCmd.GetValueByParam(CCmdConstants.CMD_PARAM_STOP);
            if (strTemp.Length == 0)
                return;

            m_strStopName = strTemp;

            strTemp = m_wcCmd.GetValueByParam(CCmdConstants.CMD_PARAM_BOUND);
            if (strTemp.Length == 0)
                return;

            if (strTemp.CompareTo(CCmdConstants.CMD_VALUE_IN) == 0)
                m_bInBound = true;
            else if (strTemp.CompareTo(CCmdConstants.CMD_VALUE_OUT) == 0)
                m_bInBound = false;
            else
                return;//abort if neither inbound nor outbound

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

        static async Task<string> GetRouteIdAsync(string strRoute)
        {
            string strRouteId_URI = "http://data.ntpc.gov.tw/od/data/api/67BB3C2B-E7D1-43A7-B872-61B2F082E11B?$format=json&$filter=nameZh%20eq%20" + strRoute;
            //That's strange, it seems URI doesn't need additional UTF-8 conversion and URL encoding
            //byte[] bytes = Encoding.Default.GetBytes(strRouteId_URI);
            //strRouteId_URI = Encoding.UTF8.GetString(bytes);

            HttpClient httpClient = new HttpClient();
            HttpResponseMessage httpResponse = await httpClient.GetAsync(strRouteId_URI);

            httpResponse.EnsureSuccessStatusCode();

            string strResBody = await httpResponse.Content.ReadAsStringAsync();

            return strResBody;
        }

        static async Task<string> GetStopIdAsync(string strRouteId, string strStopName)
        {
            string strStopId_URI = "http://data.ntpc.gov.tw/od/data/api/62519D6B-9B6D-43E1-BFD7-D66007005E6F?$format=json&$filter=routeId%20eq%20" + strRouteId + "%20and%20nameZh%20eq%20" + strStopName;
            //byte[] bytes = Encoding.Default.GetBytes(strStopId_URI);
            //strStopId_URI = Encoding.UTF8.GetString(bytes);

            HttpClient httpClient = new HttpClient();
            HttpResponseMessage httpResponse = await httpClient.GetAsync(strStopId_URI);

            httpResponse.EnsureSuccessStatusCode();

            string strResBody = await httpResponse.Content.ReadAsStringAsync();

            return strResBody;
        }

        static async Task<string> GetStopEtaAsync(string strRouteId, string strStopId)
        {
            string strStopEta_URI = "http://data.ntpc.gov.tw/od/data/api/245793DB-0958-4C10-8D63-E7FA0D39207C?$format=json&$filter=RouteID%20eq%20" + strRouteId + "%20and%20StopID%20eq%20" + strStopId;

            HttpClient httpClient = new HttpClient();
            HttpResponseMessage httpResponse = await httpClient.GetAsync(strStopEta_URI);

            httpResponse.EnsureSuccessStatusCode();

            string strResBody = await httpResponse.Content.ReadAsStringAsync();

            return strResBody;
        }

        public override ErrorCodes ProcessSelfSustainedCmd()
        {
            ErrorCodes nRetCode = ErrorCodes.ERR_NOT_IMP;
            bool bFound = false;
            string strRouteId = "";
            string strStopId = "", strGoBack = "", strEstimate = "";

            string strRouteRet = GetRouteIdAsync(m_strRouteName).GetAwaiter().GetResult();
            try
            {
                if (strRouteRet.Length > 0)
                {
                    dynamic jsonRoute = JValue.Parse(strRouteRet);
                    strRouteId = jsonRoute[0].Id;
                }
            }
            catch (Exception e)
            {
                nRetCode = ErrorCodes.UNABLE_TO_PARSE_DATA;
                Console.WriteLine("An exception was thrown during json parsing:\n" + e.ToString());
            }

            string strStopRet = GetStopIdAsync(strRouteId, m_strStopName).GetAwaiter().GetResult();
            try
            {
                if (strStopRet.Length > 0)
                {
                    dynamic jsonStop = JValue.Parse(strStopRet)[0];
                    strStopId = jsonStop.Id;
                    strGoBack = jsonStop.goBack;

                    if ((m_bInBound && (strGoBack.CompareTo("1") == 0)) || (!m_bInBound && (strGoBack.CompareTo("0") == 0)))
                    {
                        bFound = true;
                    }
                    else
                    {
                        jsonStop = JValue.Parse(strStopRet)[0].Next;
                        if (!bFound && (jsonStop != null))
                        {
                            strStopId = jsonStop.Id;
                            strGoBack = jsonStop.goBack;
                            if ((m_bInBound && (strGoBack.CompareTo("1") == 0)) || (!m_bInBound && (strGoBack.CompareTo("0") == 0)))
                            {
                                //It should be true, but in case
                                bFound = true;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                nRetCode = ErrorCodes.UNABLE_TO_PARSE_DATA;
                Console.WriteLine("An exception was thrown during json parsing:\n" + e.ToString());
            }

            if (bFound)
            {
                string strEtaRet = GetStopEtaAsync(strRouteId, strStopId).GetAwaiter().GetResult();
                try
                {
                    if (strEtaRet.Length > 0)
                    {
                        dynamic jsonEta = JValue.Parse(strEtaRet);
                        strEstimate = jsonEta[0].EstimateTime;
                        int nMin = Int32.Parse(strEstimate);

                        if (nMin > 0)
                        {
                            string strMsg = "Estimate arrival time: " + (nMin/60) + " min.";
                            Console.WriteLine(strMsg);
                        }
                        else if (nMin == -1)
                            Console.WriteLine("Not Departed");
                        else if (nMin == -2)
                            Console.WriteLine("Non-Stop");
                        else if (nMin == -3)
                            Console.WriteLine("Off Duty");
                        else if (nMin == -4)
                            Console.WriteLine("Out-of-Service");
                        else
                            Console.WriteLine("Undefined");
                    }
                }
                catch (Exception e)
                {
                    nRetCode = ErrorCodes.UNABLE_TO_PARSE_DATA;
                    Console.WriteLine("An exception was thrown during json parsing:\n" + e.ToString());
                }
            }
            else
            {
                nRetCode = ErrorCodes.UNABLE_TO_FIND_DATA;
                Console.WriteLine("Unable to retrieve arrival time!");
            }
            return nRetCode;
        }

        public override string GetUsage()
        {
            return m_strUsage;
        }
    }
}
