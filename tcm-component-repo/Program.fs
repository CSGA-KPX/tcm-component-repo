module KPX.TCMComp.Dump.Program

open System
open System.Text
open System.IO
open System.Collections.Generic

open Newtonsoft.Json
open Newtonsoft.Json.Linq

open KPX.TCMComp.Dump
open KPX.TCMComp.Dump.DataStructs

type Dumped = 
    {
        InChiKey : string
        Herbs : HashSet<string>
        Name : string
    }

[<EntryPoint>]
let main args = 
    // 麻烦东西
    //Text.Encoding.RegisterProvider(Text.CodePagesEncodingProvider.Instance)

    let dumpedDict = Dictionary<string, Dumped>()

    let inline addOrUpdate (c : ChemicalInfo) = 
        let key = c.InCHIKey
        if dumpedDict.ContainsKey (key) then
            let seq = seq {yield! c.OccurrenceInHerbs; yield! dumpedDict.[key].Herbs}
            dumpedDict.[key] <- {dumpedDict.[key] with Herbs = HashSet<_>(seq)}
        else
            dumpedDict.Add(key, {InChiKey = c.InCHIKey; Herbs = c.OccurrenceInHerbs; Name = c.Name})

    // 如果注释这一行，则需要去掉DumpTCMSP中的Array.Parallel.map操作换成Array.map
    for ret in TCMID.parseTCMID().Chemicals do
        addOrUpdate ret

    for ret in TCMSP.parseTCMSP().Chemicals do
        addOrUpdate ret


    Console.WriteLine("Writing to dumped.json")

    File.WriteAllText("dumped.json", JsonConvert.SerializeObject(dumpedDict.Values |> Seq.toArray))

    Console.WriteLine("Done")
    Console.ReadLine() |> ignore
    0