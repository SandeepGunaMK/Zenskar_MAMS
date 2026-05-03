param (
    [string]$installPath,
    [string]$repoUrl,
    [string]$branch,
    [string]$token
)

$logFile = Join-Path $env:TEMP "update_log.txt"

function Log($msg) {
    $msg | Out-File -Append $logFile
}

try {
    Log "==== UPDATE START ===="
    Log "InstallPath: $installPath"
    Log "Repo: $repoUrl"
    Log "Branch: $branch"

    Start-Sleep -Seconds 3

    $tempPath = Join-Path $env:TEMP "AppUpdateTemp"

    if (Test-Path $tempPath) {
        Log "Cleaning temp..."
        Remove-Item $tempPath -Recurse -Force
    }

    # 🔴 Check git availability
    $gitCheck = Get-Command git -ErrorAction SilentlyContinue
    if (-not $gitCheck) {
        Log "ERROR: git not found in PATH"
        exit 1
    }

    Log "Git found"

    # Inject token
    $repoWithToken = $repoUrl -replace "https://", "https://$token@"
    Log "Cloning repo..."

    git clone -b $branch $repoWithToken $tempPath 2>> $logFile

    if (!(Test-Path $tempPath)) {
        Log "ERROR: Clone failed"
        exit 1
    }

    Log "Clone success"

    # 🔴 Prevent self-delete issues
    Log "Cleaning install folder..."
    Get-ChildItem $installPath -Exclude "Updater.ps1","*.exe.config" | ForEach-Object {
        try {
            Remove-Item $_.FullName -Recurse -Force -ErrorAction Stop
        } catch {
            Log "Failed to delete: $($_.FullName)"
        }
    }

    Log "Copying new files..."
    Copy-Item "$tempPath\*" $installPath -Recurse -Force

    Log "Cleanup temp..."
    Remove-Item $tempPath -Recurse -Force

    # Restart app
    $appExe = Get-ChildItem $installPath -Filter *.exe | Select-Object -First 1
    Log "Restarting: $($appExe.FullName)"
    Start-Process $appExe.FullName

    Log "==== UPDATE SUCCESS ===="
}
catch {
    Log "EXCEPTION: $_"
}

Read-Host "Press Enter to exit"