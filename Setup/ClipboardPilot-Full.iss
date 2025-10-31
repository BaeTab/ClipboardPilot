; Clipboard Pilot - Complete Setup Script
; Inno Setup 6.x required

#define MyAppName "Clipboard Pilot"
#define MyAppKoreanName "클립보드 파일럿"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Clipboard Pilot Team"
#define MyAppURL "https://github.com/clipboardpilot"
#define MyAppExeName "ClipboardPilot.exe"

[Setup]
; App Identity
AppId={{B5F3E9A1-8D2C-4F1B-9E3A-7C6D5B4A3E2F}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}

; Installation Paths
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
DisableDirPage=no

; Privileges
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

; Output
OutputDir=..\Output
OutputBaseFilename=ClipboardPilot-Setup-v{#MyAppVersion}

; Compression
Compression=lzma2/max
SolidCompression=yes

; UI
WizardStyle=modern
SetupIconFile=compiler:SetupModernIcon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}

; Architecture
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

; Misc
AllowNoIcons=yes
DisableWelcomePage=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "korean"; MessagesFile: "compiler:Languages\Korean.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "startupicon"; Description: "Run at Windows startup"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Main executable
Source: "..\Output\ClipboardPilot\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

; Documentation (optional)
Source: "..\LICENSE.txt"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "..\README.md"; DestDir: "{app}"; Flags: ignoreversion isreadme skipifsourcedoesntexist

; Config template (optional)
Source: "..\Output\ClipboardPilot\clipboard-pilot.settings.json"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist

[Icons]
; Start Menu
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

; Desktop
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Registry]
; Startup registry key
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "{#MyAppName}"; ValueData: """{app}\{#MyAppExeName}"""; Flags: uninsdeletevalue; Tasks: startupicon

; App settings
Root: HKCU; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}"; Flags: uninsdeletekeyifempty
Root: HKCU; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"
Root: HKCU; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}"; ValueType: string; ValueName: "Version"; ValueData: "{#MyAppVersion}"

[Code]
function InitializeSetup(): Boolean;
var
  OldVersion: String;
  UninstallKey: String;
  UninstallString: String;
  ErrorCode: Integer;
begin
  Result := True;
  UninstallKey := 'Software\Microsoft\Windows\CurrentVersion\Uninstall\{#SetupSetting("AppId")}_is1';
  
  if RegQueryStringValue(HKCU, UninstallKey, 'DisplayVersion', OldVersion) then
  begin
    if MsgBox('Previous version (' + OldVersion + ') is installed.' + #13#10 + 
              'Would you like to uninstall it first?', mbConfirmation, MB_YESNO) = IDYES then
    begin
      if RegQueryStringValue(HKCU, UninstallKey, 'UninstallString', UninstallString) then
      begin
        Exec(RemoveQuotes(UninstallString), '/SILENT', '', SW_SHOW, ewWaitUntilTerminated, ErrorCode);
      end;
    end
    else
      Result := False;
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  DataPath: String;
begin
  if CurUninstallStep = usUninstall then
  begin
    DataPath := ExpandConstant('{userappdata}\ClipboardPilot');
    
    if DirExists(DataPath) then
    begin
      if MsgBox('Do you want to delete user data (clipboard history, settings, etc.)?' + #13#10 + #13#10 + 
                'Path: ' + DataPath, mbConfirmation, MB_YESNO) = IDYES then
      begin
        DelTree(DataPath, True, True, True);
      end;
    end;
  end;
end;
