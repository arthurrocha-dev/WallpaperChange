Set objShell = CreateObject("WScript.Shell") 
objShell.CurrentDirectory = CreateObject("Scripting.FileSystemObject").GetParentFolderName(WScript.ScriptFullName)
objShell.Run "WallpaperChanger.exe", 0, False
