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
        string Command(string value);
    }

    public class WolframCarbidService : IWolframCarbidService
    {
        public string Command(string value)
        {
            string strRet = "I got your back";
            return strRet;
        }
    }

    class CWcCmdCommunication
    {
        public void InitWcfServer()
        {
            Trace.WriteLine("InitWcfServer >>>");
            //https://web.archive.org/web/20141027055124/http://tech.pro/tutorial/855/wcf-tutorial-basic-interprocess-communication
            Uri baseAddress = new Uri("net.pipe://localhost");
            ServiceHost wcfHost = new ServiceHost(typeof(WolframCarbidService), baseAddress);

            wcfHost.AddServiceEndpoint(typeof(IWolframCarbidService), new NetNamedPipeBinding(), "Command");
            wcfHost.Open();
            Trace.WriteLine("InitWcfServer <<<");
        }
    }
}
