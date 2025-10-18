#!/usr/bin/env pwsh
# Wait for all microservice endpoints to be healthy before running integration tests

$ErrorActionPreference = 'Stop'

$services = @(
    @{ Name = 'PlayerService'; Url = 'http://localhost:5001/swagger/index.html' },
    @{ Name = 'ShipService'; Url = 'http://localhost:5002/swagger/index.html' },
    @{ Name = 'QuestService'; Url = 'http://localhost:5003/swagger/index.html' },
    @{ Name = 'FactionService'; Url = 'http://localhost:5004/swagger/index.html' },
    @{ Name = 'EventService'; Url = 'http://localhost:5005/swagger/index.html' },
    @{ Name = 'ItemService'; Url = 'http://localhost:5006/swagger/index.html' },
    @{ Name = 'LocationService'; Url = 'http://localhost:5007/swagger/index.html' },
    @{ Name = 'ShopService'; Url = 'http://localhost:5002/swagger/index.html' }
)

$maxRetries = 30
$delaySeconds = 3

foreach ($service in $services) {
    $retries = 0
    $healthy = $false
    Write-Host "Waiting for $($service.Name) at $($service.Url) ..."
    while (-not $healthy -and $retries -lt $maxRetries) {
        try {
            $response = Invoke-WebRequest -Uri $service.Url -UseBasicParsing -TimeoutSec 3
            if ($response.StatusCode -eq 200) {
                $healthy = $true
                Write-Host "$($service.Name) is healthy."
            } else {
                throw "Status $($response.StatusCode)"
            }
        } catch {
            $retries++
            Start-Sleep -Seconds $delaySeconds
        }
    }
    if (-not $healthy) {
        Write-Host "$($service.Name) did not become healthy in time. Exiting."
        exit 1
    }
}
Write-Host "All services are healthy. Proceeding with integration tests."
exit 0
