﻿using System;
using System.IO;
using System.Windows.Forms;
using System.ServiceProcess;
using System.Diagnostics;
using System.Threading;
using Microsoft.Win32;
using System.Linq;
using System.Text.RegularExpressions;
using System.Security.AccessControl;
using System.Security.Principal;

namespace uninstall_clean
{
    class VBx
        //Working with virtualbox
    {
        
        public static void remove_app_vbox(string app_name)
        {
            DialogResult drs = MessageBox.Show($"Remove {app_name}?", $"Removing {app_name}",
                              MessageBoxButtons.YesNo,
                              MessageBoxIcon.Question,
                              MessageBoxDefaultButton.Button1);

            if (drs == DialogResult.No)
                return;
            string res = "";
            //VirtualBox Manager, VirtualBox Interface
            //Stop VMs

            //Remove all host-only interfaces
            remove_host_only();
            //Stop Processes/Services
            clean.StageReporter("", "Stopping VirtualBox processes");
            Process[] vboxProcesses = Process.GetProcesses();
            foreach (Process process in vboxProcesses)
            {
                if (process.ProcessName.Contains("VBox") || process.ProcessName.Contains("VirtualBox"))
                {
                    MessageBox.Show($"Process: {process.ProcessName}", "Removing processes", MessageBoxButtons.OK);
                    try
                    {
                        process.Kill();
                        Thread.Sleep(2000);
                    }
                    catch (Exception e)
                    {
                        string tmp = e.Message;
                    }
                }
            }

            //Stopping services

            //Remove drivers C:\Windows\System32\drivers
            //C:\Windows\System32\drivers\VBoxDrv.sys, VBoxNetAdp6.sys, VBoxNetLwf.sys, VBoxUSBMon.sys
           
            clean.StageReporter("", "Removing VirtualBox drivers ");

            string dirStart = Path.Combine(clean.sysDrive, "Windows", "System32", "drivers");
 
            string[] vbDrv = { "VBoxDrv.sys", "VBoxNetAdp6.sys", "VBoxNetLwf.sys", "VBoxUSBMon.sys"};
            foreach (string drvName in vbDrv)
            {
                
                res = SCP.LaunchCommandLineApp("sc", $"stop {drvName.Replace(".sys","")}", 
                    true, false);
 
                string drvPath = Path.Combine(dirStart, drvName);

                bool b_res = FD.del_sysfile(drvPath);

            }
 
            clean.StageReporter("", "Removing VirtualBox directory");
            dirStart = AP.get_env_var("VBOX_MSI_INSTALL_PATH");
            if (dirStart == null || dirStart == "")
            {
                dirStart = Path.Combine(clean.sysDrive, "Program Files", "Oracle", "VirtualBox");
            }

            //MessageBox.Show($"Dir: {dirStart}", "Removing Oracle Dir", MessageBoxButtons.OK);
            if (Directory.Exists(dirStart))
            {
                Directory.Delete(dirStart, true);
            }

            //Clear Registry: VBoxDrv, VBoxNetAdp, VBoxUSBMon
            vb_clean_reg();
 
            //Remove Env VBOX_MSI_INSTALL_PATH
            Environment.SetEnvironmentVariable("VBOX_MSI_INSTALL_PATH", "", EnvironmentVariableTarget.Machine);
            Environment.SetEnvironmentVariable("VBOX_MSI_INSTALL_PATH", "", EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable("VBOX_MSI_INSTALL_PATH", "", EnvironmentVariableTarget.Process);
            clean.StageReporter("", "Removing shortcuts");
            //Remove shortcuts
            var shcutPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
            //    Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            //delete_Shortcut(shcutPath, appName);
            string appName = "Oracle VM VirtualBox";
            FD.delete_Shortcut(shcutPath, appName, false);
            shcutPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);

            var shcutStartPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
            //Folder files
            shcutPath = Path.Combine(shcutStartPath, "Programs");
            //Uninstall.lnk
            FD.delete_Shortcut(shcutPath, appName, false);
            //Remove folder
            FD.delete_Shortcut(shcutPath, appName, true);
            clean.StageReporter("", "Removing from %Path%");
            FD.remove_from_Path(dirStart);
            String mesg = String.Format("Oracle VirtualBox removed from Your machine. \n\n Please do not forget to RESTART windows before new installation!");
            MessageBox.Show(mesg, "Removing Oracle VirtualBox", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

        }

        public static void remove_app_vbox_short(string app_name)
        {
            DialogResult drs = MessageBox.Show($"Remove {app_name}?", $"Removing {app_name}",
                  MessageBoxButtons.YesNo,
                  MessageBoxIcon.Question,
                  MessageBoxDefaultButton.Button1);

            if (drs == DialogResult.No)
                return;
            string res = "";
            //VirtualBox Manager, VirtualBox Interface
            //Stop VMs

            //Remove all host-only interfaces
            remove_host_only();
            //Stop Processes/Services
            clean.StageReporter("", "Stopping VirtualBox processes");
            Process[] vboxProcesses = Process.GetProcesses();
            foreach (Process process in vboxProcesses)
            {
                if (process.ProcessName.Contains("VBox") || process.ProcessName.Contains("VirtualBox"))
                {
                    //MessageBox.Show($"Process: {process.ProcessName}", "Removing processes", MessageBoxButtons.OK);
                    try
                    {
                        process.Kill();
                        Thread.Sleep(2000);
                    }
                    catch (Exception e)
                    {
                        string tmp = e.Message;
                    }
                }
            }
            clean.StageReporter("", "Removing VirtualBox software");
            //HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products\6FBF3FE4D796A044AAADF7D5937BE326\InstallProperties
            string subkey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Installer\\UserData\\S-1-5-18\\Products";
            string uninstall_str = RG.find_and_get_value(subkey, "VirtualBox", "UninstallString", RegistryHive.LocalMachine);
            res = "";
            if (uninstall_str != "")
            {
                string[] uninst = uninstall_str.Split(' ');
                res = SCP.LaunchCommandLineApp(uninst[0], uninst[1], true, false);
            }
            //MessageBox.Show(res, "Uninstalling VirtualBox", MessageBoxButtons.OK);

            String mesg = "";
            if (AP.app_installed("Oracle\\VirtualBox") == 0)
            {
                mesg = String.Format("Oracle VirtualBox removed from Your machine. \n\n Please do not forget to RESTART windows before new installation!");
            }
            else
            {
                mesg = String.Format("Oracle VirtualBox was not removed from Your machine");
            }
            MessageBox.Show(mesg, "Removing Oracle VirtualBox", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

        }

        public static void remove_vm()
        {
            //label1.Text = "Virtual Machines";
            string outputVms = SCP.LaunchCommandLineApp("vboxmanage", $"list vms", true, false);
            if (outputVms.Contains("Error"))
            {
                return;
            }
            //string outputVmsRunning = LaunchCommandLineApp("vboxmanage", $"list runningvms");
            string[] rows = Regex.Split(outputVms, "\n");
            foreach (string row in rows)
            {
                if (row.Contains("subutai") || row.Contains("snappy"))
                {
                    string[] wrds = row.Split(' ');
                    foreach (string wrd in wrds)
                    {
                        if (wrd.Contains("subutai") || wrd.Contains("snappy"))
                        {
                            string vmName = wrd.Replace("\"", "");

                            DialogResult drs = MessageBox.Show($"Remove virtual machine {wrd}?", "Subutai Virtual Machines",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button1);
                            if (drs == DialogResult.Yes)
                            {
                                string res1 = SCP.LaunchCommandLineApp("vboxmanage", $"controlvm {vmName} poweroff ", true, false);
                                Thread.Sleep(5000);
                                string res2 = SCP.LaunchCommandLineApp("vboxmanage", $"unregistervm  --delete {vmName}", true, false);
                                Thread.Sleep(5000);
                            }
                        }
                    }
                }
            }
        }

        //Find if registry cleaner exists 
        //string rclean_path = logDir();
        //if (rclean_path != "")
        //{
        //    rclean_path = Path.Combine(rclean_path, "subutai-clean-registry.reg");
        //LaunchCommandLineApp("regedit.exe", $"/s {rclean_path}", false, true);
        //}

        public static void vb_clean_reg()
        {
            //Clear Registry: VBoxDrv, VBoxNetAdp, VBoxUSBMon
            //HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\VBoxDrv
            //HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\VBoxNetAdp
            //HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\VBoxUSBMon
            clean.StageReporter("", "Cleaning Registry");
            string subkey = "";
            rg_repeated("", RegistryHive.ClassesRoot);
            //HKEY_LOCAL_MACHINE\SOFTWARE\Classes\
            rg_repeated("SOFTWARE\\Classes", RegistryHive.LocalMachine);

            //HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\DIFx\DriverStore
            subkey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\DIFx\\DriverStore";
            RG.DeleteKeyByName(subkey, "VBox", RegistryHive.LocalMachine);

            //HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\Folders
            subkey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Installer\\Folders";
            RG.DeleteValueFound(subkey, "VirtualBox", RegistryHive.LocalMachine);

            //
            //"SYSTEM\\CurrentControlSet\\Services";
            RG.DeleteKeyByName(subkey, "VBox", RegistryHive.LocalMachine);
            
            //HKEY_LOCAL_MACHINE\SOFTWARE\Oracle\VirtualBox
            RG.DeleteSubKeyTree("VirtualBox", "SOFTWARE\\Oracle", RegistryHive.LocalMachine);

            //HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{ 257A247A - 9BC8 - 4506 - B4EC - F4A725976174}
            subkey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
            RG.DeleteKeyByValue(subkey, "VirtualBox", RegistryHive.LocalMachine);

            //HKEY_CLASSES_ROOT\AppID\{ 819B4D85 - 9CEE - 493C - B6FC - 64FFE759B3C9}
            subkey = "AppID";
            RG.DeleteKeyByValue(subkey, "VirtualBox", RegistryHive.ClassesRoot);

            //HKEY_CLASSES_ROOT\Installer\Products\A742A7528CB960544BCE4F7A52791647
            subkey = "Installer\\Products";
            RG.DeleteKeyByValue(subkey, "VirtualBox", RegistryHive.ClassesRoot);

            //HKEY_CURRENT_USER\Software\Oracle\VirtualBox
        }

        public static void rg_repeated(string startName, RegistryHive rh)
        {
            //HKEY_CLASSES_ROOT\.hdd
            string subkey = "";
            RG.DeleteKeyByValue(subkey, "VirtualBox", rh);

            //HKEY_CLASSES_ROOT\CLSID\{0BB3B78C-1807-4249-5BA5-EA42D66AF0BF}\InprocServer32
            subkey = startName + "CLSID";
            RG.DeleteKeyByValue(subkey, "VirtualBox", rh);
            //HKEY_CLASSES_ROOT\Wow6432Node\CLSID\
            subkey = startName + "Wow6432Node\\CLSID";
            RG.DeleteKeyByValue(subkey, "VirtualBox", rh);


            //HKEY_CLASSES_ROOT\Interface\{0169423F-46B4-CDE9-91AF-1E9D5B6CD945}
            subkey = startName + "Interface";
            RG.DeleteKeyByValue(subkey, "VirtualBox", rh);
            subkey = startName + "Wow6432Node\\Interface";
            RG.DeleteKeyByValue(subkey, "VirtualBox", rh);

            //HKEY_CLASSES_ROOT\TypeLib\{D7569351-1750-46F0-936E-BD127D5BC264}
            subkey = startName + "TypeLib";
            RG.DeleteKeyByValue(subkey, "VirtualBox", rh);
            subkey = startName + "Wow6432Node\\TypeLib";
            RG.DeleteKeyByValue(subkey, "VirtualBox", rh);

            //HKEY_CLASSES_ROOT\Local Settings\Software\Microsoft\Windows\Shell\MuiCache

            //HKEY_CLASSES_ROOT\progId_VirtualBox.Shell.hdd
            subkey = startName + "";
            RG.DeleteKeyByName(subkey, "VirtualBox", rh);
            //HKEY_CLASSES_ROOT\VirtualBox.Session
            RG.DeleteKeyByName(subkey, "VirtualBox", rh);


        }
        public static void remove_host_only()
        {
            string res = SCP.LaunchCommandLineApp("cmd.exe"," /C vboxmanage list hostonlyifs| findstr /b \"Name:\"", true, false);
            res = res.Remove(0, res.IndexOf("stdout"));
            res = res.Substring(0, res.IndexOf("stderr"));
            res = res.Replace("stdout:","");
            res = res.Replace("Name:", "");
            string[] ifaces = res.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string iface in ifaces)
            {
                string tmp = iface.Trim(' ');
                if (tmp == "" || iface == null)
                    continue;
                clean.StageReporter("", $"Removing {tmp}");
                res = SCP.LaunchCommandLineApp("vboxmanage ", $" hostonlyif remove \"{tmp}\"",true, false);
            }
        }
    }

}
