# run.ps1

$framework = "net6.0"
$dllPath = "bin/Debug/$framework/SpotifyCLIWrapper.dll"

if (-Not (Test-Path $dllPath)) {
    Write-Host "⚠️  Build output not found. Building..."
    dotnet build
}

Write-Host "▶️  Running: dotnet $dllPath $args"
dotnet $dllPath @args
