module KPX.TCMComp.Dump.TCMSP

open System
open System.IO
open System.Collections.Generic

open Newtonsoft.Json
open Newtonsoft.Json.Linq

open KPX.TCMComp.Dump.Resource
open KPX.TCMComp.Dump.DataStructs


type MolecularInfo =
    { [<JsonProperty("MOL_ID")>]
      MolecularIdStr: string
      [<JsonProperty("molecule_name")>]
      MolecularName: string
      [<JsonProperty("inchikey")>]
      InCHIKey: string }

    member x.ToChemicalInfo() =
        let chemi = ChemicalInfo(x.MolecularIdStr)
        chemi.Identifiers.Add(x.MolecularIdStr) |> ignore
        chemi.InCHIKey <- x.InCHIKey
        chemi.Name <- x.MolecularName

        chemi

[<CLIMutable>]
type HerbMolecularInfo =
    { [<JsonProperty("molecule_ID")>]
      MolecularId: int
      [<JsonProperty("MOL_ID")>]
      MolecularIDStr: string
      [<JsonProperty("molecule_name")>]
      MolecularName: string }

[<CLIMutable>]
type HerbMolecularInfoEx =
    { [<JsonProperty("molecule_ID")>]
      MolecularId: Nullable<int>
      [<JsonProperty("MOL_ID")>]
      MolecularIDStr: string
      [<JsonProperty("molecule_name")>]
      MolecularName: string }

let parseTCMSP () =
    let readJson (str: string) =
        let startIndex = str.IndexOf('[')
        let endIndex = str.LastIndexOf(']')

        if startIndex >= 0 && endIndex > startIndex then
            let jsonPart = str.Substring(startIndex, endIndex - startIndex + 1)
            Some(jsonPart)
        else
            None

    let molCache =
        let dict = Dictionary<string, ChemicalInfo>()

        TcmSp.getMoleculesFiles ()
        |> Array.Parallel.map (fun (_, f) ->
            let str =
                f.Split([| "\r\n"; "\r"; "\n" |], StringSplitOptions.None)
                |> Seq.find (fun line -> line.Contains("var mol = "))
                |> readJson

            JObject.Parse(str.Value.Trim([| '['; ']' |])).ToObject<MolecularInfo>())
        |> Array.iter (fun molData -> dict.Add(molData.MolecularIdStr, molData.ToChemicalInfo()))

        dict.AsReadOnly()

    printfn "分子结果处理完毕"

    let herbs =
        TcmSp.getHerbFiles ()
        |> Array.Parallel.map (fun (fullName, str) ->
            let name = Path.GetFileNameWithoutExtension(fullName)
            let herb = HerbInfo()
            herb.ChineseName <- name

            let molData =
                let str =
                    str.Split([| "\r\n"; "\r"; "\n" |], StringSplitOptions.None)
                    |> Seq.find (fun line -> line.Contains("\"file_ID\":"))
                    |> readJson

                try
                    Some(JArray.Parse(str.Value).ToObject<HerbMolecularInfoEx[]>())
                with ex ->
                    printfn "%s" str.Value
                    printfn "JSON 解析错误 (%s): %s" name ex.Message
                    None

            if molData.IsSome then
                let chemicals =
                    molData.Value
                    |> Array.choose (fun drugMol ->
                        if drugMol.MolecularId.HasValue then
                            molCache.[drugMol.MolecularIDStr].OccurrenceInHerbs.Add(name) |> ignore
                            Some(molCache.[drugMol.MolecularIDStr])
                        else
                            None)

                herb.Chemicals <- chemicals

            herb)

    { Herbs = herbs
      Chemicals = molCache.Values |> Seq.toArray }
