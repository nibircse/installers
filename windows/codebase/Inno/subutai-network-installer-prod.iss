; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "Subutai"
#define MyAppVersion "4.0.13"
#define MyAppType "prod"
#define MyAppPublisher "Subutai Social"
#define MyAppURL "http://subutai.io/"
#define MyAppExeName "Deployment.exe"
#define MySRCFiles "E:\Projects\Subutai_Installer_4Git\installers\windows\codebase\installation_files_4_VS_Install"
#define MyInnDir "E:\Projects\Subutai_Installer_4Git\installers\windows\codebase\Inno"

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
ArchitecturesAllowed=x64
DefaultDirName=C:\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
DisableWelcomePage=no
OutputDir={#MyInnDir}
OutputBaseFilename=subutai-network-installer
SetupIconFile={#MySRCFiles}\Subutai_logo_4_Light_70x70.ico
Compression=lzma
SolidCompression=yes
PrivilegesRequired=admin
UsePreviousSetupType=False
UsePreviousTasks=False
MinVersion=0,6.1
WizardImageFile={#MyInnDir}\pictures\Wizard_4.bmp
WizardSmallImageFile={#MyInnDir}\pictures\Subutai_logo_Light.bmp

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "{#MySRCFiles}\bin\Deployment.exe"; DestDir: "{app}\bin"; Flags: replacesameversion
Source: "{#MySRCFiles}\bin\*"; DestDir: "{app}\bin"; Flags: replacesameversion recursesubdirs createallsubdirs
Source: "{#MySRCFiles}\redist\*"; DestDir: "{app}\redist"; Flags: replacesameversion recursesubdirs createallsubdirs
Source: "{#MySRCFiles}\Subutai_logo_4_Light_70x70.ico"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#MySRCFiles}\uninstall.ico"; DestDir: "{app}"; Flags: replacesameversion
Source: "{#MySRCFiles}\redist\Framework\dotNetFx45_Full_setup.exe"; DestDir: "{tmp}"; Flags: 64bit deleteafterinstall; Check: FrameworkIsNotInstalled; AfterInstall: InstallFramework

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

[Code]
procedure InstallFramework_;
var
  ResultCode: Integer;
begin
  if not Exec(ExpandConstant('{tmp}\dotNetFx45_Full_setup.exe'), '/q /norestart', '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then
  begin
    // you can interact with the user that the installation failed
    MsgBox('.NET installation failed with code: ' + IntToStr(ResultCode) + '.',
      mbError, MB_OK);
  end;
end;

procedure InstallFramework;
var
  StatusText: string;
  ResultCode: Integer;
  begin
  StatusText := WizardForm.StatusLabel.Caption;
  WizardForm.StatusLabel.Caption := 'Installing .NET framework...';
  WizardForm.ProgressGauge.Style := npbstMarquee;
  
  try
    Exec(ExpandConstant('{tmp}\dotNetFx45_Full_setup.exe'), '/q /norestart', '', SW_SHOW, ewWaitUntilTerminated, ResultCode);
  except
    MsgBox('.NET installation failed with code: ' + IntToStr(ResultCode) + '.',
      mbError, MB_OK);
  finally
    WizardForm.StatusLabel.Caption := StatusText;
    WizardForm.ProgressGauge.Style := npbstNormal;
  end;
end;

function FrameworkIsNotInstalled: Boolean;
begin
  Result := not RegKeyExists(HKEY_LOCAL_MACHINE, 'Software\Microsoft\.NETFramework\policy\v4.0');   
end;


function GetUninstallString: string;
var
  sUnInstPath: string;
  sUnInstallString: String;
begin
  Result := '';
  sUnInstPath := ExpandConstant('Software\Microsoft\Windows\CurrentVersion\Uninstall\{{D8AEAA94-0C20-4F7E-A106-4E9617A3D7B9}_is1'); 
  sUnInstallString := '';
  if not RegQueryStringValue(HKLM, sUnInstPath, 'UninstallString', sUnInstallString) then
    RegQueryStringValue(HKCU, sUnInstPath, 'UninstallString', sUnInstallString);
  Result := sUnInstallString;
end;

function IsUpgrade: Boolean;
begin
  Result := (GetUninstallString() <> '');
end;

function InitializeSetup: Boolean;
var
  V: Integer;
  iResultCode: Integer;
  sUnInstallString: string;
begin
  Result := True; // in case when no previous version is found
  if RegValueExists(HKEY_LOCAL_MACHINE,'Software\Microsoft\Windows\CurrentVersion\Uninstall\{D8AEAA94-0C20-4F7E-A106-4E9617A3D7B9}_is1', 'UninstallString') then  
  begin
    V := MsgBox(ExpandConstant('Subutai is already installed. It must be uninstalled before new installation. Do you want to uninstall it now?'), mbInformation, MB_YESNO); //Custom Message if App installed
    if V = IDYES then
    begin
      sUnInstallString := GetUninstallString();
      sUnInstallString :=  RemoveQuotes(sUnInstallString);
      Exec(ExpandConstant(sUnInstallString), '', '', SW_SHOW, ewWaitUntilTerminated, iResultCode);
      Result := True; //if you want to proceed after uninstall
      //Exit; //if you want to quit after uninstall
    end
    else
      Result := False; //when older version present and not uninstalled
      Exit;
  end;
end;
[Run]
Filename: "{app}\bin\{#MyAppExeName}"; Parameters: "{#MyAppType} repomd5 Run"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent  runascurrentuser

[UninstallRun]
Filename: "{app}\bin\uninstall-clean.exe"; Flags: nowait 

[Messages]
InstallingLabel=Please wait while first stage of [name] Setup  completed.
FinishedHeadingLabel=Completing the [name] First Stage Setup
FinishedLabel=Setup has finished the first stage of  [name] installation. Please do not uncheck Launch combo box. Close Setup and wait for Second Stage Window
SetupLdrStartupMessage=This is the first stage of  %1 Installation. Do you wish to continue?
ButtonFinish=&Next