# 项目说明

本项目主要将`tcm-comp-dump`工具整理导出的InChiKey标识转换为SMILES格式，同时修正原始数据集中的以下问题：

1. 部分InChiKey无法通过PubChem IEX服务转换到SMILES格式
2. TCMID和TCMSP数据库中并没有结构信息
3. 存在结构信息，但无法被`Indigo.Net`库解析



## 复现流程

相关流程文件已经归档到`tcm-component-smiles.7z`中备用。

1. 将`tcm-comp-dump`生成的文件处理后去 https://pubchem.ncbi.nlm.nih.gov/idexchange/idexchange.cgi 转换为SMILE格式
2. 提取转换失败的InChiKey信息，可用以下代码实现

```fsharp
open System
open System.Collections.Generic
open System.IO
open System.Text

let countLines (lines : string []) = 
    let ret = 
        lines
        |> Array.filter (fun line ->
            let t = line.Split([|'\t'|],2,StringSplitOptions.RemoveEmptyEntries)
            t.Length = 1)
    printfn "%i" (ret.Length)

let test () = 
    let smiles = File.ReadAllLines("20250226 2 Pubchem IEX SMILES.txt")
    let inchi = File.ReadAllLines("20250226 3 Pubchem IEX InChi .txt")
    countLines smiles
    countLines inchi

    let known = ResizeArray<string>()
    let unknown = ResizeArray<string>()


    for line in smiles do 
        let t = line.Split([|'\t'|],2,StringSplitOptions.RemoveEmptyEntries)
        if t.Length = 1 then
            unknown.Add(line)
        else
            known.Add(line)

    File.WriteAllLines("20250226 2 Pubchem IEX SMILES_known.txt", known)
    File.WriteAllLines("20250226 2 Pubchem IEX SMILES_left.txt", unknown)

test ()
```

3. 去TCMID文件里面调阅对应的SMILES结构，整理到文件中
4. TCMSP文件里面包含对应结构的mol结构文件，可以使用`openbabel`进行转换，也整理到文件中
5. 用以下代码进行标准化和检查，对于报错的小分子，谷歌搜索查正确结构

```fsharp
#r "nuget: Indigo.Net, 1.28.0"

open System
open System.IO
open System.Collections.Generic
open System.Text

open com.epam.indigo


let files = 
    [
        "sequence/0 error_override.txt"
        "sequence/1 Pubchem IEX SMILES_known.txt"
        "sequence/2 TCMID.txt"
        "sequence/3 TCMSP_openbabel.txt"
    ]

let out = "Final_Merged.txt"

let dict = Dictionary<string, string>()

let i = new Indigo()
i.setOption("standardize-keep-largest", true)
//i.setOption("standardize-clear-charges", true)
//i.setOption("standardize-remove-single-atoms", true)

for file in files do 
    let lines = File.ReadAllLines(file)
    for line in lines do 
        let t = line.Split([|'\t'; ' '|])
        if not <| dict.ContainsKey(t.[0]) then
            try
                //printfn $"{t.[0]}"
                let mol = i.loadMolecule(t.[1])
                mol.standardize()
                dict.TryAdd(t.[0], mol.canonicalSmiles()) |> ignore
            with
            | e -> printfn $"{t.[0]} {e.Message}"

let ret = 
    dict
    |> Seq.map (fun kv -> $"{kv.Key}\t{kv.Value}")

File.WriteAllLines(out, ret)
```

