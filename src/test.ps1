#!/usr/bin/env pwsh

$BasePath = (Get-Item -Path "./").FullName
$BaseGutenapp = Join-Path $BasePath "gutenapp"
$GutenappDir = Join-Path $BaseGutenapp "gutenapp"
$Gutenapp = Join-Path $GutenappDir "gutenapp.csproj"


$build = Join-Path "." "build.ps1"
$build

dotnet run --project $Gutenapp -c debug
dotnet run --project $Gutenapp -c release