using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;

namespace Wolframcarbid
{
    //User -> WolCarbid.exe (normal user)
    //           ^
    //           |
    //   CDbInfoHandler runs here
    class CDbManifestHandler : CAbstractCmdHandler
    {
        private string m_strDbSource;
        private string m_strDbUser;
        private string m_strDbPassword;

        public CDbManifestHandler()
        {
            //This command can do task by itself
            m_bSelfSustainedCmd = true;
            m_strUsage = CCmdConstants.CMD_DB_MANIFEST_USAGE;
            m_strDbSource = "";
            m_strDbUser = "";
            m_strDbPassword = "";
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
                Trace.WriteLine("CDbManifestHandler::Already has parameters.");
                return;
            }

            if ((!m_wcCmd.IsWellFormed()) || (m_wcCmd.GetCmdName().CompareTo(cmdName) != 0))
            {
                Trace.WriteLine("CDbManifestHandler::Bad command inputs.");
                return;
            }

            string strTemp;
            strTemp = m_wcCmd.GetValueByParam(CCmdConstants.CMD_PARAM_SOURCE);
            if (strTemp.Length == 0)//Mandatory parameter
            {
                Trace.WriteLine("CDbManifestHandler::Can't find database source name");
                return;
            }
            m_strDbSource = strTemp;

            strTemp = m_wcCmd.GetValueByParam(CCmdConstants.CMD_PARAM_USER);
            if (strTemp.Length != 0)//Optinal parameter
            {
                m_strDbUser = strTemp;
            }
            strTemp = m_wcCmd.GetValueByParam(CCmdConstants.CMD_PARAM_PASSWORD);
            if (strTemp.Length != 0)
            {
                m_strDbPassword = strTemp;
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

        public override ErrorCodes ProcessSlaveryCmd(CWcCommand wcCmd, ref string strResMsg)
        {
            return ErrorCodes.SUCCESS;
        }

        public override ErrorCodes ProcessSelfSustainedCmd()
        {
            ErrorCodes nRetCode = ErrorCodes.SUCCESS;

            string strXmlPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\" + CServiceConstants.CONFIG_FILE;
            Trace.WriteLine(strXmlPath);

            XmlDocument xmlWC = new XmlDocument();
            xmlWC.Load(strXmlPath);
            XmlNode nodeDb = xmlWC.DocumentElement.SelectSingleNode("/Wolframcarbid/Database");

            if (nodeDb[CDataBaseConstants.SRC] != null)
            {
                nodeDb[CDataBaseConstants.SRC].InnerText = m_strDbSource;
            }
            else
            {
                XmlElement elem;
                elem = xmlWC.CreateElement(CDataBaseConstants.SRC);
                elem.InnerText = m_strDbSource;
                nodeDb.AppendChild(elem);
            }

            if (m_strDbUser.Length > 0)
            {
                if (nodeDb[CDataBaseConstants.USER] != null)
                {
                    nodeDb[CDataBaseConstants.USER].InnerText = m_strDbUser;
                }
                else
                {
                    XmlElement elem;
                    elem = xmlWC.CreateElement(CDataBaseConstants.USER);
                    elem.InnerText = m_strDbUser;
                    nodeDb.AppendChild(elem);
                }
            }

            if (m_strDbPassword.Length > 0)
            {
                string strCiherPwd = CWCCrypt.Encrypt(m_strDbPassword);
                if (nodeDb[CDataBaseConstants.PASSWORD] != null)
                {
                    nodeDb[CDataBaseConstants.PASSWORD].InnerText = strCiherPwd;
                }
                else
                {
                    XmlElement elem;
                    elem = xmlWC.CreateElement(CDataBaseConstants.PASSWORD);
                    elem.InnerText = strCiherPwd;
                    nodeDb.AppendChild(elem);
                }
            }
            xmlWC.Save(strXmlPath);

            Trace.WriteLine("CDbManifestHandler::ProcessSelfSustainedCmd >>>" + nRetCode + "<<<");
            Console.WriteLine("Database manifest has been successfully set");
            return nRetCode;
        }

        public override string GetUsage()
        {
            return m_strUsage;
        }
    }
}
