# Hướng Dẫn Cài Đặt MyShop

## Yêu Cầu Hệ Thống
- Windows 10 phiên bản 1809 (build 17763) trở lên
- Windows 11 (tất cả phiên bản)

---

## Cách Cài Đặt (Chỉ 2 bước)

### Bước 1: Mở thư mục cài đặt
Giải nén (nếu là file zip) và mở thư mục `MyShop.App_x.x.x.x_x64_Test`

### Bước 2: Chạy Script cài đặt
1. Tìm file **`Install.ps1`**
2. **Click chuột phải** vào file → Chọn **"Run with PowerShell"**
3. Nếu được hỏi về quyền, chọn **"Yes"** hoặc **"Đồng ý"**
4. Đợi script cài đặt hoàn tất

---

## Sau Khi Cài Đặt

- Ứng dụng **MyShop** sẽ xuất hiện trong **Start Menu**
- Nhấn phím **Windows** → Gõ **"MyShop"** → Enter để mở ứng dụng

---

## Xử Lý Lỗi Thường Gặp

### Lỗi 1: "Windows protected your PC"
**Giải pháp:** 
- Click **"More info"** 
- Click **"Run anyway"**

### Lỗi 2: "PowerShell is not recognized" 
**Giải pháp:** 
- Mở **Windows PowerShell** từ Start Menu
- Kéo thả file `Install.ps1` vào cửa sổ PowerShell
- Nhấn **Enter**

### Lỗi 3: Script không chạy được
**Giải pháp:**
1. Mở **Windows PowerShell** với quyền **Administrator**
2. Gõ lệnh sau và nhấn Enter:
   ```
   Set-ExecutionPolicy RemoteSigned -Scope CurrentUser
   ```
3. Chọn **Y** để xác nhận
4. Chạy lại file `Install.ps1`

---

## Cài Đặt Thủ Công (Nếu script không hoạt động)

### Bước 1: Cài Certificate
1. Mở file **`MyShop.App_x.x.x.x_x64.cer`** (double-click)
2. Click **"Install Certificate..."**
3. Chọn **"Local Machine"** → Next
4. Chọn **"Place all certificates in the following store"**
5. Click **"Browse..."** → Chọn **"Trusted Root Certification Authorities"** → OK
6. Next → Finish

### Bước 2: Cài Dependencies (nếu cần)
1. Mở thư mục **Dependencies\x64**
2. Double-click file **`Microsoft.WindowsAppRuntime.x.x.msix`**
3. Click **"Install"**

### Bước 3: Cài Ứng Dụng
1. Double-click file **`MyShop.App_x.x.x.x_x64.msix`**
2. Click **"Install"**

---

## Gỡ Cài Đặt

1. **Settings** → **Apps** → **Installed apps**
2. Tìm **"MyShop.App"**
3. Click **"..."** → **"Uninstall"**

---
