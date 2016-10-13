﻿using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;


namespace uninstall_clean
{
    class AP
    {
        public static void del_TAP()
        {
            string binPath = Path.Combine(clean.sysDrive, "Program Files", "TAP-Windows", "bin", "tapinstall.exe");
            string res = "";
            if (File.Exists(binPath))
            {
                res = SCP.LaunchCommandLineApp(binPath, "remove tap0901", true, false);
            }
            binPath = Path.Combine(clean.sysDrive, "Program Files", "TAP-Windows", "Uninstall.exe");
            string pathPath = Path.Combine(clean.sysDrive, "Program Files", "TAP-Windows", "bin");
            remove_app("TAP-Windows", binPath, "/S", "TAP-Windows");
        }

        public static string get_env_var(string var_name)
        {
            var EnvVar = Environment.GetEnvironmentVariable(var_name) ?? "";

            if (EnvVar == "")
            {
                EnvVar = Environment.GetEnvironmentVariable(var_name, EnvironmentVariableTarget.Machine) ?? "";
                if (EnvVar == "")
                {
                    EnvVar = Environment.GetEnvironmentVariable(var_name, EnvironmentVariableTarget.Process) ?? "";
                }
            }
            return EnvVar;
        }
        public static int app_installed(string appName)
        {
            string subkey = Path.Combine("SOFTWARE\\Wow6432Node", appName);
            string subkey86 = Path.Combine("SOFTWARE\\", appName);
            RegistryKey rk = Registry.LocalMachine.OpenSubKey(subkey);
            RegistryKey rk86 = Registry.LocalMachine.OpenSubKey(subkey86);
            if (rk == null && rk86 == null)
            {
                return 0;
            }
            return 1;
        }

        public static  void remove_app(string app_name, string cmd, string args, string app_path)
        {
            DialogResult drs = MessageBox.Show($"Remove {app_name}?", $"Removing {app_name}",
                               MessageBoxButtons.YesNo,
                               MessageBoxIcon.Question,
                               MessageBoxDefaultButton.Button1);

            if (drs == DialogResult.No)
                return;

            string mess = "";
            if (File.Exists(cmd))
            {
                string res = SCP.LaunchCommandLineApp(cmd, args, false, false);
                if (res.Contains("|Error"))
                {
                    mess = $"{app_name} was not removed, please uninstall manually";
                }
                else
                {
                    mess = $"{app_name} uninstalled";
                }
            }
            else
            {
                mess = $"Probably {app_name} was not installed, please check and uninstall manually";
            }
            //MessageBox.Show(mess, $"Uninstalling {app_name}", MessageBoxButtons.OK);
            if (app_path != "" || app_path != null)
            {
                FD.remove_from_Path(app_path);
            }
        }

        private void clean_all()
        {
            string SubutaiDir = clean.SubutaiDir;
            SCP.remove_fw_rules(SubutaiDir);
            string mess = SCP.stop_process("p2p");
            mess = "";
            mess = SCP.stop_service("Subutai Social P2P", 5000);
            mess = "";
            mess = SCP.remove_service("Subutai Social P2P");
            mess = "";
            mess = SCP.stop_process("SubutaiTray");
            mess = "";
            FD.delete_Shortcuts("Subutai");

            if (clean.SubutaiDir != "" && SubutaiDir != null && SubutaiDir != "C:\\" && SubutaiDir != "D:\\" && SubutaiDir != "E:\\")
            {
                DialogResult drs = MessageBox.Show($"Remove folder {SubutaiDir}? (Do not remove if going to install again)", "Subutai Virtual Machines",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button1);
                mess = "";
                if (drs == DialogResult.Yes)
                {
                    mess = FD.delete_dir(SubutaiDir);
                }

                if (mess.Contains("Can not"))
                {
                    MessageBox.Show($"Folder {SubutaiDir} can not be removed. Please delete it manually",
                        "Removing Subutai folder", MessageBoxButtons.OK);
                }
            }
            //Remove Subutai dir from ApplicationData
            string appUserDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            //MessageBox.Show($"AppData: {appUserDir}", "AppData", MessageBoxButtons.OK);
            appUserDir = Path.Combine(appUserDir, "Subutai Social");
            //MessageBox.Show($"Subutai Social: {appUserDir}", "Subutai Social", MessageBoxButtons.OK);
            mess = FD.delete_dir(appUserDir);
            //Remove /home shortcut
            mess = FD.remove_home(SubutaiDir);
            //Remove Subutai dirs from Path
            mess = FD.remove_from_Path("Subutai");
            //Save Path
            Environment.SetEnvironmentVariable("Subutai", "", EnvironmentVariableTarget.Machine);
            Environment.SetEnvironmentVariable("Subutai", "", EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable("Subutai", "", EnvironmentVariableTarget.Process);
            //Clean registry
            RG.delete_from_reg();
            
            //Remove TAP interfaces and uninstall TAP
            del_TAP();
            //Remove snappy and subutai machines
            VBx.remove_vm();
            //Remove Oracle VirtualBox
            //            remove_app_vbox("Oracle VirtualBox");
            //Remove log dir
            FD.remove_log_dir();
            MessageBox.Show("Subutai Social uninstalled", "Information", MessageBoxButtons.OK);
            Environment.Exit(0);
        }


    }
}
