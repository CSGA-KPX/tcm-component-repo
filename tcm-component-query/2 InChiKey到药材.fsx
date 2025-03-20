#load "Data/Data.fsx"

open System
open System.Collections.Generic
open System.IO
open System.Text


let utf8 = UTF8Encoding(true)

let multipleHerbChemiLimit = Int32.MaxValue

let check() = 
    let herbs = HashSet<string>()
    for h in Data.readHerb "2 InChiKey到药材_herb.txt" Data.HerbSource.TCMID do
        herbs.Add(h) |> ignore
    for h in Data.readHerb "2 InChiKey到药材_herb.txt" Data.HerbSource.TCMSP do
        herbs.Add(h) |> ignore

    let chemiDict =
        let dict = Dictionary<string, Data.HerbInfo>()

        for chemi in Data.herbs do
            dict.Add(chemi.InChiKey, chemi)

        dict

    let ret = 
        File.ReadAllLines("2 InChiKey到药材.txt")
        |> Array.distinct
        |> Array.map (fun key ->
            let item = chemiDict.[key]
            let herbs = item.Herbs |> Seq.filter herbs.Contains |> HashSet
            {item with Herbs = herbs}
        )

    // 第一份数据，原始结果
    File.WriteAllLines("2 InChiKey到药材_out_name.csv", ret |> Array.map (fun item -> $"{item.InChiKey},\"{String.Join(',', item.Herbs)}\",\"{item.Name}\""),utf8)

    // 第二份，按照药材整理

    let summarizeDicts (herbs : Data.HerbInfo[]) = 
        let dict = Dictionary<string, int>()
        for item in herbs do 
            for h in item.Herbs do
                if not <| dict.ContainsKey(h) then
                    dict.[h] <- 0
                if item.Herbs.Count < multipleHerbChemiLimit then
                    dict.[h] <- dict.[h] + 1
        dict.AsReadOnly()

    let totalHerbsChemicals = summarizeDicts Data.herbs
    let knownChemicals = summarizeDicts ret

    File.WriteAllLines("2 InChiKey到药材_out_count.csv", knownChemicals |> Seq.map (fun kv -> 
        let known = float <| kv.Value
        let total = float <| totalHerbsChemicals.[kv.Key]
        {|Name = kv.Key; Count = known; Ratio = known/total|}
        )
        |> Seq.sortByDescending (fun item -> item.Ratio)
        |> Seq.map (fun item -> $"{item.Name},{item.Count},{item.Ratio}"),utf8)

check()