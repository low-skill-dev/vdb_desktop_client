#define MyAppName "Vdb VPN"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Vdb"
#define MyAppURL "https://vdb.bruhcontent.ru/"
#define MyAppExeName "UserInterface.exe"
#define DotNetFileName "windowsdesktop-runtime-8.0.0-preview.3.23178.1-win-x64.exe";
#define WgFileName "wireguard-amd64-0.5.3.msi";

[Setup]
AppId={{DA1F8E9D-521C-43BC-B6BD-9896B1D68951}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
LicenseFile=LICENSE.txt
OutputBaseFilename=VdbInstaller
SetupIconFile=icons\favicon256.ico
Compression=none
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}";

[Files]
Source: "..\UserInterface\bin\Release\net8.0-windows\publish\win-x64\{#MyAppExeName}";  DestDir: "{app}"
Source: ".\additional_software\{#DotNetFileName}"; DestDir: "{tmp}"; Flags: deleteafterinstall; AfterInstall: InstallDotnet();
Source: ".\additional_software\{#WgFileName}"; DestDir: "{tmp}"; Flags: deleteafterinstall; AfterInstall: InstallWg();

[Code]
procedure InstallDotnet;
  var
    resultCode: integer;
  begin
    WizardForm.StatusLabel.Caption := 'Installing dotNET...';
    WizardForm.ProgressGauge.Style := npbstMarquee;
    Exec(ExpandConstant('{tmp}\{#DotNetFileName}'), '/quiet /norestart', '', SW_HIDE, ewWaitUntilTerminated, resultCode);
  end;
procedure InstallWg;
  var
    resultCode: integer;
  begin
    WizardForm.StatusLabel.Caption := 'Installing WireGuard...';
    WizardForm.ProgressGauge.Style := npbstMarquee;
    Exec('cmd.exe', ExpandConstant('/q /c MsiExec.exe /i {tmp}\{#WgFileName} DO_NOT_LAUNCH=1 /qn'), '', SW_HIDE, ewWaitUntilTerminated, resultCode);
  end;

//[Run]
//Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: shellexec runasoriginaluser postinstall waituntilterminated skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{userdocs}\Vdb\refresh.token"
Type: filesandordirs; Name: "{userdocs}\Vdb\vdb0.conf"
Type: filesandordirs; Name: "{userdocs}\Vdb\vdb0.key"
Type: dirifempty; Name: "{userdocs}\Vdb"; 


