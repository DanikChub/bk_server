param([string]$PostgresBin='C:\Program Files\PostgreSQL\17\bin',[int]$Port=55441,[switch]$WithHttp)
$ErrorActionPreference='Stop'
$repoRoot=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$runRoot=Join-Path $repoRoot ('database/.local/backend-'+[guid]::NewGuid().ToString('N'))
$dataRoot=Join-Path $runRoot 'data'
New-Item -ItemType Directory -Path $runRoot -Force | Out-Null
$oldDatabase=$env:BACKEND_TEST_DATABASE_URL
$started=$false
try {
  & (Join-Path $PostgresBin 'initdb.exe') -D $dataRoot -U backend_test -A trust --no-locale -E UTF8 --no-sync > (Join-Path $runRoot 'initdb.log') 2>&1
  if($LASTEXITCODE -ne 0){throw 'initdb failed'}
  & (Join-Path $PostgresBin 'pg_ctl.exe') -D $dataRoot -l (Join-Path $runRoot 'postgres.log') -o "-h 127.0.0.1 -p $Port" -w start
  if($LASTEXITCODE -ne 0){throw 'PostgreSQL start failed'}
  $started=$true
  & (Join-Path $PostgresBin 'createdb.exe') -h 127.0.0.1 -p $Port -U backend_test -T template0 backend_test
  if($LASTEXITCODE -ne 0){throw 'createdb failed'}
  foreach($file in @('database/init.sql','database/migrations/004_next_runtime.sql')) {
    & (Join-Path $PostgresBin 'psql.exe') -X -h 127.0.0.1 -p $Port -U backend_test -d backend_test -v ON_ERROR_STOP=1 -f (Join-Path $repoRoot $file) > (Join-Path $runRoot ([IO.Path]::GetFileName($file)+'.log')) 2>&1
    if($LASTEXITCODE -ne 0){throw "SQL failed: $file"}
  }
  $env:BACKEND_TEST_DATABASE_URL="postgresql://backend_test@127.0.0.1:$Port/backend_test"
  Push-Location (Join-Path $repoRoot 'frontend')
  try {
    $oldAppDatabase=$env:DATABASE_URL
    try {$env:DATABASE_URL=$env:BACKEND_TEST_DATABASE_URL; & npm run db:migrate; if($LASTEXITCODE -ne 0){throw 'Runtime migration CLI failed'}}
    finally {$env:DATABASE_URL=$oldAppDatabase}
    & npm run test:backend; if($LASTEXITCODE -ne 0){throw 'Backend tests failed'}
    if($WithHttp){& node scripts/verify-http.mjs;if($LASTEXITCODE -ne 0){throw 'HTTP verification failed'}}
  }
  finally { Pop-Location }
  Write-Output "Backend verification logs: $runRoot"
} finally {
  if($started){& (Join-Path $PostgresBin 'pg_ctl.exe') -D $dataRoot -m fast -w stop}
  $env:BACKEND_TEST_DATABASE_URL=$oldDatabase
}
