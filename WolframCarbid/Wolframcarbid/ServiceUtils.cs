using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration.Install;
using System.ServiceProcess;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;

//I've learned so much from these two links
//http://www.c-sharpcorner.com/article/building-dynamic-service-in-C-Sharp/
//https://stackoverflow.com/questions/1449994/inno-setup-for-windows-service/1450051#1450051

namespace Wolframcarbid
{
    [RunInstaller(true)]
    public class CWcInstaller : Installer
    {
        private bool m_bInitialized;
        private ServiceProcessInstaller m_psInstaller;
        private ServiceInstaller m_svcInstaller;

        public CWcInstaller()
        {
            BeforeInstall += new InstallEventHandler(BeforeInstallEventHandler);
            BeforeUninstall += new InstallEventHandler(BeforeUninstallEventHandler);
            AfterInstall += new InstallEventHandler(AfterInstallEventHandler);

            m_bInitialized = false;
        }

        private void Init()
        {
            m_psInstaller = new ServiceProcessInstaller();
            m_svcInstaller = new ServiceInstaller();

            m_psInstaller.Account = ServiceAccount.LocalSystem;

            m_svcInstaller.DisplayName = CServiceConstants.SERVICE_DISPLAY_NAME;
            m_svcInstaller.StartType = ServiceStartMode.Automatic;
            m_svcInstaller.ServiceName = CServiceConstants.SERVICE_NAME;
            m_svcInstaller.Description = "A simple command-line interface written in C#";

            Installers.Add(m_psInstaller);
            Installers.Add(m_svcInstaller);
            m_bInitialized = true;
        }

        public override void Install(IDictionary stateSaver)
        {
            try
            {
                base.Install(stateSaver);

                Microsoft.Win32.RegistryKey regSystem, regCurentControlSet, regServices;
                Microsoft.Win32.RegistryKey regWolframCarbid;

                regSystem = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("System");
                regCurentControlSet = regSystem.OpenSubKey("CurrentControlSet");
                regServices = regCurentControlSet.OpenSubKey("Services");
                regWolframCarbid = regServices.OpenSubKey(CServiceConstants.SERVICE_NAME, true);

                string strImagePath = (string)regWolframCarbid.GetValue("ImagePath");
                if (strImagePath.Length > 0)
                {
                    strImagePath = strImagePath + " " + CServiceConstants.SERVICE_MODE;
                    regWolframCarbid.SetValue("ImagePath", strImagePath);
                }
                else
                {
                    //do nothing is ImagePath is empty
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception was thrown during service installation:\n" + e.ToString());
            }
        }

        private void BeforeInstallEventHandler(object sender, InstallEventArgs e)
        {
            if (!m_bInitialized)
            {
                Init();
            }
        }

        private void BeforeUninstallEventHandler(object sender, InstallEventArgs e)
        {
            if (!m_bInitialized)
            {
                Init();
            }
        }

        private void AfterInstallEventHandler(object sender, InstallEventArgs e)
        {
            //https://stackoverflow.com/questions/1036713/automatically-start-a-windows-service-on-install/14162063#14162063
            //ServiceController theSC = new ServiceController(CServiceConstants.SERVICE_DISPLAY_NAME);

            //if (theSC.Status.Equals(ServiceControllerStatus.Stopped))
            //{
            //    theSC.Start();
            //}

            //Somehow, above solution causes exception, so I use this one as alternative
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C net start " + CServiceConstants.SERVICE_NAME;

            Process process = new Process();
            process.StartInfo = startInfo;
            process.Start();
        }
    }
}

