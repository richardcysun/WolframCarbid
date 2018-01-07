using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Diagnostics;

namespace Wolframcarbid
{
    [ServiceContract]
    public interface IWolframCarbidService
    {
        [OperationContract]
        string Command(string[] strValues);
    }

    public class WolframCarbidService : IWolframCarbidService
    {
        public string Command(string[] strValues)
        {
            ErrorCodes nRetCode = ErrorCodes.ERR_NOT_IMP;
            CWcCommand wcCmd = new CWcCommand(strValues);

            string strRet = "";
            CWcCmdInvoker.IssueSlaveryCmdToCmdHandler(wcCmd, ref strRet, ref nRetCode);

            return strRet;
        }
    }

    class CWcfCommunication
    {
        private ServiceHost m_wcfHost;

        public void InitWcfServer()
        {
            Trace.WriteLine("InitWcfServer >>>");
            //https://web.archive.org/web/20141027055124/http://tech.pro/tutorial/855/wcf-tutorial-basic-interprocess-communication
            Uri baseAddress = new Uri("net.pipe://localhost");
            m_wcfHost = new ServiceHost(typeof(WolframCarbidService), baseAddress);

            m_wcfHost.AddServiceEndpoint(typeof(IWolframCarbidService), new NetNamedPipeBinding(), "Command");
            m_wcfHost.Open();
            Trace.WriteLine("InitWcfServer <<<");
        }

        public void DeinitWcfServer()
        {
            m_wcfHost.Close();
        }

        public string SendAndRecv(string[] strCmd)
        {
            ChannelFactory<IWolframCarbidService> pipeFactory =
                new ChannelFactory<IWolframCarbidService>(new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/Command"));

            IWolframCarbidService pipeClient = pipeFactory.CreateChannel();

            return pipeClient.Command(strCmd);
        }
    }
}
