#!/usr/bin/env pwsh

$BasePath = (Get-Item -Path "./").FullName
$BaseGutenapp = Join-Path $BasePath "gutenapp"
$GutenappDir = Join-Path $BaseGutenapp "gutenapp"
$Gutenapp = Join-Path $GutenappDir "gutenapp.csproj"
$WordcountDir = Join-Path $BaseGutenapp "wordcount"
$Wordcount = Join-Path $WordcountDir "wordcount.csproj"
$MostcommonwordsDir = Join-Path $BaseGutenapp "mostcommonwords"
$Mostcommonwords = Join-Path $MostcommonwordsDir "mostcommonwords.csproj"

dotnet publish $Gutenapp -c release -o out
dotnet publish $Wordcount -c release -o out
dotnet publish $Mostcommonwords -c release -o out

Set-Location $BasePath

$testdir = Join-Path . testdir
New-Item -ItemType Directory -Path $testdir -Force
Copy-Item ([System.IO.Path]::Combine($GutenappDir, "out", "*")) $testdir -Force
New-Item -ItemType Directory -Path (Join-Path $testdir wordcount) -Force
Copy-Item ([System.IO.Path]::Combine($WordcountDir, "out", "*")) (Join-Path $testdir wordcount) -Force
New-Item -ItemType Directory -Path (Join-Path $testdir mostcommonwords) -Force
Copy-Item ([System.IO.Path]::Combine($MostcommonwordsDir, "out", "*")) (Join-Path $testdir mostcommonwords) -Force

dotnet (Join-Path $testdir gutenapp.dll)