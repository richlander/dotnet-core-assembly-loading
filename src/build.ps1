#!/usr/bin/env pwsh

$BasePath = (Get-Item -Path "./").FullName
$BaseGutenapp = Join-Path $BasePath "gutenapp"
$GutenappDir = Join-Path $BaseGutenapp "gutenapp"
$Gutenapp = Join-Path $GutenappDir "gutenapp.csproj"
$WordcountDir = Join-Path $BaseGutenapp "wordcount"
$Wordcount = Join-Path $WordcountDir "wordcount.csproj"
$MostcommonwordsDir = Join-Path $BaseGutenapp "mostcommonwords"
$Mostcommonwords = Join-Path $MostcommonwordsDir "mostcommonwords.csproj"

dotnet build $Gutenapp
dotnet build $Wordcount
dotnet build $Mostcommonwords
