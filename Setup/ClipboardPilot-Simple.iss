; Clipboard Pilot Setup - Ultra Simple Version
; No dependencies, just works!

[Setup]
AppName=Clipboard Pilot
AppVersion=1.0.0
AppPublisher=Clipboard Pilot Team
AppPublisherURL=https://github.com/BaeTab/ClipboardPilot
DefaultDirName={autopf}\ClipboardPilot
DefaultGroupName=Clipboard Pilot
OutputDir=..\Output
OutputBaseFilename=ClipboardPilot-Setup-v1.0.0
Compression=lzma2
SolidCompression=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
WizardStyle=modern
DisableProgramGroupPage=yes
DisableWelcomePage=no

[Files]
; 실제 publish 경로에서 파일 가져오기
Source: "..\ClipboardPilot\bin\Release\net8.0-windows\win-x64\publish\ClipboardPilot.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\Clipboard Pilot"; Filename: "{app}\ClipboardPilot.exe"
Name: "{autodesktop}\Clipboard Pilot"; Filename: "{app}\ClipboardPilot.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop icon"; GroupDescription: "Additional icons:"

[Run]
Filename: "{app}\ClipboardPilot.exe"; Description: "Launch Clipboard Pilot"; Flags: postinstall skipifsilent nowait

[Code]
function InitializeSetup(): Boolean;
begin
  Result := True;
end;
