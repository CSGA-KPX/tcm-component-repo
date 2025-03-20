#r "nuget: Indigo.Net, 1.28.0"

open System
open System.IO
open System.Collections.Generic
open System.Text

open com.epam.indigo


type SRUResults =
    { Id: int
      Name: string
      OriginalSMILES: string
      DeglycosylatedSMILES: string
      HasSugars: bool }

let sru =
    File.ReadAllLines("deglycosylation_results.txt")
    |> Array.tail
    |> Array.choose (fun line ->
        
        if not <| line.Contains("Deglycosylation not possible because") then
            let t = line.Split(';')
            let id = int t.[0]
            let name = t.[1]
            let osmile = t.[2]
            let dsmile = t.[3]
            let hasSugar = Boolean.Parse(t.[4])

            Some {  Id = id
                    Name = name
                    OriginalSMILES = osmile
                    DeglycosylatedSMILES = dsmile
                    HasSugars = hasSugar }
        else
            printfn $"error: {line}"
            None)

let test () =
    use indigo = new Indigo()
    let dict = Dictionary<string, SRUResults>()
    for ret in sru do 
        if ret.DeglycosylatedSMILES <> "[empty]" then
            let normalized = 
                //printfn $"{ret.DeglycosylatedSMILES}"
                let mol = indigo.loadMolecule(ret.DeglycosylatedSMILES)
                mol.canonicalSmiles()
            if dict.ContainsKey(normalized) then
                printfn $"Adding {ret.Name} failed"
                printfn $"{dict.[normalized].Name} has same normalized SMILES and added"
            else
                dict.Add(normalized, ret)
    let contents = 
        dict
        |> Seq.map (fun kv -> $"{kv.Value.DeglycosylatedSMILES}\t{kv.Value.Name}")
    File.WriteAllLines("sru_post_out.smi", contents)

test()