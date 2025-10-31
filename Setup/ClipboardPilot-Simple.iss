; Clipboard Pilot Setup - Ultra Simple Version
; No dependencies, just works!

[Setup]
AppName=Clipboard Pilot
AppVersion=1.0.0
AppPublisher=Clipboard Pilot Team
DefaultDirName={autopf}\ClipboardPilot
DefaultGroupName=Clipboard Pilot
OutputDir=..\Output
OutputBaseFilename=ClipboardPilot-Setup
Compression=lzma2
SolidCompression=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

[Files]
Source: "..\Output\ClipboardPilot\ClipboardPilot.exe"; DestDir: "{app}"

[Icons]
Name: "{autoprograms}\Clipboard Pilot"; Filename: "{app}\ClipboardPilot.exe"
Name: "{autodesktop}\Clipboard Pilot"; Filename: "{app}\ClipboardPilot.exe"

[Run]
Filename: "{app}\ClipboardPilot.exe"; Description: "Launch Clipboard Pilot"; Flags: postinstall skipifsilent nowait
