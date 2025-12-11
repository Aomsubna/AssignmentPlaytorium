#!/usr/bin/env pwsh
Start-Sleep -Seconds 2

Write-Host "Testing API endpoint..."

$body = @{
    selectedProduct = @(
        @{
            product = @{
                name = "T-Shirt"
                category = "Clothing"
                sku = "SKU-TS"
                price = 500
            }
            quantity = 1
        }
    )
    campaignsSelected = @{
        selectedCampaignCoupon = @{
            campaignName = "Fix amount"
            category = "Coupon"
        }
        discountCoupon = "200"
        selectedCampaignOnTop = $null
        discountOnTopClassify = ""
        discountOnTopValue = ""
        selectedCampaignSeasonal = $null
        discountSeasonalClassify = ""
        discountSeasonalValue = ""
    }
    userInput = @{
        amount = 200
    }
} | ConvertTo-Json -Depth 10

Write-Host "Request JSON:"
Write-Host $body
Write-Host ""
Write-Host "Sending POST to http://localhost:5224/api/products/calculatetotalsum..."

try {
    $response = Invoke-WebRequest `
        -Uri "http://localhost:5224/api/products/calculatetotalsum" `
        -Method POST `
        -ContentType "application/json" `
        -Body $body

    Write-Host "Status: $($response.StatusCode)"
    Write-Host "Response:"
    $response.Content | ConvertFrom-Json | ConvertTo-Json -Depth 10
} catch {
    Write-Host "Error: $_"
}
