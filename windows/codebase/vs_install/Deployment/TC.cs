﻿using System;
using System.IO;
using System.ComponentModel;
using NLog;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using Renci.SshNet;

namespace Deployment
{
    //Task Factory Components
    class TC
    {
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly Dictionary<string, string> _arguments = Program.form1._arguments;
        private static readonly string _cloneName = $"subutai-{DateTime.Now.ToString("yyyyMMddhhmm")}";
        private static readonly PrivateKeyFile[] _privateKeys = new PrivateKeyFile[] { };
        public static string snapFile = "";
        public static string[] rows;
        public static string installation_type = "";

        public static void download_repo()
        {
            //DOWNLOAD REPO
            Deploy.StageReporter("Downloading prerequisites", "");

            //Deploy.HideMarquee();
            logger.Info("Downloading repo_descriptor");
            download_description_file("repo_descriptor");
            if (_arguments["params"].Contains("dev"))
            {
                installation_type = "dev";
            }
            else if (_arguments["params"].Contains("master"))
            {
                installation_type = "master";
            }
            else
            {
                installation_type = "prod";
            }
            Deploy.StageReporter("", $"Creating download list for installation type: {installation_type}");

            rows = FD.repo_rows($"{_arguments["appDir"]}{_arguments["repo_descriptor"]}", _arguments["peer"], installation_type);
            string regfile = Path.Combine(FD.logDir(), "subutai-clean-registry.reg");
            Deploy.HideMarquee();
            download_file(regfile, download_prerequisites);
        }

        private static void download_description_file(String arg_name)
        {
            Deploy.StageReporter("", "Getting description file");
            Program.form1._deploy.DownloadFile(
                url: _arguments["kurjunUrl"],
                destination: $"{_arguments["appDir"]}{_arguments[arg_name]}",
                onComplete: null,
                report: "Getting repo descriptor",
                async: false,
                kurjun: true);
        }

        private static void download_file(String file_name, AsyncCompletedEventHandler cmplHandler)
        {
            Deploy.StageReporter("", $"Getting file {file_name}");
            logger.Info("Getting  file");
            Program.form1._deploy.DownloadFile(
                url: _arguments["kurjunUrl"],
                destination: file_name,
                onComplete: cmplHandler,
                report: $"Getting file {file_name}",
                async: true,
                kurjun: true);
        }

        private static int _prerequisitesDownloaded = 0;

        private static void download_prerequisites(object sender, AsyncCompletedEventArgs e)
        {
            //Check thread exception
            if (e != null)
            {
                if (e.Cancelled)
                {
                    logger.Error("File download cancelled");
                    Program.form1.finished = 2;
                    Program.form1.Visible = false;
                }

                if (e.Error != null && _prerequisitesDownloaded > 0)
                {
                    if (e.Error is WebException)
                    {
                        WebException we = (WebException)e.Error;
                        logger.Error(we.Message, "File download error");
                        Program.ShowError("File Download error, uninstalling partially installed Subutai Social",
                            we.Message);
                        Program.form1.Visible = false;
                    }
                    else
                    {
                        Exception ne = (Exception)e.Error;
                        logger.Error(ne.Message);
                        Program.ShowError("Download error, uninstalling partially installed Subutai Social",
                            ne.Message);
                        Program.form1.Visible = false;
                    }
                }
            }
            else // e == null
            {
                logger.Info("File not downloaded");
            }

            //Start download
            var row = rows[_prerequisitesDownloaded];
            var folderFile = row.Split(new[] { "|" }, StringSplitOptions.None);

            var folder = folderFile[0].Trim();
            var file = folderFile[1].Trim();

            //download tray-dev if installing -dev version
            if (file.Contains("tray") && _arguments["params"].Contains("dev"))
            {
                file = file.Replace("tray.", "tray-dev.");
            }

            if (_prerequisitesDownloaded < rows.Length - 1) //For last row will change OnComplete
            {
                _prerequisitesDownloaded++;
                Program.form1._deploy.DownloadFile(
                    url: _arguments["kurjunUrl"],
                    destination: $"{_arguments["appDir"]}{folder}/{file}",
                    onComplete: download_prerequisites,
                    report: $"Getting {file}",
                    async: true,
                    kurjun: true
                    );
            }
            else
            {//changed OnComplete
                var destfile = file;
                snapFile = file; //Last is .snap, need to remember name
                Program.form1._deploy.DownloadFile(
                   url: _arguments["kurjunUrl"],
                   destination: $"{_arguments["appDir"]}{folder}/{file}",
                   onComplete: Program.form1.TaskFactory,
                   report: $"Getting {file}",
                   async: true,
                   kurjun: true);
            }
        }


        public static void check_md5()
        {
            //verify downloaded 
            Deploy.StageReporter(" ", " ");
            Deploy.StageReporter("Verifying MD5", "");
            Deploy.HideMarquee();
            foreach (var info in Program.form1.PrerequisiteFilesInfo)
            {
                var filepath = info.Key;
                var filepath_local = filepath; 
                if (filepath.Contains("tray-dev"))
                    filepath_local = filepath.Replace("-dev", "");

                var filename = Path.GetFileName(filepath);
                Deploy.StageReporter("", $"Checking {filepath_local}");
                var kurjunFileInfo = info.Value;
                var calculatedMd5 = Deploy.Calc_md5(filepath_local, upperCase: false);
                if (calculatedMd5 == "-1")
                {
                    logger.Info("File {0} does not exist: {1}", filepath_local, calculatedMd5);
                    continue;
                }
                logger.Info("Checking orig: {0} {1},  local: {2} {3}", filepath, kurjunFileInfo.id, filepath_local, calculatedMd5);

                if (calculatedMd5 != kurjunFileInfo.id.Replace("raw.", ""))
                {
                    logger.Error("Verification of MD5 checksums for {0} failed: calc = {1}", filepath, calculatedMd5);
                }
            }
            Deploy.StageReporter(" ", " ");
        }
        public static void unzip_extracted()
        {
            // UNZIP FILES
            Deploy.StageReporter(" ", " ");
            Deploy.StageReporter("Extracting", "");
            logger.Info("Unzipping");
            Deploy.HideMarquee();
            Program.form1._deploy.unzip_files(_arguments["appDir"]);
            Deploy.StageReporter(" ", " ");
        }

        public static void deploy_redist()
        {
            // DEPLOY REDISTRIBUTABLES
            Deploy.StageReporter(" ", " ");
            Deploy.StageReporter("Installing redistributables", "");
            logger.Info("Installing redistributables");
            Deploy.ShowMarquee();
            string appDir = _arguments["appDir"];

            if (_arguments["peer"] != "rh-only") //install components if not installing RH only
            {
                string res = "";

                Deploy.StageReporter("", "TAP driver");
                Inst.inst_TAP(appDir);

                Deploy.StageReporter("", "MS Visual C++");
                res = Deploy.LaunchCommandLineApp($"{_arguments["appDir"]}redist\\vcredist64.exe", "/install /quiet");
                logger.Info("MS Visual C++: {0}", res);

                Deploy.StageReporter("", "Chrome browser");
                Inst.inst_Chrome(appDir);

                Deploy.StageReporter("", "Checking Chrome E2E extension");
                Inst.inst_E2E();

                Deploy.StageReporter("", "SSH ");
                Inst.inst_ssh(Path.Combine(FD.sysDrive(), "Subutai"));
            }

            Deploy.StageReporter("", "Virtual Box");//installing VBox if not Client-only installation
            if (_arguments["peer"] != "client-only")
            {
                Inst.inst_VBox(appDir);
            }
            Deploy.StageReporter(" ", " ");
        }

        public static void prepare_vbox()
        {
            // PREPARE VBOX
            Deploy.StageReporter(" ", " ");
            Deploy.StageReporter("Preparing Virtual Box", "");
            Deploy.StageReporter("", "Configuring network");
            logger.Info("Preparing Virtual Box");
            Deploy.ShowMarquee();
            // prepare NAT network
            string res = "";
            res = Deploy.LaunchCommandLineApp("vboxmanage", "natnetwork remove --netname natnet1 ");
            logger.Info("Removing NAT network: {0}", res);
            res = Deploy.LaunchCommandLineApp("vboxmanage", "natnetwork add --netname natnet1 --network '10.0.5.0/24' --enable --dhcp on");
            logger.Info("Configuring NAT network: {0}", res);
            //if (Deploy.com_out(res,2) == "Error")
            if (res.ToLower().Contains("error"))
            {
                logger.Error("Can not run command, please check if VirtualBox installed properly", "Configure VM");
                Program.ShowError("Can not configure VM, please check if VitrualBox installed properly", "Prepare VBox");
                Program.form1.Visible = false;
            }
            // import OVAs
            Deploy.StageReporter("", "Importing Snappy");
            res = Deploy.LaunchCommandLineApp("vboxmanage", $"import {_arguments["appDir"]}ova\\snappy.ova");
            logger.Info("Importing snappy: {0}", Deploy.com_out(res, 0));
            if (res.ToLower().Contains("error"))
            {
                logger.Error("Can not run command, please check if VirtualBox installed properly", "Importing Snappy");
                Program.ShowError("Can not Import Snappy, please check if VitrualBox installed properly", "Prepare VBox");
                Program.form1.Visible = false;
            }
            Deploy.StageReporter(" ", " ");
        }

        public static void prepare_rh()
        {
            // PREPARE RH
            Deploy.StageReporter(" ", " ");
            Deploy.StageReporter("Preparing resource host", "");
            logger.Info("Preparing resource host");
            string res = "";
            Deploy.ShowMarquee();

            // clone VM
            Deploy.StageReporter("", "Cloning VM");
            VMs.clone_vm(_cloneName);

            //Preparing temporary network interfaces to upload and run installation scripts
            Deploy.StageReporter("", "Preparing NIC - NAT");
            logger.Info("Preparing NIC-NAT");
            res = Deploy.LaunchCommandLineApp("vboxmanage",
                $"modifyvm {_cloneName} --nic1 nat --cableconnected1 on --natpf1 'ssh-fwd,tcp,,4567,,22' --natpf1 'mgt-fwd,tcp,,9999,,8443'");
            logger.Info("nic 1 --nat: {0}", res);
            //if (res.ToLower().Contains("error"))
            //{
            //    logger.Error("Can not run command, please check if VirtualBox installed properly", "Importing Snappy");
            //    Program.ShowError("Can not modify VM, please check if VitrualBox installed properly", "Prepare VBox");
            //    Program.form1.Visible = false;
            //}
            res = Deploy.LaunchCommandLineApp("vboxmanage", $"modifyvm {_cloneName} --nic4 none");
            if (res.ToLower().Contains("error"))
            {
                logger.Error("Can not run command, please check if VirtualBox installed properly", "Importing Snappy");
                Program.ShowError("Can not modify VM, please check if VitrualBox installed properly", "Prepare VBox");
                Program.form1.Visible = false;
            }
            // set RAM
            Deploy.StageReporter("", "Setting RAM");
            VMs.vm_set_RAM(_cloneName);

            //set number of cores
            Deploy.StageReporter("", "Setting number of processors");
            VMs.vm_set_CPUs(_cloneName);

            //set UTC timezone
            Deploy.StageReporter("", "Setting timezone");
            VMs.vm_set_timezone(_cloneName);

            //start VM, will try 2 times
            Deploy.StageReporter("", "Starting VM");
            if (!VMs.start_vm(_cloneName))
            {
                VMs.stop_vm(_cloneName);
                Thread.Sleep(10000);
                if (!VMs.start_vm(_cloneName))
                {
                    logger.Error("Can not start VM, please try to start manualy", "Waiting for SSH");
                    Program.ShowError("Can not start VM, please try to start manualy", "Waiting for SSH");
                    Program.form1.Visible = false;
                }
            }

            Deploy.StageReporter("Setting up peer", "");
            logger.Info("Setting up peer");
            // waiting SSH session
            Deploy.StageReporter("", "Waiting for SSH ");
            logger.Info("Waiting for SSH 1");
            bool res_b = VMs.waiting_4ssh(_cloneName);
            if (!res_b)
            {
                logger.Error("SSH 1 false", "Can not open ssh, please check VM state manually and report error");
                Program.ShowError("Can not open ssh, please check VM state manually and report error", "Waiting for SSH");
                Program.form1.Visible = false;
            }
            // DEPLOY PEER
            VMs.run_scripts(_arguments["appDir"], _cloneName);

            Deploy.StageReporter("", "Setting peer options");
            logger.Info("Setting peer options");

            if (_arguments["peer"] == "trial")
            {
                Deploy.StageReporter("Preparing management host", "");
                logger.Info("Preparing management host");
                logger.Info("trial - installing management host");
                if (_arguments["network-installation"].ToLower() != "true")
                {
                    // setting iptables rules
                    Deploy.StageReporter("", "Restricting SSH only");
                    Deploy.SendSshCommand("127.0.0.1", 4567, "ubuntu", _privateKeys,
                        "sudo iptables -P INPUT DROP; sudo iptables -P OUTPUT DROP; sudo iptables -A INPUT -p tcp -m tcp --dport 22 -j ACCEPT; sudo iptables -A OUTPUT -p tcp --sport 22 -m state --state ESTABLISHED, RELATED -j ACCEPT");
                }

                if (_arguments["network-installation"].ToLower() == "true")
                {
                    Inst.install_mh_nw();
                }
                else
                {
                    Inst.install_mh_lc(_privateKeys);
                }

                if (_arguments["network-installation"].ToLower() != "true")
                {
                    // setting iptables rules
                    Deploy.StageReporter("", "Allowing TCP trafic");
                    logger.Info("Allowing TCP trafic");
                    Deploy.SendSshCommand("127.0.0.1", 4567, "ubuntu", "ubuntu",
                        "sudo iptables - P INPUT ACCEPT; sudo iptables -P OUTPUT ACCEPT");
                }

            }
            if (_arguments["peer"] == "rh-only")
                Program.form1.finished = 1;
            Deploy.SendSshCommand("127.0.0.1", 4567, "ubuntu", "ubuntu", "sudo sync;sync");
            Deploy.StageReporter(" ", " ");
        }

        public static void deploy_p2p()
        {
            // DEPLOYING P2P SERVICE
            //_arguments["appDir"] = "C:\\Subutai\\"; for debug
            Deploy.StageReporter(" ", " ");
            string appPath = _arguments["appDir"];
            Deploy.StageReporter("Installing P2P service", "");
            Deploy.ShowMarquee();
            
            string name = "Subutai Social P2P";
            var binPath = $"{appPath}bin\\p2p.exe";
            const string binArgument = "daemon";
            //Check if service is running and remove if yes
            Inst.service_stop(name);

            // installing service
            Deploy.StageReporter("", "Installing P2P service");
            Inst.service_install(name, binPath, binArgument);
            //Inst.service_install(name, "C:\\Subutai\\bin\\p2p.exe", binArgument);
            Deploy.StageReporter("", "Adding P2P service to firewall exceptions");
            Net.set_fw_rules(name, "p2p_s", true);

            Net.set_fw_rules(binPath, "p2p", false);
            Net.set_fw_rules($"{appPath}bin\\tray\\SubutaiTray.exe", "SubutaiTray", false);

            //Configuring service logs
            Deploy.StageReporter("", "Configuring P2P service logs");
            Inst.p2p_logs_config("Subutai Social P2P");

            //Starting P2P service 
            Deploy.StageReporter("", "Starting P2P service");
            Inst.service_start(name);

            //configuring service restart on failure
            Deploy.StageReporter("", "Configuring P2P service");
            Inst.service_config(name);

            if (_arguments["peer"] != "rh-only")
                Program.form1.finished = 1;
        }
    }
}
