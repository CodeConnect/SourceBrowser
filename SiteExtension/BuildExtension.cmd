@echo %WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild ..\src\SourceBrowser.Site\SourceBrowser.Site.csproj /p:DeployOnBuild=true /p:PublishProfile=local


powershell.exe -nologo -noprofile -command "& { Add-Type -A 'System.IO.Compression.FileSystem'; [IO.Compression.ZipFile]::CreateFromDirectory('PublishProfiles', 'sourcebrowser.zip'); }"