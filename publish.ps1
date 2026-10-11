#.\publish.ps1
#.\publish.ps1 -Only win-fd
#.\publish.ps1 -Only win-sc

[CmdletBinding()]
param(
    [ValidateSet('all', 'win-fd', 'win-sc')]
    [string] $Only = 'all'
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path

$proj = [System.IO.Path]::Combine($root, 'src', 'SaveManager.UI', 'SaveManager.UI.csproj')
$outRoot = [System.IO.Path]::Combine($root, 'publish')

$targets = @(
    @{ Key = 'win-fd';   Name = 'win-x64';                 Rid = 'win-x64';  SelfContained = $false }
    @{ Key = 'win-sc';   Name = 'win-x64-self-contained';  Rid = 'win-x64';  SelfContained = $true }
)

$selected = if ($Only -eq 'all') { $targets } else { $targets | Where-Object { $_.Key -eq $Only } }

foreach ($t in $selected) {
    $out = [System.IO.Path]::Combine($outRoot, $t.Name)

    if (Test-Path $out) { Remove-Item $out -Recurse -Force }

    Write-Host ''
    Write-Host "==> $($t.Name)" -ForegroundColor Cyan

    $args = @(
        'publish', $proj
        '-c', 'Release'
        '-r', $t.Rid
        '--self-contained', $t.SelfContained.ToString().ToLowerInvariant()
        '-p:PublishSingleFile=true'
        '-p:IncludeNativeLibrariesForSelfExtract=true'
        '-p:DebugType=None'
        '-p:DebugSymbols=false'
        '-o', $out
    )

    & dotnet @args
    if ($LASTEXITCODE -ne 0) { throw "publish failed for $($t.Name)" }

    Get-ChildItem $out -Filter '*.pdb' -EA SilentlyContinue | Remove-Item -Force

    $files = Get-ChildItem $out -File
    $total = ($files | Measure-Object -Property Length -Sum).Sum / 1MB

    Write-Host ("    {0} file(s), {1:N1} MB" -f $files.Count, $total) -ForegroundColor DarkGray
    $files | ForEach-Object { Write-Host ("      {0,-28} {1,7:N1} MB" -f $_.Name, ($_.Length / 1MB)) -ForegroundColor DarkGray }
}

Write-Host ''