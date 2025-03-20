#r "nuget: Newtonsoft.Json"

open System
open System.IO
open System.Collections.Generic

open Newtonsoft.Json


type HerbInfo =
    { InChiKey: string
      Herbs: HashSet<string>
      Name: string }

let herbs =
    JsonConvert.DeserializeObject<HerbInfo[]>(File.ReadAllText(__SOURCE_DIRECTORY__ + "/Dumped.json"))

type SmilesInfo = { InChiKey: string; SMILES: string }

let smiles =
    seq {
        let lines = File.ReadAllLines(__SOURCE_DIRECTORY__ + "/Final_Merged.csv")

        for line in lines do
            let t = line.Split('\t', 2)
            t.[0], { InChiKey = t.[0]; SMILES = t.[1] }
    }
    |> readOnlyDict


let tcmidNames =
    File.ReadAllLines(__SOURCE_DIRECTORY__ + "/tcmid_Names.txt") |> HashSet

let tcmspNames =
    File.ReadAllLines(__SOURCE_DIRECTORY__ + "/tcmsp_Names.txt") |> HashSet

type HerbSource = 
    | TCMID
    | TCMSP

let readHerb (file) (source : HerbSource) =
    /// replace应该有两种用法，替换和补充，都有
    let herbNameReplace =
        seq {
            
            let file = 
                match source with
                | TCMID -> __SOURCE_DIRECTORY__ + "/tcmid_Replace.csv"
                | TCMSP -> __SOURCE_DIRECTORY__ + "/tcmsp_Replace.csv"

            let lines = File.ReadAllLines(file)

            for line in lines do
                let t = line.Split(',')
                t.[0], t.[1]
        }
        |> readOnlyDict

    File
        .ReadAllText(file)
        .Split(
            [| '、'; '，'; '\r'; '\n'; '\t' |],
            StringSplitOptions.RemoveEmptyEntries ||| StringSplitOptions.TrimEntries
        )
    |> Array.map (fun herb ->
        let succ, item = herbNameReplace.TryGetValue(herb)
        if succ then item else herb)
    |> HashSet
