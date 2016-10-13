# Subutai Windows Installer

# How to install
Run installer w/Administrative privileges on fresh Windows 7/8/10 x64 machine </br>
Wait until installation process and shell scripts finish installation </br>
After installation finished You will see SubutaiTray login form, log in with Your Hub account, SubutaiTray icon will appera in Windows tray. </br> 
Right click on the SubutaiTray icon to open menu. Now You can open management dashbord with Launch->Launch to SS console menu.</br>


# Setup environment
You need the following tools to build the installer:
	<ul>
		<li> Visual Studio 2015 </li>
	</ul>

# Build the installer
Build Visual Studio project and copy Deployment.exe to \bin folder of Advanced Installer project
	Run the Build process from Advanced installer (you can use CLI http://www.advancedinstaller.com/user-guide/command-line.html)

# Test the installer
You can Build and Run the installer inside VM right from Advanced installer

# Overview
## Deployment tool
And we use Deployment.exe tool developed under Visual Studio to handle the second part of installation.
### Features:
<ul>
	<li> Download prerequisites from Kurjun </li>
	<li> Verify MD5 checksums </li>
	<li> Configuring resourse host </li>
	<li> Setting up P2P service </li>
</ul>

### Flags (case-sensitive):
<ul>
	<li>
		params - activities to perform during installation. Order of parameters does not matter. All parameters should be separated w/comma
		<ul>
			<li>deploy-redist - deploy redistributables (Google Chrome, Oracle VirtualBox, etc.)</li>
			<li>prepare-vbox - configure VirtualBox (create NAT network, import Snappy.ova)</li>
			<li>prepare-rh - configure resource host (install Subutai, import templates, etc.)</li>
			<li>deploy-p2p - deploy Subutai P2P service</li>
		</ul>
	<li>network-installation - can be true or false</li>
	<li>kurjunUrl - Kurjun CDN network URL</li>
	<li>repo_descriptor - file-descriptor of Kurjun CDN repository for Windows installer</li>
	<li>appDir - Subutai installation directory</li>
	<li>peer - can be "trial" or "rh-only". Identifies type of RH installation.</li>
	</li>
</ul>

### Flags examples:
<ul>
	<li> params=deploy-redist,prepare-vbox,prepare-rh,deploy-p2p </li>
	<li> network-installation=true </li>
	<li> kurjunUrl=https://kurjun.cdn.subutai.io:8338/ </li>
	<li> repo_descriptor=repomd5 </li>
	<li> appDir=C:\Subutai </li>
	<li> peer=trial </li>
</ul>

## Repository descriptor (repomd5)
	i.e. bin | tray.zip | 1 | 0 | 1 |1|1|1
	tray.zip - file to download from Kurjun
	bin - is target directory where the file will be saved. I.e. if our Subutai directory is C:\Subutai then full path will be C:\Subutai\bin\tray.zip

"| 1 | 0 | 1 |1|1|1" describes if this file will needed for given peer type and installation type, 1 means fille need to be installed, 0 - file not needed:
| Trial | RH | Tray |prod|-dev|-master
peer type can be:
	 Trial: RH + Management + SubutaiTray, recomended for start
	 RH: RH only - will be needed for multy-RH installations (1 MH and many RH), recommended for advanced users, Subutai Tray can be installed if needed
	SubutaiTray: SubutaiTray application only. Installed if You have MH or RH installed before, and if You plan to work with environments on remote hosts.

## Full content of repomd5 (all these files must persist on Kurjun):
	bin | tray.zip - tray application
	bin | p2p.exe - Subutai P2P service
	ova | snappy.ova - Ubuntu Snappy ViratualBox image
	redist | chrome.msi - Google Chrome browser (https://www.google.com/work/chrome/browser/)
	redist | tap-driver.exe - TAP driver (https://swupdate.openvpn.org/community/releases/tap-windows-9.21.1.exe)
	redist | vcredist64.exe - Visual C++ Redistributables (https://www.microsoft.com/en-us/download/details.aspx?id=48145)
	redist | virtualbox.exe - Oracle VirtualBox (http://download.virtualbox.org/virtualbox/5.0.16/VirtualBox-5.0.16-105871-Win.exe)
	redist/subutai | subutai_4.0.<VN>_amd64.snap - Subutai package for Ubuntu Snappy 	redist/subutai | subutai_4.0.<VN>_amd64-dev.snap - Subutai package for Ubuntu Snappy built from dev branch
	redist/subutai | subutai_4.0.<VN>_amd64-master.snap - Subutai package for Ubuntu Snappy built from master branch

VN - SubVersion Number



# Vagrant Box for Windows Installer
We prepared a Vagrant box for you with all required environment to build / change the installer as you wish.
<ul>
	<li>RDP: localhost:43389 (you can change this in Vagrant file)</li>
	<li>Username: user</li>
	<li>Passord: user</li>
	<li>Shared folders: please open Vagrant file and change synced folders as you wishm but the first one shares sources of Windows Installer w/the virtual machine</li>
</ul>