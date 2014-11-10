$buildFolder =  "$PSScriptRoot\build"
$toolsFolder = "$PSScriptRoot\tools"
$nuget = "$toolsFolder\nuget.exe"
& "$buildFolder\paket.bootstrapper.exe"
Push-Location $buildFolder
& "$buildFolder\paket.exe" restore
Pop-Location

if(-not (Test-Path $toolsFolder)) {
  mkdir $toolsFolder | Out-Null
}

if(-not (Test-Path $nuget)) {
  Invoke-WebRequest "https://nuget.org/nuget.exe" -OutFile $nuget
}

$fake = "$buildFolder\packages\FAKE\tools\Fake.exe"

& $fake "$buildFolder\build.fsx"
if($LASTEXITCODE -ne 0) {
  if($env:APPVEYOR) {
    exit 1
  }
}
