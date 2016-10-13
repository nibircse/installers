﻿using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using Microsoft.Win32;

namespace vs_preinstall
{
   
    public partial class Preinstall_check : Form
    {
        public Boolean res = true;
        public int hostCores; //number of logical processors
        public Boolean host64;
        public string hostOSversion;
        public string hostOSversion_user;
        private long hostRam;
        private string hostVT;
        private string shortVersion;
        private string vboxVersion;
        private string vb_version2fit = "5.1.0";
        
        public Preinstall_check()
        {
            InitializeComponent();
            showing();
        }

        private void showing()
        {
            hostOSversion = Environment.OSVersion.Version.ToString();
            hostOSversion_user = OS_name();
            shortVersion = hostOSversion.Substring(0, 3);
            hostCores = Environment.ProcessorCount; //number of logical processors
            host64 = Environment.Is64BitOperatingSystem;
            vboxVersion = vbox_version();

            hostRam = (long)new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory / 1024 / 1024;
            hostVT = check_vt();

            l_Proc.Text = hostCores.ToString();
            l_RAM.Text = hostRam.ToString();
            l_S64.Text = host64.ToString();
            l_OS.Text = hostOSversion_user;//shortVersion;//hostOSversion.ToString();
            l_VT.Text = hostVT;
            l_VB.Text = vboxVersion;

            tb_Info.Text = "";// "* This value may need to be checked in BIOS. If installation fails, check if  hardware support for virtualization(VT-x/AMD-V) is allowed in BIOS.";
            checking();
        }
        private void checking()
        {
            if (hostCores < 2)
            {
                l_Proc.ForeColor = Color.Red;
                res = false;
            } else
            {
                l_Proc.ForeColor = Color.Green;
            }

            if ((long)hostRam < 4000) //2000
            {
                l_RAM.ForeColor = Color.Red;
                res =  false;
            } else
            {
                l_RAM.ForeColor = Color.Green;
            }
            if (!host64)
            {
                l_S64.ForeColor = Color.Red;
                res = false;
            } else
            {
                l_S64.ForeColor = Color.Green;
            }
            
            if ( !hostVT.ToLower().Contains("true") && shortVersion != "6.1")
            {
                l_VT.ForeColor = Color.Red;
                res = false;
            }

            if (!vbox_version_fit(vb_version2fit, l_VB.Text))
            {
                l_VB.ForeColor = Color.Red;
                res = false;
            }
            else
            {
                l_VB.ForeColor = Color.Green;
            }

            if (!vbox_version_fit("6.1", shortVersion))
            {
                l_OS.ForeColor = Color.Red;
                res = false;
            }
            else
            {
                l_OS.ForeColor = Color.Green;
            }

            if (res)
            {
                if (shortVersion != "6.1")
                {
                    label5.Text = "Subutai Social can be installed on Your system. Press Next button";
                    label5.ForeColor = Color.Green;
                    l_VT.ForeColor = Color.Green;
                    //tb_Info.Text = " ";
                   
                    tb_Info.Text += Environment.NewLine;
                    tb_Info.Text += "Please turn off SmartScreen and Antivirus software for installation time.";
                    tb_Info.Text += Environment.NewLine;
                    tb_Info.Text += Environment.NewLine;
                    tb_Info.Text += "DHCP server must to be running on the local network.";
                    tb_Info.Text += Environment.NewLine;
                    tb_Info.Text += Environment.NewLine;
                    tb_Info.Text += $"Subutai needs Oracle VirtualBox version {vb_version2fit} or higher. Please update or uninstall old version and restart Windows!";
                }
                else {
                    label5.Text = "Impossible to check if VT-x is enabled.";
                    label5.ForeColor = Color.Blue;
                    l_VT.ForeColor = Color.DarkBlue;
                    tb_Info.Text = "Can not define if VT-x is enabled.";
                    tb_Info.Text += Environment.NewLine;
                    tb_Info.Text += "If not sure, press Next button, cancel installation and check in BIOS.";
                    tb_Info.Text += Environment.NewLine;
                    tb_Info.Text += Environment.NewLine;
                    tb_Info.Text += "If VT-x enabled, please turn off Antivirus software for installation time!";
                }
                tb_Info.Text += Environment.NewLine;
                tb_Info.Text += Environment.NewLine;
                //tb_Info.Text += "If installation fails or interrupted, please run Start->All Applications->Subutai folder->Uninstall or uninstall from Control Panel.";
                //tb_Info.Text += Environment.NewLine;
                //tb_Info.Text += Environment.NewLine;
                //tb_Info.Text += "Press Next button to proceed.";
            } else
            {
                label5.Text = "Sorry, Subutai Social can not be installed. Press Next button and cancel installation";
                label5.ForeColor = Color.Red;
                tb_Info.Text = "Please check Subutai system requirements.";
                tb_Info.Text += Environment.NewLine;
                tb_Info.Text += Environment.NewLine;
                tb_Info.Text += $"Subutai needs Oracle VirtualBox version {vb_version2fit} or higher. Please update or uninstall old version and restart Windows!";
                tb_Info.Text += Environment.NewLine;
                tb_Info.Text += Environment.NewLine;
                tb_Info.Text += "Press Next button to exit.";
             }
         }

        private String check_vt()
        {
            ManagementClass managClass = new ManagementClass("win32_processor");
            ManagementObjectCollection managCollec = managClass.GetInstances();
            foreach (ManagementObject managObj in managCollec)
            {
                foreach (var prop in managObj.Properties)
                {
                    if (prop.Name == "VirtualizationFirmwareEnabled")
                    {
                        return prop.Value.ToString();
                    }
                    //Console.WriteLine("Property Name: {0} Value: {1}", prop.Name, prop.Value);
                }
            }
          return "Not found";
        }

        public string vbox_version()
        {
            //HKEY_LOCAL_MACHINE\SOFTWARE\Oracle\VirtualBox
            string subkey = "SOFTWARE\\Oracle\\VirtualBox";
            RegistryKey rk = Registry.LocalMachine.OpenSubKey(subkey);
            if (rk == null)
            {
                return "0";
            }
            var vers = rk.GetValue("Version");
            return vers.ToString();
        }

        public bool vbox_version_fit(string versFit, string versCheck)
        {
            string[] vb = versFit.Split('.');
            string[] vb_check = versCheck.Split('.');
            if (versCheck == "0")//
            {
                return true;
            }
            if (versCheck.Equals(versFit, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            int bound = Math.Min(vb.Length, vb_check.Length);
            int[] vi = new int[bound];//minimal version
            int[] vi_check = new int[bound];//checked version
            for (int i = 0; i < bound; ++i)
            {
                if (!Int32.TryParse(vb[i], out vi[i]) || !Int32.TryParse(vb_check[i], out vi_check[i]))
                {
                    bound = i;
                    break;
                }
            }

            for (int i = 0; i < bound; ++i)
            {
                if (i < 2 && vi_check[i] < vi[i])
                {
                    return false;
                }
                if (i < 2 && vi_check[i] > vi[i])
                {
                    return true;
                }
                if (i > 2)//previous is equal
                {
                    if (vi_check[i] < vi[i])
                        return false;
                }
             }
            return true;
        }

        public string OS_name()
        {
            String subKey = @"SOFTWARE\Wow6432Node\Microsoft\Windows NT\CurrentVersion";
            RegistryKey key = Registry.LocalMachine.OpenSubKey(subKey);
            if (key == null)
            {
                return "Unknown";
            }
            var vers = key.GetValue("ProductName");
            return vers.ToString();
        }

        private void Preinstall_check_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e) //Next
        {
            if (!res)//(label5.Text.Contains("Sorry"))
            {
                //MessageBox.Show("Sorry", "No", MessageBoxButtons.OK);
                Environment.Exit(1);
            }
            else
            {
                //MessageBox.Show("Yes", "Yes", MessageBoxButtons.OK);
                //Environment.Exit(0);
                this.Close();
            }
        }
    }
}
