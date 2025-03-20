module KPX.TCMComp.Dump.Resource

open System
open System.Collections.Generic
open System.IO
open System.Reflection

open SharpCompress.Archives
open SharpCompress.Archives.SevenZip
open SharpCompress.Common
open SharpCompress.Readers
open SharpCompress.Factories


let private TCMID_WEBSITE = "KPX.TCMComp.Dump.TCMID_website.7z"
let private TCMSP_HERBS = "KPX.TCMComp.Dump.TCMSP_Herbs.7z"
let private TCMSP_MOLECULES = "KPX.TCMComp.Dump.TCMSP_Molecules.7z"

let private readResource (name: string) =
    let asm = Assembly.GetExecutingAssembly()

    printfn $"Opening resource {name}"
    asm.GetManifestResourceStream(name)


let private readFileContents (name: string) =
    printfn $"Reading files {name}"

    use stream = readResource name
    use archive = SharpCompress.Archives.SevenZip.SevenZipArchive.Open(stream)
    use reader = archive.ExtractAllEntries()

    // ZipArchive不支持并发访问，只能提前提取内容
    seq {
        while reader.MoveToNextEntry() do
            use stream = reader.OpenEntryStream()
            use sr = new StreamReader(stream)

            (reader.Entry.Key, sr.ReadToEnd())
    }
    |> Seq.toArray

module TcmId =
    let getWebsiteFiles () = readFileContents TCMID_WEBSITE

module TcmSp =
    let getHerbFiles () = readFileContents TCMSP_HERBS

    let getMoleculesFiles () = readFileContents TCMSP_MOLECULES
