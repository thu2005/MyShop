# MyShop - Khởi động nhanh toàn bộ hệ thống
# Chạy script này để khởi động Database + Backend + App

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  MyShop - Quick Start" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$RootPath = $PSScriptRoot

# Bước 1: Khởi động Docker (PostgreSQL)
Write-Host "[1/3] Khởi động PostgreSQL (Docker)..." -ForegroundColor Yellow
$BackendPath = Join-Path $RootPath "src\MyShop.Backend"
$DockerComposePath = Join-Path $BackendPath "docker-compose.yml"

if (Test-Path $DockerComposePath) {
    docker-compose -f $DockerComposePath up -d
    if ($LASTEXITCODE -eq 0) {
        Write-Host "      PostgreSQL đã khởi động!" -ForegroundColor Green
    } else {
        Write-Host "      LỖI: Không thể khởi động Docker. Đảm bảo Docker Desktop đang chạy!" -ForegroundColor Red
        Read-Host "Nhấn Enter để thoát"
        exit 1
    }
} else {
    Write-Host "      CẢNH BÁO: Không tìm thấy docker-compose.yml" -ForegroundColor Yellow
}

Start-Sleep -Seconds 3

# Bước 2: Khởi động Backend
Write-Host "[2/3] Khởi động Backend (NestJS)..." -ForegroundColor Yellow

if (Test-Path $BackendPath) {
    # Mở terminal mới cho Backend
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$BackendPath'; npm run start:dev"
    Write-Host "      Backend đang khởi động trong terminal mới..." -ForegroundColor Green
    Write-Host "      Đợi khoảng 10-15 giây để backend sẵn sàng" -ForegroundColor Gray
} else {
    Write-Host "      LỖI: Không tìm thấy thư mục Backend!" -ForegroundColor Red
}

Start-Sleep -Seconds 5

# Bước 3: Thông báo
Write-Host ""
Write-Host "[3/3] Hoàn tất!" -ForegroundColor Green
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Hệ thống đã sẵn sàng!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Các dịch vụ đang chạy:" -ForegroundColor White
Write-Host "  - PostgreSQL: localhost:5432" -ForegroundColor Gray
Write-Host "  - Backend:    http://localhost:4000/graphql" -ForegroundColor Gray
Write-Host ""
Write-Host "Bạn có thể mở app MyShop từ Start Menu hoặc chạy từ Visual Studio" -ForegroundColor Yellow
Write-Host ""

# Hỏi có muốn mở app không
$openApp = Read-Host "Bạn có muốn mở app MyShop không? (Y/N)"
if ($openApp -eq "Y" -or $openApp -eq "y") {
    # Tìm và chạy app
    $appPath = Get-AppxPackage | Where-Object { $_.Name -like "*MyShop*" } | Select-Object -First 1
    if ($appPath) {
        Start-Process "shell:AppsFolder\$($appPath.PackageFamilyName)!App"
    } else {
        Write-Host "Không tìm thấy app đã cài. Hãy mở từ Visual Studio." -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "Để dừng hệ thống, chạy: docker-compose down" -ForegroundColor Gray
Read-Host "Nhấn Enter để đóng"
