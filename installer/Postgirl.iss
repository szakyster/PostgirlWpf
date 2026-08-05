#define ExeName "Postgirl.exe"

#ifndef AppName
  #define AppName "Postgirl"
#endif

#ifndef AppVersion
  #define AppVersion "0.0.0"
#endif

#ifndef AppFullVersion
  #define AppFullVersion AppVersion
#endif

#ifndef AppPublisher
  #define AppPublisher "KHE Tools"
#endif

#ifndef OutputBaseFileName
  #define OutputBaseFileName "Postgirl-Setup"
#endif

#ifndef SourceDir
  #error SourceDir define is required.
#endif

#ifndef OutputDir
  #define OutputDir "."
#endif

[Setup]
AppId={{7F53B57A-4F80-4B8D-9A92-A5C840B0219B}
AppName={#AppName}
AppVersion={#AppFullVersion}
AppVerName={#AppName} {#AppFullVersion}
AppPublisher={#AppPublisher}
AppPublisherURL=https://github.com/szakyster/PostgirlWpf
DefaultDirName={localappdata}\Programs\{#AppName}
DefaultGroupName={#AppName}
UninstallDisplayIcon={app}\{#ExeName}
OutputDir={#OutputDir}
OutputBaseFilename={#OutputBaseFileName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
SetupIconFile=..\Assets\postgirl.ico

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional icons:"; Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#ExeName}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#ExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#ExeName}"; Description: "Launch {#AppName}"; Flags: nowait postinstall skipifsilent
