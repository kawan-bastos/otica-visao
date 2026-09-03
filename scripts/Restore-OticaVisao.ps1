[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string]$BackupDirectory,
    [Parameter(Mandatory)] [switch]$ConfirmRestore,
    [string]$ConnectionString,
    [string]$ImagePath = (Join-Path $PSScriptRoot '..\src\OticaVisao.Web\App_Data\frame-images')
)

$ErrorActionPreference = 'Stop'
if (-not $ConfirmRestore) { throw 'Use -ConfirmRestore para confirmar a substituição do banco e das fotos.' }

$resolvedBackup = [IO.Path]::GetFullPath($BackupDirectory)
$databaseFile = Join-Path $resolvedBackup 'database.dump'
$imagesFile = Join-Path $resolvedBackup 'frame-images.zip'
$manifestFile = Join-Path $resolvedBackup 'manifest.json'
if (-not (Test-Path -LiteralPath $databaseFile -PathType Leaf) -or
    -not (Test-Path -LiteralPath $imagesFile -PathType Leaf) -or
    -not (Test-Path -LiteralPath $manifestFile -PathType Leaf)) {
    throw 'O diretório não contém um pacote de backup completo.'
}

function Get-LocalConnectionString {
    if ($ConnectionString) { return $ConnectionString }
    $project = Join-Path $PSScriptRoot '..\src\OticaVisao.Web'
    $secret = dotnet user-secrets list --project $project |
        Where-Object { $_ -like 'ConnectionStrings:DefaultConnection = *' } |
        Select-Object -First 1
    if ($secret) { return ($secret -split ' = ', 2)[1] }
    $settings = Get-Content -LiteralPath (Join-Path $project 'appsettings.Development.json') -Raw | ConvertFrom-Json
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
    Where-Object { Test-Path -LiteralPath (Join-Path $_ 'pg_restore.exe') } |
    Select-Object -First 1
$pgRestore = if ($postgresBin) { Join-Path $postgresBin 'pg_restore.exe' } else { (Get-Command pg_restore -ErrorAction Stop).Source }

$previousPassword = $env:PGPASSWORD
try {
    $env:PGPASSWORD = $connection.Password
    & $pgRestore --host=$($connection.Host) --port=$($connection.Port ?? '5432') --username=$($connection.Username) --dbname=$($connection.Database) --clean --if-exists --no-owner --no-privileges $databaseFile
    if ($LASTEXITCODE -ne 0) { throw 'O PostgreSQL não conseguiu restaurar o backup.' }
}
finally {
    $env:PGPASSWORD = $previousPassword
}

$resolvedImages = [IO.Path]::GetFullPath($ImagePath)
if ((Split-Path -Leaf $resolvedImages) -ne 'frame-images') {
    throw 'Por segurança, o destino das fotos deve terminar com frame-images.'
}
$imageParent = Split-Path -Parent $resolvedImages
$temporaryImages = Join-Path $imageParent ('.restore-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temporaryImages -Force | Out-Null
try {
    Expand-Archive -LiteralPath $imagesFile -DestinationPath $temporaryImages -Force
    $extracted = Join-Path $temporaryImages 'frame-images'
    if (-not (Test-Path -LiteralPath $extracted -PathType Container)) { throw 'O arquivo de fotos do backup é inválido.' }
    if (Test-Path -LiteralPath $resolvedImages) { Remove-Item -LiteralPath $resolvedImages -Recurse -Force }
    Move-Item -LiteralPath $extracted -Destination $resolvedImages
}
finally {
    if (Test-Path -LiteralPath $temporaryImages) { Remove-Item -LiteralPath $temporaryImages -Recurse -Force }
}

Write-Output "Backup restaurado: $resolvedBackup"
