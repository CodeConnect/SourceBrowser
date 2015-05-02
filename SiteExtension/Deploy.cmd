SET FileToDelete=sourcebrowser.zip

IF EXIST %FileToDelete% del /F %FileToDelete%

powershell.exe -nologo -noprofile -command "& { Add-Type -A 'System.IO.Compression.FileSystem'; [IO.Compression.ZipFile]::CreateFromDirectory('PublishProfiles', 'sourcebrowser.zip'); }"

@if "%_echo%" neq "1" @echo off

setlocal
set _SCRIPT=%~nx0
set _SCMURI=%~1
set _CURLEXE=%ProgramFiles(x86)%\git\bin\curl.exe

REM first parameter is the deploy uri with embedded cred
if "%_SCMURI%" equ "" (
  call :USAGE 
  goto :EOF
)

REM remove any after .net
set _SCMURI=%_SCMURI:.net=&rem.%
set _SCMURI=%_SCMURI%.net

if NOT EXIST "%FileToDelete%" (
  @echo "%FileToDelete%" does not exists! 
  goto :EOF
)

if NOT EXIST "%_CURLEXE%" (
  @echo "%_CURLEXE%" does not exists! 
  goto :EOF
)

@echo.
@echo "%_CURLEXE%" -k -v -T "%FileToDelete%" "%_SCMURI%/zip"
@echo.
@call "%_CURLEXE%" -k -v -T "%FileToDelete%" "%_SCMURI%/zip"
@echo.

exit /b 0

:USAGE
@echo usage: %_SCRIPT% "<scm_uri>"
exit /b 0
 
REM testing