Đồ Án: Ứng Dụng Hướng Dẫn Du Lịch Ẩm Thực Vĩnh Khánh
# TourGuide System
Hệ thống hướng dẫn du lịch bao gồm:
* 📱 Mobile App (MAUI) – hiển thị bản đồ, POI, thuyết minh
* 🌐 WebCMS – quản lý địa điểm (POI)
* 🔌 API Server – cung cấp dữ liệu cho App

##  1. Yêu cầu hệ thống
Trước khi chạy, cần cài:
* Visual Studio 2022+
* .NET SDK 9.0
* Android Emulator hoặc thiết bị thật
* SQL Server / SQLite (tuỳ config server)

## 📁 2. Cấu trúc project
TourGuide/
│
├── TourGuideMauiApp   # Mobile App (MAUI)
├── TourGuideServer    # API Backend (.NET)
├── WebCMS             # Web quản lý (Admin)
└── TourGuideMauiApp.sln

##  3. Cách chạy hệ thống

### 🔹 Bước 1: Chạy API Server
1. Mở project:  TourGuideServer

2. Set làm **Startup Project**

3. Chạy:  Ctrl + F5

4. Kiểm tra API:
https://localhost:xxxx/api/poi

 Nếu ra JSON → OK

### 🔹 Bước 2: Chạy WebCMS

1. Mở project:   WebCMS
2. Run project
3. Truy cập:
https://localhost:xxxx

 Dùng để:
* Thêm POI
* Sửa mô tả
* Upload ảnh
* Nhập nội dung thuyết minh

### 🔹 Bước 3: Cấu hình API cho App

Mở file:
TourGuideMauiApp/Services/DatabaseService.cs
Sửa URL:
string baseUrl = "http://YOUR_IP:PORT/api/"; //đổi  ip theo máy

| Trường hợp       | IP cần dùng              |
| ---------------- | ------------------------ |
| Emulator Android | 10.0.2.2                 |
| Máy thật         | IP LAN (vd: 192.168.x.x) |

### 🔹 Bước 4: Chạy Mobile App

1. Set:
TourGuideMauiApp
làm **Startup Project**

2. Chọn:
Android Emulator / Device

3. Run:   F5

##  4. Tính năng chính

###  Bản đồ

* Hiển thị bản đồ (OpenStreetMap)
* Load danh sách POI từ server
* Hiển thị marker

###  POI (Point of Interest)

* Click → hiện thông tin
* Hiện mô tả + ảnh
* Có nút phát thuyết minh

###  Text-to-Speech

* Đọc nội dung từ DB
* Ngôn ngữ: Tiếng Việt

###  GPS
* Lấy vị trí hiện tại
* Tự động zoom vào user

##  5. Lỗi thường gặp

###  Không hiện POI

* Kiểm tra API có chạy chưa
* Kiểm tra đúng IP chưa
* Kiểm tra dữ liệu DB

## Không gọi được API
* Sai port
* Chưa mở firewall
* Sai IP emulator

##  6. Ghi chú

* App sử dụng:

  * Mapsui (OpenStreetMap)
  * MAUI
  * SQLite / API

* Không cần Google Maps API key

##  Author

* Phát triển bởi: **Phú Dương**
* Project phục vụ đồ án học tập
