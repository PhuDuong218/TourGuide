CREATE database TourGuideDB;
-- 1. Quản lý người dùng (Admin, Chủ quán, Du khách)
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(100),
    Role NVARCHAR(20) DEFAULT 'user', -- 'admin', 'merchant', 'user'
    CreatedAt DATETIME2 DEFAULT GETDATE()
);

-- 2. Danh mục ngôn ngữ (vi, en, ja...)
CREATE TABLE Languages (
    LanguageCode NVARCHAR(10) PRIMARY KEY, -- Ví dụ: 'vi-VN', 'en-US'
    LanguageName NVARCHAR(100) NOT NULL
);

-- 3. Điểm quan tâm (POI) - Lưu tọa độ cố định để tính Haversine
CREATE TABLE POI (
    POIID INT IDENTITY(1,1) PRIMARY KEY,
    RestaurantName NVARCHAR(200),
    Latitude DECIMAL(9,6) NOT NULL, -- Bắt buộc để làm Geofencing
    Longitude DECIMAL(9,6) NOT NULL,
    Address NVARCHAR(255),
    Category NVARCHAR(100), 
    OwnerID INT,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_POI_User FOREIGN KEY (OwnerID) REFERENCES Users(UserID)
);

-- 4. QUAN TRỌNG: Nội dung thuyết minh cho TTS
-- Gộp Name, Description và kịch bản đọc vào đây
CREATE TABLE POI_Translations (
    TranslationID INT IDENTITY(1,1) PRIMARY KEY,
    POIID INT NOT NULL,
    LanguageCode NVARCHAR(10) NOT NULL,
    DisplayName NVARCHAR(200), -- Tên hiển thị theo ngôn ngữ
    ShortDescription NVARCHAR(500), -- Mô tả ngắn hiện trên màn hình
    NarrationText NVARCHAR(MAX) NOT NULL, -- Văn bản chi tiết để máy ĐỌC (TTS)
    
    CONSTRAINT FK_Translation_POI FOREIGN KEY (POIID) REFERENCES POI(POIID) ON DELETE CASCADE,
    CONSTRAINT FK_Translation_Language FOREIGN KEY (LanguageCode) REFERENCES Languages(LanguageCode)
);

-- 5. Mã QR kích hoạt (Sử dụng cho tính năng MVP)
CREATE TABLE QRCode (
    QRID INT IDENTITY(1,1) PRIMARY KEY,
    POIID INT NOT NULL,
    QRValue NVARCHAR(255) UNIQUE, -- Giá trị mã hóa trong QR
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_QR_POI FOREIGN KEY (POIID) REFERENCES POI(POIID) ON DELETE CASCADE
);

-- 6. Nhật ký sử dụng (Phục vụ Analytics & Heatmap)
CREATE TABLE VisitHistory (
    VisitID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NULL, -- Cho phép NULL nếu khách dùng ẩn danh
    POIID INT NOT NULL,
    VisitTime DATETIME2 DEFAULT GETDATE(),
    ScanMethod NVARCHAR(50), -- 'GPS_Trigger' hoặc 'QR_Scan'
    UserLat DECIMAL(9,6), -- Lưu tọa độ thực tế của khách lúc đó
    UserLon DECIMAL(9,6), -- Để làm bản đồ nhiệt (Heatmap)
    LanguageUsed NVARCHAR(10),

    CONSTRAINT FK_Visit_POI FOREIGN KEY (POIID) REFERENCES POI(POIID)
);

INSERT INTO Users (Username, Email, PasswordHash, FullName, Role)
VALUES
('admin', 'admin@gmail.com', '123456', N'Quản trị viên', 'admin'),
('owner1', 'owner1@gmail.com', '123456', N'Nguyễn Văn An', 'owner'),
('user1', 'user1@gmail.com', '123456', N'Trần Thị Bo', 'user'),
('user2', 'user2@gmail.com', '123456', N'Lê Văn Cung', 'user');

INSERT INTO Languages (LanguageCode, LanguageName)
VALUES
('vi', N'Tiếng Việt'),
('en', N'English'),
('ru', N'Russian'),
('ko', N'Korean'),
('ja', N'Japanese'),
('fr', N'French'),
('ch', N'China');

INSERT INTO POI (RestaurantName, Latitude, Longitude, Address, Category, OwnerID)
VALUES 
(N'Phở Hòa Pasteur', 10.7876, 106.6912, N'260C Pasteur, Quận 3', N'Phở', 2),
(N'Bún Bò Huế Đông Ba', 10.7795, 106.6953, N'110A Nguyễn Du, Quận 1', N'Bún bò', 2),
(N'Cơm Tấm Ba Ghiền', 10.7938, 106.6713, N'84 Đặng Văn Ngữ, Phú Nhuận', N'Cơm tấm', 2),
(N'Bánh Mì Huỳnh Hoa', 10.7719, 106.6912, N'26 Lê Thị Riêng, Quận 1', N'Bánh mì', 2);


INSERT INTO POI_Translations (POIID, LanguageCode, DisplayName, ShortDescription, NarrationText)
VALUES 
-- Phở Hòa (Tiếng Việt & Tiếng Anh)
(1, 'vi', N'Phở Hòa Pasteur', N'Quán phở lâu đời nhất Sài Gòn.', 
    N'Chào mừng bạn đến với Phở Hòa. Đây là quán phở có lịch sử hơn 50 năm. Nước dùng ở đây được ninh từ xương bò trong 12 giờ tạo nên hương vị đậm đà khó quên.'),
(1, 'en', 'Pho Hoa Pasteur', 'The oldest Pho restaurant in Saigon.', 
    'Welcome to Pho Hoa. This restaurant has over 50 years of history. The broth here is simmered from beef bones for 12 hours, creating an unforgettable rich flavor.'),

-- Bún Bò Huế (Tiếng Việt & Tiếng Pháp)
(2, 'vi', N'Bún Bò Huế Đông Ba', N'Hương vị miền Trung giữa lòng Sài Gòn.', 
    N'Bạn đang đứng trước quán bún bò Đông Ba. Đặc trưng ở đây là sợi bún to, nước dùng thơm mùi mắm ruốc và sả, ăn kèm với chả cua béo ngậy.'),
(2, 'fr', 'Soupe de nouilles de Hue', 'Saveur du Centre au coeur de Saigon.', 
    'Vous êtes devant le restaurant de nouilles Dong Ba. La particularité ici est les grosses nouilles, le bouillon parfumé à la citronnelle, servi avec du pâté de crabe.'),

-- Bánh Mì Huỳnh Hoa (Tiếng Việt & Tiếng Anh)
(4, 'vi', N'Bánh Mì Huỳnh Hoa', N'Ổ bánh mì đắt nhất nhưng đáng thử nhất.', 
    N'Đây là bánh mì Huỳnh Hoa. Một ổ bánh mì ở đây nặng gần nửa ký với rất nhiều tầng thịt nguội, pate gan và bơ tươi làm thủ công.'),
(4, 'en', 'Huynh Hoa Bread', 'The most expensive but worth trying banh mi.', 
    'This is Huynh Hoa bakery. A loaf of bread here weighs nearly half a kilogram with many layers of cold cuts, liver pate, and handmade fresh butter.');

INSERT INTO QRCode (POIID, QRValue)
VALUES 
(1, 'QR_PHOHOA_001'),
(2, 'QR_BUNBO_002'),
(3, 'QR_COMTAM_003'),
(4, 'QR_BANHMI_004');

INSERT INTO VisitHistory (UserID, POIID, ScanMethod, UserLat, UserLon, LanguageUsed)
VALUES 
(3, 1, 'GPS_Trigger', 10.7875, 106.6911, 'en'), -- Khách đến gần quán và app tự phát tiếng Anh
(3, 4, 'QR_Scan', 10.7718, 106.6913, 'en'),    -- Khách quét mã QR tại quán Bánh mì
(NULL, 2, 'GPS_Trigger', 10.7794, 106.6952, 'vi'); -- Khách ẩn danh dùng tiếng Việt 