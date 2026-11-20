[CmdletBinding()]
param(
    [string]$Path = (Join-Path $PSScriptRoot '../artifacts/pages'),
    [string]$BasePath = '/LearnSql/',
    [int]$Port = 8080
)

$ErrorActionPreference = 'Stop'

$root = Resolve-Path $Path
$serverScript = Join-Path $PSScriptRoot 'serve_pages.py'
python3 $serverScript $root $BasePath $Port
