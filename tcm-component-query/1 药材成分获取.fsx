#load "Data/Data.fsx"

open System
open System.Collections.Generic
open System.IO
open System.Text


let file = "1 药材成分获取.txt"

let getChemicals () =
    let fetched = HashSet<Data.SmilesInfo>()

    let herbs =
        let ret = HashSet<string>()
        ret.UnionWith(Data.readHerb (file) Data.HerbSource.TCMID)
        ret.UnionWith(Data.readHerb (file) Data.HerbSource.TCMSP)
        ret

    for herb in Data.herbs do
        if herbs.Overlaps(herb.Herbs) then
            let smiles = Data.smiles.[herb.InChiKey]
            fetched.Add(smiles) |> ignore

    let contents = fetched |> Seq.map (fun item -> $"{item.SMILES}\t{item.InChiKey}")

    File.WriteAllLines("1 药材成分获取.smi", contents)

getChemicals ()
