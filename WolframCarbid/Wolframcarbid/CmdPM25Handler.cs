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
using System.Data;
using System.Data.SqlClient;

namespace Wolframcarbid
{
    public struct AirPollution
    {
        public float AQI;
        public float NO;
        public float NO2;
        public float NOx;
        public float O3;
        public float CO;
        public float SO2;
        public float PM2_5;
        public float PM10;
        public float WindSpeed;
        public float WindDirec;
        public string strTimeStamp;
    }

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

        private bool ExecuteSQLCmd(string strDatabase, string strSqlStatement)
        {
            bool bRet = false;
            string strConnect = "Server=.\\SQLEXPRESS;Trusted_Connection=yes;database=" + strDatabase;
            SqlConnection sqlConn = new SqlConnection(strConnect);
            SqlCommand sqlCommand = new SqlCommand(strSqlStatement, sqlConn);

            try
            {
                sqlConn.Open();
                sqlCommand.ExecuteNonQuery();
                Trace.WriteLine("SQL Staement(" + strSqlStatement + ") is executed successfully");
                bRet = true;
            }
            catch (Exception e)
            {
                Trace.WriteLine("An exception was thrown during json parsing: " + e.ToString());
            }
            finally
            {
                if (sqlConn.State == ConnectionState.Open)
                {
                    sqlConn.Close();
                }
            }
            return bRet;
        }

        private bool IsDBExisted()
        {
            string strStatement = "SELECT * FROM dbo.AirPollution";
            return ExecuteSQLCmd(CDataBaseConstants.WOLFRAMCARBID_DB, strStatement);
        }

        private bool CreateDatabase()
        {
            string strStatement = "CREATE DATABASE Wolframcarbid";
            return ExecuteSQLCmd(CDataBaseConstants.MSATER_DB, strStatement);
        }

        private bool CreateTable()
        {
            string strStatement = "CREATE TABLE AirPollution" +
                "(ID int PRIMARY KEY NOT NULL IDENTITY(1,1)," +
                "AQI FLOAT, NO FLOAT, NO2 FLOAT, NOx FLOAT," +
                "CO FLOAT, SO2 FLOAT, O3 FLOAT, " +
                "WindSpeed FLOAT, WinDirection FLOAT, DataTime datetime UNIQUE NOT NULL)";

            return ExecuteSQLCmd(CDataBaseConstants.WOLFRAMCARBID_DB, strStatement);
        }

        private bool UpdateDatabase(AirPollution airPollution)
        {
            string strStatement = "INSERT INTO AirPollution VALUES (" +
                airPollution.AQI.ToString() + ", " +
                airPollution.NO.ToString() + ", " +
                airPollution.NO2.ToString() + ", " +
                airPollution.NOx.ToString() + ", " +
                airPollution.CO.ToString() + ", " +
                airPollution.SO2.ToString() + ", " +
                airPollution.O3.ToString() + ", " +
                airPollution.WindSpeed.ToString() + ", " +
                airPollution.WindDirec.ToString() + ", " +
                "'" + airPollution.strTimeStamp + "')";
            return ExecuteSQLCmd(CDataBaseConstants.WOLFRAMCARBID_DB, strStatement);
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
                string strJdata = stmReader.ReadToEnd();

                JObject jEpaData = (JObject)JsonConvert.DeserializeObject(strJdata);

                if (stmReader != null)
                    stmReader.Close();

                JArray jArr = (JArray)jEpaData["feeds"];
                AirPollution airPollution = new AirPollution();

                foreach (JObject jObj in jArr.Children<JObject>())
                {
                    if (jObj["SiteEngName"] != null)
                    {
                        if (m_strLocation.CompareTo((string)jObj["SiteEngName"]) != 0)
                            continue;
                    }
                    else
                        continue;

                    if (jObj["NO"] != null)
                        airPollution.NO = (float)jObj["NO"];
                    if (jObj["NO2"] != null)
                        airPollution.NO2 = (float)jObj["NO2"];
                    if (jObj["NOx"] != null)
                        airPollution.NOx = (float)jObj["NOx"];
                    if (jObj["AQI"] != null)
                        airPollution.AQI = (float)jObj["AQI"];
                    if (jObj["CO"] != null)
                        airPollution.CO = (float)jObj["CO"];
                    if (jObj["SO2"] != null)
                        airPollution.SO2 = (float)jObj["SO2"];
                    if (jObj["O3"] != null)
                        airPollution.O3 = (float)jObj["O3"];
                    if (jObj["PM2_5"] != null)
                        airPollution.PM2_5 = (float)jObj["PM2_5"];
                    if (jObj["PM10"] != null)
                        airPollution.PM10 = (float)jObj["PM10"];
                    if (jObj["WindSpeed"] != null)
                        airPollution.WindSpeed = (float)jObj["WindSpeed"];
                    if (jObj["WindDirec"] != null)
                        airPollution.WindDirec = (float)jObj["WindDirec"];
                    if (jObj["timestamp"] != null)
                        airPollution.strTimeStamp = (string)jObj["timestamp"];
                }

                if (!IsDBExisted())
                {
                    CreateDatabase();
                    CreateTable();
                }

                UpdateDatabase(airPollution);

            }
            catch (Exception e)
            {
                nRetCode = ErrorCodes.UNABLE_TO_PARSE_DATA;
                Trace.WriteLine("An exception was thrown during json parsing: " + e.ToString());
            }

            return nRetCode;
        }

        public override string GetUsage()
        {
            return m_strUsage;
        }
    }
}
