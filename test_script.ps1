$loginBody = @{
    email = "admin@stylo.com"
    password = "Admin123!"
} | ConvertTo-Json

$loginRes = Invoke-RestMethod -Uri "http://localhost:59129/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
$token = $loginRes.token
$headers = @{ Authorization = "Bearer $token" }
Write-Host ">>> LOGGED IN SUCCESSFULLY"

# 1. TEST CREATE PRODUCT WITH JSON ("size": "L")
$createBody = @{
    name = "Refactored Test Tee"
    description = "Testing size JSON"
    price = 500
    categoryId = 1
    gender = "Men"
    size = "L"
    stock = 25
    imageUrl = "https://res.cloudinary.com/demo/sample.jpg"
    imagePublicId = "stylo/products/sample_public_id"
} | ConvertTo-Json

$created = Invoke-RestMethod -Uri "http://localhost:59129/api/products" -Method Post -Headers $headers -Body $createBody -ContentType "application/json"
Write-Host ">>> CREATED PRODUCT RESPONSE:"
$created | ConvertTo-Json -Depth 5

# 2. TEST UPDATE PRODUCT WITH JSON ("size": "XL")
$updateBody = @{
    name = "Refactored Test Tee Updated"
    description = "Testing size update"
    price = 550
    categoryId = 1
    gender = "Men"
    size = "XL"
    stock = 40
    imageUrl = "https://res.cloudinary.com/demo/sample.jpg"
    imagePublicId = "stylo/products/sample_public_id"
} | ConvertTo-Json

$updated = Invoke-RestMethod -Uri "http://localhost:59129/api/products/$($created.id)" -Method Put -Headers $headers -Body $updateBody -ContentType "application/json"
Write-Host ">>> UPDATED PRODUCT RESPONSE:"
$updated | ConvertTo-Json -Depth 5

# 3. VERIFY WITH GET BY ID
$retrieved = Invoke-RestMethod -Uri "http://localhost:59129/api/products/$($created.id)" -Method Get
Write-Host ">>> RETRIEVED PRODUCT FROM GET BY ID:"
$retrieved | ConvertTo-Json -Depth 5
