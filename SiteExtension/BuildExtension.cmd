%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild ..\src\SourceBrowser.sln /p:DeployOnBuild=true /p:PublishProfile=local.pubxml


SET FileToDelete="sourcebrowser.zip"

IF EXIST %FileToDelete% del /F %FileToDelete%

IF EXIST %FileToDelete% exit 1

powershell.exe -nologo -noprofile -command "& { Add-Type -A 'System.IO.Compression.FileSystem'; [IO.Compression.ZipFile]::CreateFromDirectory('PublishProfiles', 'sourcebrowser.zip'); }"