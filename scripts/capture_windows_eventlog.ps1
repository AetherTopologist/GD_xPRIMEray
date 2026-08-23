param(
    [DateTime]$StartTime = (Get-Date).AddHours(-1),
    [DateTime]$EndTime = (Get-Date),
    [string]$OutputPath = "windows_eventlog.json",
    [string]$RunId = ""
)

$providers = 'WHEA-Logger|BugCheck|Kernel-Power|Display|AMD|amdwddmg|Application Error|Windows Error Reporting'
$events = @()
try {
    foreach ($log in @('System', 'Application')) {
        Get-WinEvent -FilterHashtable @{ LogName = $log; StartTime = $StartTime; EndTime = $EndTime } -ErrorAction Stop |
            Where-Object { $_.ProviderName -match $providers -or $_.LevelDisplayName -eq 'Error' } |
            ForEach-Object {
                $events += [ordered]@{
                    timestamp = $_.TimeCreated.ToUniversalTime().ToString('o')
                    log = $log; provider = $_.ProviderName; id = $_.Id
                    level = $_.LevelDisplayName; message = $_.Message
                    run_id = $RunId
                }
            }
    }
} catch {
    $events += [ordered]@{ timestamp = (Get-Date).ToUniversalTime().ToString('o'); event = 'EVENT_LOG_UNAVAILABLE'; reason = $_.Exception.Message; run_id = $RunId }
}
$events | ConvertTo-Json -Depth 5 | Set-Content -Encoding utf8 $OutputPath
