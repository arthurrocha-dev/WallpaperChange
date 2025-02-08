#define MyAppVersion "1.0.1"
#define MyAppPublisher "arthurrocha.dev"
#define MyAppWebsite "https://arthurrocha.dev" 
[Setup]
AppName=WallpaperChanger
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppWebsite}
DefaultDirName={localappdata}\WallpaperChanger
DefaultGroupName=WallpaperChanger
UninstallDisplayIcon={app}\WallpaperChanger.exe
OutputDir=.
OutputBaseFilename=WallpaperChanger_Installer_{#MyAppVersion}
Compression=lzma
SolidCompression=yes
SetupIconFile=icone.ico

[Files]
Source: "bin\Release\net8.0\win-x64\*"; DestDir: "{app}"
Source: "start.vbs"; DestDir: "{app}"

[Run]
Filename: "{app}\WallpaperChanger.exe"; Description: "Executar WallpaperChanger"; Flags: nowait postinstall
  