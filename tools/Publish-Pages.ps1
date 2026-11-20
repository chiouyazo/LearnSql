[CmdletBinding()]
param(
    [string]$OutputPath = (Join-Path $PSScriptRoot '../artifacts/pages'),
    [string]$BasePath = '/LearnSql/'
)

$ErrorActionPreference = 'Stop'

if (-not $BasePath.StartsWith('/') -or -not $BasePath.EndsWith('/')) {
    throw "BasePath must start and end with '/', got '$BasePath'."
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$project = Join-Path $repoRoot 'src/LearnSql.Core/LearnSql.Core.csproj'
$publishPath = Join-Path $repoRoot 'artifacts/publish'

if (Test-Path $publishPath) { Remove-Item $publishPath -Recurse -Force }
if (Test-Path $OutputPath) { Remove-Item $OutputPath -Recurse -Force }

dotnet publish $project -c Release -o $publishPath
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish failed.' }

New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
Copy-Item (Join-Path $publishPath 'wwwroot/*') $OutputPath -Recurse -Force

Get-ChildItem $OutputPath -Recurse -File -Include '*.br', '*.gz' | Remove-Item -Force

$indexPath = Join-Path $OutputPath 'index.html'
$index = Get-Content $indexPath -Raw
$rewritten = $index.Replace('<base href="/" />', "<base href=`"$BasePath`" />")
if ($rewritten -eq $index -and $BasePath -ne '/') {
    throw 'Base element not found in index.html.'
}
Set-Content $indexPath $rewritten -NoNewline

Copy-Item $indexPath (Join-Path $OutputPath '404.html') -Force
New-Item -ItemType File -Path (Join-Path $OutputPath '.nojekyll') -Force | Out-Null

$sizeMb = [math]::Round((Get-ChildItem $OutputPath -Recurse -File | Measure-Object Length -Sum).Sum / 1MB, 1)
Write-Host "Published to $OutputPath ($sizeMb MB) for base path $BasePath"
