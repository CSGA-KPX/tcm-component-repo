#load "Data/Data.fsx"

open System
open System.Collections.Generic
open System.IO
open System.Text


let check () =
    let tcmidPass = ResizeArray<string>()

    for name in Data.readHerb "0 药材名称检查.txt" Data.TCMID do
        if Data.tcmidNames.Contains(name) then
            tcmidPass.Add(name) |> ignore
        else
            printfn $"NOT EXIST TCM_ID : {name}"

    printfn $"TCMID PASSS = {String.Join(',', tcmidPass)}"

    let tcmspPass = ResizeArray<string>()

    for name in Data.readHerb "0 药材名称检查.txt" Data.TCMSP do
        if Data.tcmspNames.Contains(name) then
            tcmspPass.Add(name) |> ignore
        else
            printfn $"NOT EXIST TCM_SP : {name}"

    printfn $"TCMSP PASSS = {String.Join(',', tcmspPass)}"


check ()
