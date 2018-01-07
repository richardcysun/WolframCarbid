using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Wolframcarbid
{
    class CWcCommand
    {
        private string[] m_strRawCmd;
        private string m_strCmdName;
        private bool m_bWellFormed;
        private Dictionary<string, string> m_dictNameValuePair;

        public CWcCommand(string[] strCmd)
        {
            m_strRawCmd = strCmd;
            m_bWellFormed = false;

            Parse();
        }

        public bool IsWellFormed()
        {
            return m_bWellFormed;
        }

        public string GetCmdName()
        {
            return m_strCmdName;
        }

        public string[] GetRawCmd()
        {
            return m_strRawCmd;
        }

        private void Parse()
        {
            Dictionary<string, string> dictNameValuePair = new Dictionary<string, string>();

            //-c=cmd, at least one parameter
            if (m_strRawCmd.Length < 1)
            {
                Trace.WriteLine("Mal-formed");   
                return;
            }

            //Let's assume the incoming parameters are good
            m_bWellFormed = true;
            int nCmdPos = 0;
            foreach (string element in m_strRawCmd)
            {
                string[] seperators = new string[] { "=" };
                string[] strNamedValuePair = element.Split(seperators, StringSplitOptions.RemoveEmptyEntries);

                if (strNamedValuePair.Length < 2)
                {
                    Trace.WriteLine("Not a qualified -name=value format");
                    m_bWellFormed = false;
                    break;
                }
                
                if ((strNamedValuePair[0].CompareTo(CCmdConstants.CMD_PREFIX) == 0) && (nCmdPos == 0))
                {
                    //The -c=cmd is at the first named-value pair, good call
                    m_strCmdName = strNamedValuePair[1];
                }
                else if ((strNamedValuePair[0].CompareTo(CCmdConstants.CMD_PREFIX) == 0) && (nCmdPos != 0))
                {
                    Trace.WriteLine("The - c=cmd is not at the first position");
                    m_bWellFormed = false;
                    break;
                }
                else if ((strNamedValuePair[0].CompareTo(CCmdConstants.CMD_PREFIX) != 0) && (nCmdPos == 0))
                {
                    Trace.WriteLine("The first position is not the -c=cmd");
                    m_bWellFormed = false;
                    break;
                }
                else
                {
                    //The rest named-value pair
                    if (dictNameValuePair.ContainsKey(strNamedValuePair[0]))
                    {
                        Trace.WriteLine("Duplicated commands");
                        m_bWellFormed = false;
                        break;
                    }
                    dictNameValuePair.Add(strNamedValuePair[0], strNamedValuePair[1]);
                }

                nCmdPos++;
            }

            if (m_bWellFormed)
                m_dictNameValuePair = dictNameValuePair;
        }

        public string GetValueByParam(string strParam)
        {
            string strValue = "";
            if (m_dictNameValuePair.ContainsKey(strParam))
                strValue = m_dictNameValuePair[strParam];

            return strValue;
        }
    }
}
