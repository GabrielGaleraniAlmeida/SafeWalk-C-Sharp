param(
    [string]$BaseUrl = "http://localhost:5080"
)

$ErrorActionPreference = "Stop"
$evidenceDirectory = Join-Path $PSScriptRoot "..\docs\evidencias"
New-Item -ItemType Directory -Path $evidenceDirectory -Force | Out-Null

function Save-JsonEvidence {
    param(
        [string]$Name,
        [object]$Value
    )

    $path = Join-Path $evidenceDirectory $Name
    $json = ConvertTo-Json -InputObject $Value -Depth 12
    [System.IO.File]::WriteAllText($path, $json, [System.Text.UTF8Encoding]::new($false))
}

$health = Invoke-RestMethod -Method Get -Uri "$BaseUrl/health"
Save-JsonEvidence "health.json" $health

$openApi = Invoke-RestMethod -Method Get -Uri "$BaseUrl/swagger/v1/swagger.json"
Save-JsonEvidence "swagger.json" @{
    openApi = $openApi.openapi
    title = $openApi.info.title
    version = $openApi.info.version
    paths = @($openApi.paths.PSObject.Properties.Name | Sort-Object)
}

$reports = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/reports?latitude=-23.5505&longitude=-46.6333&radiusKm=1"
Save-JsonEvidence "reports.json" $reports

$reportRequest = @{
    category = 4
    description = "Calçada com desnível (relato demonstrativo da API)."
    latitude = -23.5512
    longitude = -46.6333
} | ConvertTo-Json
$createdReport = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/reports" -ContentType "application/json" -Body $reportRequest
$confirmedReport = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/reports/$($createdReport.id)/confirm"
$resolvedReport = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/reports/$($createdReport.id)/resolve"
Save-JsonEvidence "report-workflow.json" @{
    created = $createdReport
    confirmed = $confirmedReport
    resolved = $resolvedReport
}

$routeRequest = @{
    routes = @(
        @{
            name = "Mais rápida"
            durationMinutes = 14
            segments = @(@{ latitude = -23.5509; longitude = -46.6342; simulatedCameraCoverage = $false })
        },
        @{
            name = "Mais segura"
            durationMinutes = 17
            segments = @(
                @{ latitude = -23.5488; longitude = -46.6350; simulatedCameraCoverage = $true },
                @{ latitude = -23.5480; longitude = -46.6320; simulatedCameraCoverage = $false }
            )
        }
    )
} | ConvertTo-Json -Depth 10
$routeComparison = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/routes/compare" -ContentType "application/json" -Body $routeRequest
Save-JsonEvidence "route-comparison.json" $routeComparison

$journeyRequest = @{ latitude = -23.5505; longitude = -46.6333 } | ConvertTo-Json
$journey = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/journeys" -ContentType "application/json" -Body $journeyRequest
$locationRequest = @{
    trackerToken = $journey.trackerToken
    latitude = -23.5498
    longitude = -46.6326
} | ConvertTo-Json
Invoke-RestMethod -Method Put -Uri "$BaseUrl/api/journeys/$($journey.id)/location" -ContentType "application/json" -Body $locationRequest | Out-Null
$sharedLocation = Invoke-RestMethod -Method Get -Uri "$BaseUrl$($journey.shareUrl)"
$journeyEvidence = @{
    journeyId = "[redigido]"
    positionReturned = ($null -ne $sharedLocation.latitude -and $null -ne $sharedLocation.longitude)
    startedAtUtc = "[redigido]"
    expiresAtUtc = "[redigido]"
    sharedLocation = @{
        latitude = "[redigida]"
        longitude = "[redigida]"
        lastUpdatedAtUtc = "[redigido]"
        expiresAtUtc = "[redigido]"
    }
    secretsRedacted = $true
    coordinatesRedacted = $true
    timestampsRedacted = $true
}
Save-JsonEvidence "journey.json" $journeyEvidence
$completeRequest = @{ trackerToken = $journey.trackerToken } | ConvertTo-Json
Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/journeys/$($journey.id)/complete" -ContentType "application/json" -Body $completeRequest | Out-Null

$sosRequest = @{ latitude = -23.5505; longitude = -46.6333; pin = "4826" } | ConvertTo-Json
$alert = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/sos-alerts" -ContentType "application/json" -Body $sosRequest
$contactsStage = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/sos-alerts/$($alert.id)/advance"
$cityStage = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/sos-alerts/$($alert.id)/advance"
$cancelRequest = @{ pin = "4826" } | ConvertTo-Json
$canceledAlert = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/sos-alerts/$($alert.id)/cancel" -ContentType "application/json" -Body $cancelRequest
Save-JsonEvidence "sos.json" @{
    id = $alert.id
    stages = @($alert.status, $contactsStage.status, $cityStage.status, $canceledAlert.status)
    descriptions = @($alert.stageDescription, $contactsStage.stageDescription, $cityStage.stageDescription, $canceledAlert.stageDescription)
    pinRedacted = $true
}

$start = [DateTime]::UtcNow.AddDays(-30).ToString("o")
$end = [DateTime]::UtcNow.ToString("o")
$dashboard = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/dashboard/reports?startUtc=$([uri]::EscapeDataString($start))&endUtc=$([uri]::EscapeDataString($end))"
Save-JsonEvidence "dashboard.json" $dashboard

Write-Output "Evidências gravadas em $([System.IO.Path]::GetFullPath($evidenceDirectory))"
