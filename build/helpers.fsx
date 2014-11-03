module Helpers

#r "packages/FAKE/tools/FakeLib.dll" // include Fake lib
open Fake
open System


let inline FullNameFromHere (fileName : string) = (__SOURCE_DIRECTORY__ + "/" + fileName)

let CustomRestorePackage solution =
  traceStartTask "CustomRestorePackage" solution
  let (parameters:RestorePackageParams) = RestorePackageDefaults


  let args =" \"restore\" \"" + (solution |> FullName) + "\""

  runNuGetTrial parameters.Retries parameters.ToolPath parameters.TimeOut args (fun () -> failwithf "Package restore of %s  failed." solution)
  traceEndTask "CustomRestorePackage" solution

let CusomtNuGetPack nuspecOrProject version configuration =
  traceStartTask "CusomtNuGetPack" nuspecOrProject

  let toolPath = findNuget (currentDirectory @@ "tools" @@ "NuGet")

  let args = String.concat " " ["pack"; nuspecOrProject; "-version"; version; "-p"; (sprintf "Configuration=%s" configuration) ]

  let result =
    ExecProcess (fun info ->
      info.FileName <- toolPath
      info.WorkingDirectory <- (".." |> FullNameFromHere)
      info.Arguments <- args) (TimeSpan.FromMinutes 5.)

  if result <> 0 then failwithf "Error during NuGet package creation. %s %s" toolPath args

  traceEndTask "CusomtNuGetPack" nuspecOrProject
