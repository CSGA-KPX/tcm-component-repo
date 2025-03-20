#r "nuget: FSharp.Data"

open System
open System.IO
open System.Collections.Generic
open System.Text

open FSharp.Data


[<Literal>]
let template = "raw/1"

type Json = JsonProvider<template>


for file in Directory.EnumerateFiles("raw/") do
    let json = Json.Parse(File.ReadAllText(file))
    let title = json.Data.Title
    let sb = 
        StringBuilder()
            .Append("【概述】")
            .Append(json.Data.HtmlContent)
            .Replace("<p>", "")
            .Replace("</p>", "")
            .Replace("<i>", "")
            .Replace("</i>", "")
            .Replace("<b>", "")
            .Replace("</b>", "")
            .Replace("<sub>", "")
            .Replace("</sub>", "")
            .Replace("【性味】", "【性味与归经】")
    let str = sb.ToString()
    let key = "【性味与归经】"
    let posStart = str.IndexOf(key) + key.Length
    let posEnd = str.IndexOf("【", posStart + 1) - 1
    let ret = str.[posStart .. posEnd]
    printfn $"{file} -> {title} : {ret}"