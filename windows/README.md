# Subutai Windows Installer
Subutai installation consists of two parts:
1. Installer, created using Inno Setup  (www.jrsoftware.org/isinfo.php), performs mimimal job: copies files, needed for further installation, on target machine, defines installation directory, creates \Software\Subutai Social\Subutai subkeys in HKEY_CURRENT_USER and HKEY_LOCAL_MACHINE hives, and runs executable.
2. Binary, created with Visual Studio 2015 project, performs the main stage of installation process:
<ul>
	<li> Change environment variabled %Path% and %Subutai%</li>
	<li> Download files needed for further installation</li>
	<li> Verify MD5 checksums </li>
	<li> Install software needed</li>
	<li> Setup virtual machine </li>
	<li> Setup virtual machine network</li>
	<li> Install/configure/start Subutai Social P2P service </li>
	<li> Create shortcuts </li>
</ul>

# How to install
Run installer (it will run with Administrative privileges) on fresh Windows 7-eng/8/10 x64 machine </br>
Choose what need to be installed (Resource Host(RH) only, Management Host(MH) - full installation, Client only).
Wait until installation process finishes install </br>
After installation finished You will see SubutaiTray login form, log in with Your Hub account, SubutaiTray icon will appear in Windows tray. </br> 
Right click on the SubutaiTray icon to open menu. Now You can open management dashbord with Launch->Launch to SS console menu.</br>


# Setup environment
You need the following tools to build the installer:
	<ul>
		<li> Visual Studio 2015 </li>
		<li> Inno Setup script compiler (www.jrsoftware.org/isinfo.php)</li>
	</ul> 
<br>
Note: Inno Setup Studio provides UI to make work with Inno Setuo scripts more comfortable: https://www.kymoto.org/


# Build the installer
Open vs-install or vs-uninstall project, modify if needed, build and put binaries into 'windows\codebase\installation_files_4_VS_Install\bin' folder. Sign binaries.

Open installation script \windows\codebase\Inno\subutai-network-installer.iss with Inno Setuo compiler. Change version in #define section: <br>
<ul>
	<li> define MyAppName "Subutai" </li>  
	<li> define MyAppVersion "4.0.x" </li>  
	<li> define MyAppPublisher "Subutai Social" </li>  
	<li> define MyAppURL "http://subutai.io/" </li>  
	<li> define MyAppExeName "Deployment.exe" </li>  
	<li> define MySRCFiles "<Path to repo>\installers\windows\codebase\installation_files" </li>   
	<li> define kurjunURL "https://cdn.subut.ai:8338" <li>  
</ul>
  
and parameters for Deployment.exe in Run section : <installation type> repo descriptor> <Run></br>
Compile script, it will be placed into Inno directory. 
</br>
Names for installers should be: subutai-network-installer.exe for production, subutai-network-installer-dev.exe for dev and subutai-network-installer-master.exe for master installations.</br>


# Overview
## Deployment Tool Parameters - in that order:
<ul>
	<li>Installation type: -dev. -master. prod </li>  
	<li>Installation description file </li> 
	<li>Run</li>  
	<li>URL for file download (kurjun URL) </li> 
</ul>


## Repository descriptor (repomd5)
	i.e. bin | tray.zip | 1 | 0 | 1 |1|1|1
	tray.zip - file to download from Kurjun
	bin - is target directory where the file will be saved. I.e. if our Subutai directory is C:\Subutai then full path will be C:\Subutai\bin\tray.zip

"| 1 | 0 | 1 |1|1|1" describes if this file will needed for given peer type and installation type, 1 means fille need to be installed, 0 - file not needed:
| RH only | MH | Client |prod|-dev|-master
peer type can be:
	
	RH: RH only - recommended for advanced users, Subutai Tray can be installed if needed
	MH: RH + Management + Client, recommended for start
	Client only: SubutaiTray + P2P + Google Chrome + E2E plugin. Installed if You plan to work with environments on remote hosts.

## Full content of repomd5 (all these files must persist on Kurjun):
	bin | tray.zip - tray application
	bin | p2p.exe - Subutai P2P service
	bin | ssh.zip - ssh shell 
	ova | snappy.ova - Ubuntu Snappy ViratualBox image
	redist | chrome.msi - Google Chrome browser (https://www.google.com/work/chrome/browser/)
	redist | tap-driver.exe - TAP driver (https://swupdate.openvpn.org/community/releases/tap-windows-9.21.1.exe)
	redist | vcredist64.exe - Visual C++ Redistributables (https://www.microsoft.com/en-us/download/details.aspx?id=48145)
	redist | virtualbox.exe - Oracle VirtualBox (http://download.virtualbox.org/virtualbox/5.0.16/VirtualBox-5.0.16-105871-Win.exe)
	redist/subutai | subutai_4.0.<VN>_amd64.snap - Subutai package for Ubuntu Snappy 	
redist/subutai | subutai_4.0.<VN + 1>_amd64-dev.snap - Subutai package for Ubuntu Snappy built from dev branch
	redist/subutai | subutai_4.0.<VN + 1>_amd64-master.snap - Subutai package for Ubuntu Snappy built from master branch

Installation Manual can be found here: https://github.com/subutai-io/installers/wiki/Windows-Installer:-Installation-Manual

# Code signing on Windows
Deployment.exe, uninstall-clean.exe and installer can be signed with command:
signtool sign /a /f  <.pfx> /p <password> /t http://timestamp.comodoca.com/?td=sha256 <.exe> 

NOTE
#What need to be done if version changed (before release)
In nothing changed except version number:

1. Change 3 last lines of repo descriptor file: subutai_4.0.<VN>_amd64.snap  - VN is Version Number like 4.0.5; subutai_4.0.<VN + 1>_amd64-dev.snap - 4.0.6 if VN = 4.0.5; subutai_4.0.<VN + 1>_amd64-master.snap - 4.0.6.
2. Open project script in Inno Sinstallation project, change version number for prod, master and dev, build project for each installation type and build installer for each installation type as described above (Build the installer section). Do not change GUID.
