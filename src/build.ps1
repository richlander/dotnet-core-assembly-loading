#!/usr/bin/env pwsh

$BaseGutenapp = Join-Path "." "gutenapp"
$Gutenapp = Join-Path $BaseGutenapp "gutenapp" "gutenapp.csproj"
$Wordcount = Join-Path $BaseGutenapp "wordcount" "wordcount.csproj"

dotnet build $Gutenapp
dotnet build $Wordcount