module KPX.TCMComp.Dump.TCMID

open System
open System.Collections.Generic

open HtmlAgilityPack
open KPX.TCMComp.Dump.Resource
open KPX.TCMComp.Dump.DataStructs


let parseTCMID () =
    let pages = TcmId.getWebsiteFiles()

    let herbs =
        pages
        |> Array.Parallel.map (fun (name, html) ->
            let doc = HtmlDocument()
            doc.LoadHtml(html)

            let herb = HerbInfo()

            seq {
                let prop =
                    doc.DocumentNode.SelectSingleNode("//div[h4[text()=\"Description of the Component\"]]")

                yield! prop.SelectNodes(".//h5 | .//h6")
            }
            |> Seq.iter (fun x ->
                let title = x.InnerText.Trim()
                let content = x.ParentNode.NextSibling.NextSibling.InnerText.Trim()

                match title with
                | "Component ID" -> herb.Id <- content
                | "Latin Name" -> herb.LatinName <- content
                | "English Name" -> herb.EnglishName <- content
                | "Chinese Pinyin Name" -> herb.PinyinName <- content
                | "中文名" -> herb.ChineseName <- content
                | "TCM Properties" -> ()
                | "TCM Meridians" -> ()
                | "Therapeutic Class English" -> ()
                | "Therapeutic Class Chinese" -> ()
                | "Functions" -> ()
                | "Toxicity" -> ()
                | "Geo-authentic habitats (道地产区)" -> ()
                | "Reference" -> ()
                | "Barcode ID" -> ()
                | "Barcode Source" -> ()
                | str -> failwithf $"Unknown filed {str}")

            let modals = doc.DocumentNode.SelectNodes("//div[@class='modal']")

            let chemicalCache = Dictionary<string, ChemicalInfo>()

            let inline tryGetCache (id: string) =
                if not (chemicalCache.ContainsKey(id)) then
                    chemicalCache.Add(id, ChemicalInfo(id))

                chemicalCache.[id].Identifiers.Add(id) |> ignore
                chemicalCache.[id]

            if isNull modals then
                printfn $"指定药物不包含分子信息 : {name} -> {herb.ChineseName}"
            else
                for modal in modals do
                    let tcmcId = modal.GetAttributeValue("id", String.Empty)

                    if tcmcId = String.Empty then
                        failwithf ""

                    let tcmc = tryGetCache (tcmcId)

                    for node in modal.SelectNodes(".//font") do
                        let readValue (node: HtmlNode) =
                            node.ParentNode.NextSibling.InnerText.Trim()

                        match node.InnerText with
                        | "Common Name" -> tcmc.Name <- readValue node
                        | "Standard InCHIKey" -> tcmc.InCHIKey <- readValue node
                        | _ -> ()

                    modal.SelectNodes(".//a[contains(text(),'TCMH')]")
                    |> Seq.map (fun node -> node.InnerText.Trim())
                    |> Seq.iter (tcmc.OccurrenceInHerbs.Add >> ignore)

            herb.Chemicals <- chemicalCache.Values |> Seq.toArray

            herb)

    let tcmhToHerbName = 
        seq {
            for herb in herbs do
                herb.Id, herb.ChineseName
        }
        |> readOnlyDict

    let totalChemicals = 
        [|
            for herb in herbs do
                yield! herb.Chemicals
        |]
        |> Array.map (fun x -> 
            let herbs = 
                x.OccurrenceInHerbs
                |> Seq.map (fun tcmh -> 
                    if not <| tcmhToHerbName.ContainsKey(tcmh) then
                        failwithf $"NOT EXIST {tcmh}"
                    tcmhToHerbName.[tcmh])
                |> Seq.toArray
            x.OccurrenceInHerbs.Clear()
            herbs |> Array.iter (x.OccurrenceInHerbs.Add >> ignore)

            x)
        |> Array.distinctBy (fun chemi -> chemi.Id)

    { ParseResult.Herbs = herbs
      Chemicals = totalChemicals }