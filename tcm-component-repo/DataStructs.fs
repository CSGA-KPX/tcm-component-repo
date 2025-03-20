namespace KPX.TCMComp.Dump.DataStructs

open System
open System.Text
open System.IO
open System.Collections.Generic


type ChemicalInfo(id: string) =
    member val Id = id
    member val InCHIKey = String.Empty with get, set
    member val Name = String.Empty with get, set
    /// TCMID或TCMSP编号
    member val Identifiers: HashSet<string> = HashSet<string>()
    /// 出现药物
    member val OccurrenceInHerbs: HashSet<string> = HashSet<string>()

type HerbInfo() =
    member val Id = String.Empty with get, set
    member val LatinName = String.Empty with get, set
    member val EnglishName = String.Empty with get, set
    member val PinyinName = String.Empty with get, set
    member val ChineseName = String.Empty with get, set
    member val Chemicals: ChemicalInfo[] = [||] with get, set

type ParseResult = 
    {
        Herbs : HerbInfo []
        Chemicals : ChemicalInfo []
    }