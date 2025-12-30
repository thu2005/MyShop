# MyShop App Publish Script
# Chạy script này để đóng gói app: Right-click -> Run with PowerShell

param(
    [string]$Configuration = "Release",
    [string]$Platform = "x64"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  MyShop App - Publish Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Certificate Thumbprint (đã tạo sẵn)
$CertThumbprint = "6671E4AB4DD8E95F9C833B6484CC190862AB91F4"

# Đường dẫn project
$ProjectPath = "$PSScriptRoot\src\MyShop.App\MyShop.App.csproj"

# Kiểm tra project tồn tại
if (-not (Test-Path $ProjectPath)) {
    Write-Host "ERROR: Không tìm thấy project tại: $ProjectPath" -ForegroundColor Red
    Read-Host "Nhấn Enter để thoát"
    exit 1
}

Write-Host "[1/3] Đang clean project..." -ForegroundColor Yellow
dotnet clean $ProjectPath -c $Configuration -v q

Write-Host "[2/3] Đang build và tạo package..." -ForegroundColor Yellow
Write-Host "      Configuration: $Configuration" -ForegroundColor Gray
Write-Host "      Platform: $Platform" -ForegroundColor Gray

dotnet publish $ProjectPath `
    -c $Configuration `
    -p:Platform=$Platform `
    -p:GenerateAppxPackageOnBuild=true `
    -p:AppxPackageSigningEnabled=true `
    -p:PackageCertificateThumbprint=$CertThumbprint

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "[3/3] THÀNH CÔNG!" -ForegroundColor Green
    Write-Host ""
    
    # Tìm thư mục package mới nhất
    $AppPackagesPath = "$PSScriptRoot\src\MyShop.App\AppPackages"
    $LatestPackage = Get-ChildItem -Path $AppPackagesPath -Directory | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    
    if ($LatestPackage) {
        Write-Host "Package được tạo tại:" -ForegroundColor Cyan
        Write-Host "  $($LatestPackage.FullName)" -ForegroundColor White
        Write-Host ""
        Write-Host "Để cài đặt trên máy khác:" -ForegroundColor Yellow
        Write-Host "  1. Copy toàn bộ thư mục trên sang máy đích" -ForegroundColor Gray
        Write-Host "  2. Right-click Install.ps1 -> Run with PowerShell" -ForegroundColor Gray
        
        # Mở thư mục package
        explorer $LatestPackage.FullName
    }
} else {
    Write-Host ""
    Write-Host "LỖI: Build thất bại!" -ForegroundColor Red
}

Write-Host ""
Read-Host "Nhấn Enter để thoát"
