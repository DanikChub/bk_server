param(
    [string]$PostgresBin = 'C:\Program Files\PostgreSQL\18\bin',
    [int]$Port = 55440
)
$ErrorActionPreference = 'Stop'
$databaseRoot = Split-Path -Parent $PSScriptRoot
$runRoot = Join-Path $databaseRoot ('.local/verify-' + [guid]::NewGuid().ToString('N'))
$dataRoot = Join-Path $runRoot 'data'
$pgctl = Join-Path $PostgresBin 'pg_ctl.exe'
$psql = Join-Path $PostgresBin 'psql.exe'
$initdb = Join-Path $PostgresBin 'initdb.exe'
$createdb = Join-Path $PostgresBin 'createdb.exe'
foreach ($binary in @($pgctl, $psql, $initdb, $createdb)) {
    if (-not (Test-Path -LiteralPath $binary)) { throw "Missing PostgreSQL executable: $binary" }
}
if (-not (Get-Command node -ErrorAction SilentlyContinue)) { throw 'Node.js is required to run the verification scripts.' }
New-Item -ItemType Directory -Path $runRoot -Force | Out-Null
$started = $false
$envNames = @('PSQL','PGHOST','PGPORT','PGUSER','PGDATABASE','PGPASSWORD','PGOPTIONS','PGSERVICE','PGSERVICEFILE')
$savedEnvironment = @{}
foreach ($name in $envNames) { $savedEnvironment[$name] = [Environment]::GetEnvironmentVariable($name, 'Process') }
try {
    # Always use a new private cluster, never a connection string from the old application.
    & $initdb -D $dataRoot -U schema_verify -A trust --no-locale -E UTF8 --no-sync > (Join-Path $runRoot 'initdb.log') 2>&1
    if ($LASTEXITCODE -ne 0) { throw "initdb failed; inspect $runRoot/initdb.log" }
    & $pgctl -D $dataRoot -l (Join-Path $runRoot 'postgres.log') -o "-h 127.0.0.1 -p $Port" -w start
    if ($LASTEXITCODE -ne 0) { throw 'Temporary PostgreSQL did not start (check whether the port is available).' }
    $started = $true
    $env:PSQL = $psql
    $env:PGHOST = '127.0.0.1'
    $env:PGPORT = [string]$Port
    $env:PGUSER = 'schema_verify'
    $env:PGDATABASE = 'schema_verification'
    foreach ($name in @('PGPASSWORD','PGOPTIONS','PGSERVICE','PGSERVICEFILE')) { Remove-Item -LiteralPath "Env:$name" -ErrorAction SilentlyContinue }
    & $createdb -h 127.0.0.1 -p $Port -U schema_verify -T template0 schema_verification
    if ($LASTEXITCODE -ne 0) { throw 'Could not create the disposable database.' }
    & $psql -X -v ON_ERROR_STOP=1 -f (Join-Path $databaseRoot 'init.sql') > (Join-Path $runRoot 'init.log') 2>&1
    if ($LASTEXITCODE -ne 0) { throw "Initialization failed; inspect $runRoot/init.log" }
    & node (Join-Path $databaseRoot 'tests/verify.mjs')
    if ($LASTEXITCODE -ne 0) { throw 'Schema verification failed.' }
    Write-Output "Verification logs: $runRoot"
}
finally {
    if ($started) {
        & $pgctl -D $dataRoot -m fast -w stop
        if ($LASTEXITCODE -ne 0) { Write-Warning "Stop the temporary cluster manually: $pgctl -D $dataRoot -m fast -w stop" }
    }
    foreach ($name in $envNames) {
        if ($null -eq $savedEnvironment[$name]) { Remove-Item -LiteralPath "Env:$name" -ErrorAction SilentlyContinue }
        else { [Environment]::SetEnvironmentVariable($name, $savedEnvironment[$name], 'Process') }
    }
}
