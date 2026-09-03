[CmdletBinding()]
param(
    [string]$Destination = (Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'OticaVisaoBackups'),
    [string]$ConnectionString,
    [string]$ImagePath = (Join-Path $PSScriptRoot '..\src\OticaVisao.Web\App_Data\frame-images')
)

$ErrorActionPreference = 'Stop'

function Get-LocalConnectionString {
    if ($ConnectionString) { return $ConnectionString }
    $project = Join-Path $PSScriptRoot '..\src\OticaVisao.Web'
    $secret = dotnet user-secrets list --project $project |
        Where-Object { $_ -like 'ConnectionStrings:DefaultConnection = *' } |
        Select-Object -First 1
    if ($secret) { return ($secret -split ' = ', 2)[1] }

    $settingsPath = Join-Path $project 'appsettings.Development.json'
    $settings = Get-Content -LiteralPath $settingsPath -Raw | ConvertFrom-Json
    return $settings.ConnectionStrings.DefaultConnection
}

function ConvertTo-ConnectionArguments([string]$value) {
    $parts = @{}
    foreach ($item in ($value -split ';')) {
        if ($item -match '^\s*([^=]+)=(.*)$') { $parts[$matches[1].Trim().ToLowerInvariant()] = $matches[2].Trim() }
    }
    return @{
        Host = $parts['host']; Port = $parts['port']; Database = $parts['database'];
        Username = $parts['username']; Password = $parts['password']
    }
}

$connection = ConvertTo-ConnectionArguments (Get-LocalConnectionString)
if (-not $connection.Host -or -not $connection.Database -or -not $connection.Username) {
    throw 'A conexão precisa informar Host, Database e Username.'
}

$postgresBin = Get-ChildItem -LiteralPath 'C:\Program Files\PostgreSQL' -Directory -ErrorAction SilentlyContinue |
    Sort-Object Name -Descending |
    ForEach-Object { Join-Path $_.FullName 'bin' } |
    Where-Object { Test-Path -LiteralPath (Join-Path $_ 'pg_dump.exe') } |
    Select-Object -First 1
$pgDump = if ($postgresBin) { Join-Path $postgresBin 'pg_dump.exe' } else { (Get-Command pg_dump -ErrorAction Stop).Source }

$resolvedDestination = [IO.Path]::GetFullPath($Destination)
$resolvedImages = [IO.Path]::GetFullPath($ImagePath)
$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$backupDirectory = Join-Path $resolvedDestination $timestamp
New-Item -ItemType Directory -Path $backupDirectory -Force | Out-Null

$databaseFile = Join-Path $backupDirectory 'database.dump'
$imagesFile = Join-Path $backupDirectory 'frame-images.zip'
$previousPassword = $env:PGPASSWORD
try {
    $env:PGPASSWORD = $connection.Password
    & $pgDump --host=$($connection.Host) --port=$($connection.Port ?? '5432') --username=$($connection.Username) --dbname=$($connection.Database) --format=custom --file=$databaseFile
    if ($LASTEXITCODE -ne 0) { throw 'O PostgreSQL não conseguiu criar o backup.' }
}
finally {
    $env:PGPASSWORD = $previousPassword
}

$imageStaging = Join-Path $backupDirectory '.image-staging'
$stagedImages = Join-Path $imageStaging 'frame-images'
New-Item -ItemType Directory -Path $stagedImages -Force | Out-Null
try {
    if (Test-Path -LiteralPath $resolvedImages) {
        Get-ChildItem -LiteralPath $resolvedImages -Force |
            Copy-Item -Destination $stagedImages -Recurse -Force
    }
    [IO.Compression.ZipFile]::CreateFromDirectory($imageStaging, $imagesFile, [IO.Compression.CompressionLevel]::Optimal, $false)
}
finally {
    Remove-Item -LiteralPath $imageStaging -Recurse -Force
}

$manifest = [ordered]@{
    createdAt = (Get-Date).ToString('o')
    database = $connection.Database
    databaseFile = 'database.dump'
    imagesFile = 'frame-images.zip'
    formatVersion = 1
}
$manifest | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $backupDirectory 'manifest.json') -Encoding utf8

Write-Output "Backup criado em: $backupDirectory"
