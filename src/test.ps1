#!/usr/bin/env pwsh

$BasePath = (Get-Item -Path "./").FullName
$BaseGutenapp = Join-Path $BasePath "gutenapp"
$GutenappDir = Join-Path $BaseGutenapp "gutenapp"
$Gutenapp = Join-Path $GutenappDir "gutenapp.csproj"


.\build.ps1

dotnet run --project $Gutenapp -c debug
dotnet run --project $Gutenapp -c release