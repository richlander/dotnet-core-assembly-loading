#!/usr/bin/env pwsh

$BasePath = (Get-Item -Path "./").FullName
$BaseGutenapp = Join-Path $BasePath "gutenapp"
$GutenappDir = Join-Path $BaseGutenapp "gutenapp"
$WordcountDir = Join-Path $BaseGutenapp "wordcount"
$Gutenapp = Join-Path $GutenappDir "gutenapp.csproj"
$Wordcount = Join-Path $WordcountDir "wordcount.csproj"

dotnet build $Gutenapp
dotnet build $Wordcount
dotnet build $Wordcount -c release

Set-Location $GutenappDir
#dotnet run -c debug
#dotnet run -c release

Set-Location $GutenappDir
dotnet publish -c debug -o out
Set-Location $WordcountDir
dotnet publish -c debug -o out

Set-Location $BasePath

$testdir = Join-Path . testdir
New-Item -ItemType Directory -Path $testdir -Force

Copy-Item (Join-Path $GutenappDir out *) $testdir -Force

New-Item -ItemType Directory -Path (Join-Path $testdir wordcount) -Force

Copy-Item (Join-Path $WordcountDir out *) (Join-Path $testdir wordcount) -Force

# dotnet (Join-Path $testdir gutenapp.dll)

# Remove-Item -Recurse ./testdir