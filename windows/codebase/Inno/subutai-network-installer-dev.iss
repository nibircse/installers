; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "Subutai"
#define MyAppVersion "4.0.5"
#define MyAppPublisher "Subutai Social"
#define MyAppURL "http://subutai.io/"
#define MyAppExeName "Deployment.exe"
#define MySRCFiles "E:\Projects\Subutai_Installer_4Git\installers\windows\codebase\installation_files_4_VS_Install"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
;AppId={{D8AEAA94-0C20-4F7E-A106-4E9617A3D7B9}
AppId={{D8AEAA94-0C20-4F7E-A106-4E9617A3D7B9}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName=C:\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=E:\Projects\Subutai_Installer_4Git\installers\windows\codebase\Inno
OutputBaseFilename=subutai-network-installer-dev
SetupIconFile={#MySRCFiles}\Subutai_logo_4_Light_70x70.ico
Compression=lzma
SolidCompression=yes
PrivilegesRequired=admin

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"


[Files]

Source: "{#MySRCFiles}\bin\Deployment.exe"; DestDir: "{app}\bin"; Flags: replacesameversion
Source: "{#MySRCFiles}\bin\*"; DestDir: "{app}\bin"; Flags: replacesameversion recursesubdirs createallsubdirs
Source: "{#MySRCFiles}\redist\*"; DestDir: "{app}\redist"; Flags: replacesameversion recursesubdirs createallsubdirs
Source: "{#MySRCFiles}\Subutai_logo_4_Light_70x70.ico"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#MySRCFiles}\uninstall.ico"; DestDir: "{app}"; Flags: replacesameversion

;Source: "E:\Projects\Subutai_Installer\Inno\bin\Deployment.exe"; DestDir: "{app}\bin"; Flags: replacesameversion
;Source: "E:\Projects\Subutai_Installer\Inno\bin\*"; DestDir: "{app}\bin"; Flags: replacesameversion recursesubdirs createallsubdirs
;Source: "E:\Projects\Subutai_Installer\Inno\redist\*"; DestDir: "{app}\redist"; Flags: replacesameversion recursesubdirs createallsubdirs
;Source: "E:\Projects\Subutai_Installer\Inno\Subutai_logo_4_Light_70x70.ico"; DestDir: "{app}"; Flags: replacesameversion
;Source: "E:\Projects\Subutai_Installer\Inno\uninstall.ico"; DestDir: "{app}"; Flags: replacesameversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[Registry]
; Start "Software\My Company\My Program" keys under HKEY_CURRENT_USER
; and HKEY_LOCAL_MACHINE. The flags tell it to always delete the
; "My Program" keys upon uninstall, and delete the "My Company" keys
; if there is nothing left in them.
Root: HKCU; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}\"; Flags: uninsdeletekeyifempty
Root: HKCU; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}\"; ValueType: string; ValueName: "Path"; ValueData: "{app}\"
Root: HKCU; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}\"; ValueType: string; ValueName: "Version"; ValueData: "{#MyAppVersion}"
Root: HKLM; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}\"; Flags: uninsdeletekeyifempty
Root: HKLM; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}\"; ValueType: string; ValueName: "Path"; ValueData: "{app}\"
Root: HKLM; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}\"; ValueType: string; ValueName: "Version"; ValueData: "{#MyAppVersion}"


[Run]
Filename: "{app}\bin\{#MyAppExeName}"; Parameters: "dev repomd5 Run"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent  runascurrentuser

[UninstallRun]
Filename: "{app}\bin\uninstall-clean.exe"; Flags: nowait 
