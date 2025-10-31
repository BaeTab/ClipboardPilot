; Clipboard Pilot Setup - Minimal Version
; Bare minimum, fastest install

[Setup]
AppName=Clipboard Pilot
AppVersion=1.0.0
AppPublisher=Clipboard Pilot Team
DefaultDirName={autopf}\ClipboardPilot
DisableProgramGroupPage=yes
DisableWelcomePage=yes
OutputDir=..\Output
OutputBaseFilename=ClipboardPilot-Setup-Minimal-v1.0.0
Compression=lzma2
SolidCompression=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
WizardStyle=modern

[Files]
; Main executable from publish folder
Source: "..\ClipboardPilot\bin\Release\net8.0-windows\win-x64\publish\ClipboardPilot.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\Clipboard Pilot"; Filename: "{app}\ClipboardPilot.exe"

[Run]
Filename: "{app}\ClipboardPilot.exe"; Flags: postinstall nowait

[Code]
function InitializeSetup(): Boolean;
begin
  Result := True;
end;
