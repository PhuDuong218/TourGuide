-- ══════════════════════════════════════════════════════════
-- 10 POI · 4 ngôn ngữ (vi, en, fr, ja) · có ImageUrl
-- Chạy trên SQL Server Management Studio
-- ══════════════════════════════════════════════════════════

CREATE DATABASE TourGuideDB;
GO


-- ══════════════════════════════════════════════════════════
-- BẢNG 1: Users
-- ══════════════════════════════════════════════════════════
CREATE TABLE Users (
    UserID       NVARCHAR(10)  PRIMARY KEY,
    Username     NVARCHAR(50)  NOT NULL UNIQUE,
    Email        NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName     NVARCHAR(100),
    Role         NVARCHAR(20)  DEFAULT 'user', -- 'admin', 'owner', 'user'
    CreatedAt    DATETIME2     DEFAULT GETDATE()
);

-- ══════════════════════════════════════════════════════════
-- BẢNG 2: Languages
-- ══════════════════════════════════════════════════════════
CREATE TABLE Languages (
    LanguageCode NVARCHAR(10)  PRIMARY KEY,
    LanguageName NVARCHAR(100) NOT NULL
);

-- ══════════════════════════════════════════════════════════
-- BẢNG 3: POI 
-- ══════════════════════════════════════════════════════════
CREATE TABLE POI (
    POIID          NVARCHAR(10)  PRIMARY KEY,
    RestaurantName NVARCHAR(200),
    Latitude       DECIMAL(9,6)  NOT NULL,
    Longitude      DECIMAL(9,6)  NOT NULL,
    Address        NVARCHAR(255),
    Category       NVARCHAR(100),
    Img            NVARCHAR(255),          
    OwnerID        NVARCHAR(10),
    CreatedAt      DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_POI_User FOREIGN KEY (OwnerID) REFERENCES Users(UserID)
);

-- ══════════════════════════════════════════════════════════
-- BẢNG 4: POI_Translations
-- ══════════════════════════════════════════════════════════
CREATE TABLE POI_Translations (
    TranslationID    NVARCHAR(10)  PRIMARY KEY,
    POIID            NVARCHAR(10)  NOT NULL,
    LanguageCode     NVARCHAR(10)  NOT NULL,
    DisplayName      NVARCHAR(200),
    ShortDescription NVARCHAR(500),
    NarrationText    NVARCHAR(MAX) NOT NULL,
    CONSTRAINT FK_Translation_POI      FOREIGN KEY (POIID)         REFERENCES POI(POIID)             ON DELETE CASCADE,
    CONSTRAINT FK_Translation_Language FOREIGN KEY (LanguageCode)  REFERENCES Languages(LanguageCode)
);

-- ══════════════════════════════════════════════════════════
-- BẢNG 5: QRCode
-- ══════════════════════════════════════════════════════════
CREATE TABLE QRCode (
    QRID      NVARCHAR(10)  PRIMARY KEY,
    POIID     NVARCHAR(10)  NOT NULL,
    QRValue   NVARCHAR(255) UNIQUE,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_QR_POI FOREIGN KEY (POIID) REFERENCES POI(POIID) ON DELETE CASCADE
);

-- ══════════════════════════════════════════════════════════
-- BẢNG 6: VisitHistory
-- ══════════════════════════════════════════════════════════
CREATE TABLE VisitHistory (
    VisitID      NVARCHAR(10)  PRIMARY KEY,
    UserID       NVARCHAR(10)  NULL,
    POIID        NVARCHAR(10)  NOT NULL,
    VisitTime    DATETIME2    DEFAULT GETDATE(),
    ScanMethod   NVARCHAR(50),   -- 'GPS_Trigger' | 'QR_Scan'
    UserLat      DECIMAL(9,6),
    UserLon      DECIMAL(9,6),
    LanguageUsed NVARCHAR(10),
    CONSTRAINT FK_Visit_POI FOREIGN KEY (POIID) REFERENCES POI(POIID)
);
GO

-- ══════════════════════════════════════════════════════════
-- DỮ LIỆU: Users
-- ══════════════════════════════════════════════════════════
INSERT INTO Users (UserID, Username, Email, PasswordHash, FullName, Role) VALUES
('U001', 'admin',  'admin@gmail.com',  '123456', N'Quản trị viên',  'admin'),
('U002', 'owner1', 'owner1@gmail.com', '123456', N'Nguyễn Văn An',  'owner'),
('U003', 'user1',  'user1@gmail.com',  '123456', N'Trần Thị Bảo',   'user'),
('U004', 'user2',  'user2@gmail.com',  '123456', N'Lê Văn Cường',   'user');
GO
 
-- ══════════════════════════════════════════════════════════
-- DỮ LIỆU: Languages (4 ngôn ngữ)
-- ══════════════════════════════════════════════════════════
INSERT INTO Languages (LanguageCode, LanguageName) VALUES
('vi', N'Tiếng Việt'),
('en', N'English'),
('fr', N'Français'),
('ja', N'日本語');
GO
 
-- ══════════════════════════════════════════════════════════
-- DỮ LIỆU: 10 POI tại TP.HCM (có ImageUrl)
-- ══════════════════════════════════════════════════════════
INSERT INTO POI (POIID, RestaurantName, Latitude, Longitude, Address, Category, Img, OwnerID) VALUES
-- 1
('P001', N'Phở Hòa Pasteur',        10.7876, 106.6912, N'260C Pasteur, Quận 3, TP.HCM',             N'Phở',
 'PhoHoaPasteur.jpg', 'U002'),
-- 2
('P002', N'Bún Bò Huế Đông Ba',     10.7795, 106.6953, N'110A Nguyễn Du, Quận 1, TP.HCM',           N'Bún bò',
 'BunBoHueDongBa.jpg', 'U002'),
-- 3
('P003', N'Cơm Tấm Ba Ghiền',       10.7938, 106.6713, N'84 Đặng Văn Ngữ, Phú Nhuận, TP.HCM',      N'Cơm tấm',
 'ComTamBaGhien.jpg', 'U002'),
-- 4
('P004', N'Bánh Mì Huỳnh Hoa',      10.7719, 106.6912, N'26 Lê Thị Riêng, Quận 1, TP.HCM',         N'Bánh mì',
 'BanhMiHuynhHoa.jpg', 'U002'),
-- 5
('P005', N'Bến Thành Market',       10.7722, 106.6983, N'Quảng trường Quách Thị Trang, Quận 1',     N'Chợ',
 'ChoBenThanh.jpg', 'U002'),
-- 6
('P006', N'Nhà thờ Đức Bà',         10.7797, 106.6990, N'01 Công xã Paris, Quận 1, TP.HCM',         N'Di tích',
 'NhathoDucBa.jpg', 'U002'),
-- 7
('P007', N'Bưu điện Thành phố',     10.7796, 106.6992, N'02 Công xã Paris, Quận 1, TP.HCM',         N'Di tích',
 'BuudienThanhpho.jpg', 'U002'),
-- 8
('P008', N'Hủ Tiếu Nam Vang Sa Đéc', 10.7755, 106.6988, N'178 Nguyễn Thị Minh Khai, Quận 3',       N'Hủ tiếu',
 'HuTieuNamVangSaDec.jpg', 'U002'),
-- 9
('P009', N'Chợ Bình Tây',           10.7509, 106.6479, N'57A Thập Tam Trại, Quận 6, TP.HCM',        N'Chợ',
 'ChoBinhTay.jpg', 'U002'),
-- 10
('P010', N'Dinh Độc Lập',           10.7769, 106.6956, N'135 Nam Kỳ Khởi Nghĩa, Quận 1, TP.HCM',   N'Di tích',
 'DinhDocLap.jpg', 'U002');
GO
 
-- ══════════════════════════════════════════════════════════
-- DỮ LIỆU: POI_Translations — 4 ngôn ngữ × 10 POI
-- ══════════════════════════════════════════════════════════
 
-- ── POI 1: Phở Hòa Pasteur ──────────────────────────────
INSERT INTO POI_Translations (TranslationID, POIID, LanguageCode, DisplayName, ShortDescription, NarrationText) VALUES
('T001','P001','vi',N'Phở Hòa Pasteur',N'Quán phở lâu đời nhất Sài Gòn.',
 N'Chào mừng bạn đến với Phở Hòa Pasteur. Đây là quán phở có lịch sử hơn 50 năm tại Sài Gòn. Nước dùng được ninh từ xương bò trong 12 giờ, tạo nên hương vị đậm đà khó quên. Phở Hòa là điểm đến quen thuộc của người Sài Gòn và du khách quốc tế.'),
('T002','P001','en','Pho Hoa Pasteur','The oldest Pho restaurant in Saigon.',
 'Welcome to Pho Hoa Pasteur. This iconic restaurant has been serving authentic Vietnamese pho for over 50 years. The broth is simmered from beef bones for 12 hours, creating a rich and unforgettable flavor that has made it a landmark of Saigon cuisine.'),
('T003','P001','fr','Pho Hoa Pasteur','Le plus vieux restaurant de Pho à Saigon.',
 'Bienvenue au Pho Hoa Pasteur. Ce restaurant emblématique sert un pho authentique depuis plus de 50 ans. Le bouillon est mijoté à partir d''os de bœuf pendant 12 heures, créant une saveur riche et inoubliable.'),
('T004','P001','ja',N'フォー・ホア・パスター','サイゴン最古のフォーレストラン。',
 N'フォー・ホア・パスターへようこそ。このレストランは50年以上の歴史を持つサイゴンの名店です。牛骨を12時間煮込んだスープは深いコクと忘れられない味わいを生み出しています。');
 
-- ── POI 2: Bún Bò Huế Đông Ba ───────────────────────────
INSERT INTO POI_Translations (TranslationID, POIID, LanguageCode, DisplayName, ShortDescription, NarrationText) VALUES
('T005','P002','vi',N'Bún Bò Huế Đông Ba',N'Hương vị miền Trung giữa lòng Sài Gòn.',
 N'Bạn đang đứng trước quán Bún Bò Huế Đông Ba. Đặc trưng của món ăn ở đây là sợi bún to, nước dùng thơm mùi mắm ruốc và sả, ăn kèm với chả cua béo ngậy. Đây là hương vị miền Trung chính hiệu ngay giữa lòng thành phố.'),
('T006','P002','en','Bun Bo Hue Dong Ba','Central Vietnamese flavor in the heart of Saigon.',
 'You are standing in front of Bun Bo Hue Dong Ba. This restaurant specializes in the famous Hue-style beef noodle soup, featuring thick noodles in a fragrant lemongrass and shrimp paste broth, served with tender pork and crab paste.'),
('T007','P002','fr','Bun Bo Hue Dong Ba','Saveur du Centre vietnamien au cœur de Saigon.',
 'Vous êtes devant le restaurant Bun Bo Hue Dong Ba. Ce restaurant propose la fameuse soupe de nouilles de Hue, avec de grosses nouilles dans un bouillon parfumé à la citronnelle et à la pâte de crevettes, servie avec du porc et de la pâte de crabe.'),
('T008','P002','ja',N'ブンボーフエ・ドンバ',N'サイゴンの中心で味わう中部ベトナムの味。',
 N'ブンボーフエ・ドンバへようこそ。フエスタイルの牛肉ヌードルスープを提供するこのレストランは、レモングラスとエビペーストの香り豊かなスープが特徴です。豚肉とカニペーストと一緒にお楽しみください。');
 
-- ── POI 3: Cơm Tấm Ba Ghiền ─────────────────────────────
INSERT INTO POI_Translations (TranslationID, POIID, LanguageCode, DisplayName, ShortDescription, NarrationText) VALUES
('T009','P003','vi',N'Cơm Tấm Ba Ghiền',N'Cơm tấm sườn bì chả ngon nhất Sài Gòn.',
 N'Chào mừng bạn đến với Cơm Tấm Ba Ghiền tại Phú Nhuận. Đây là một trong những quán cơm tấm nổi tiếng nhất Sài Gòn. Cơm tấm ở đây được phục vụ với sườn nướng thơm lừng, bì giòn và chả trứng mềm mịn, rưới nước mắm pha chua ngọt đặc trưng.'),
('T010','P003','en','Com Tam Ba Ghien','The most famous broken rice restaurant in Saigon.',
 'Welcome to Com Tam Ba Ghien in Phu Nhuan. This is one of the most renowned broken rice restaurants in Saigon. The dish features fragrant grilled pork ribs, crispy shredded pork skin, and silky steamed egg meatloaf, served with sweet fish sauce dressing.'),
('T011','P003','fr','Com Tam Ba Ghien','Le riz brisé le plus célèbre de Saigon.',
 'Bienvenue au Com Tam Ba Ghien à Phu Nhuan. C''est l''un des restaurants de riz brisé les plus réputés de Saigon. Le plat comprend des côtes de porc grillées parfumées, de la peau de porc croustillante et un pâté aux œufs soyeux, servi avec une sauce de poisson aigre-douce.'),
('T012','P003','ja',N'コムタム・バーギエン',N'サイゴンで最も有名な砕き米レストラン。',
 N'フーニュアンのコムタム・バーギエンへようこそ。ここはサイゴンで最も有名な砕き米レストランの一つです。香ばしいグリルポークリブ、カリカリの豚皮、滑らかな卵肉ローフを甘酸っぱいフィッシュソースと一緒にお楽しみください。');
 
-- ── POI 4: Bánh Mì Huỳnh Hoa ────────────────────────────
INSERT INTO POI_Translations (TranslationID, POIID, LanguageCode, DisplayName, ShortDescription, NarrationText) VALUES
('T013','P004','vi',N'Bánh Mì Huỳnh Hoa',N'Ổ bánh mì đắt nhất nhưng đáng thử nhất.',
 N'Đây là Bánh Mì Huỳnh Hoa, một trong những tiệm bánh mì nổi tiếng nhất Sài Gòn. Một ổ bánh mì ở đây nặng gần nửa ký với rất nhiều tầng thịt nguội, pate gan và bơ tươi làm thủ công. Dù giá cao hơn bình thường nhưng hàng khách vẫn xếp hàng dài mỗi ngày.'),
('T014','P004','en','Huynh Hoa Bakery','The most expensive but worth every bite banh mi.',
 'This is Huynh Hoa Bakery, one of the most famous banh mi shops in Saigon. A single loaf weighs nearly half a kilogram, packed with multiple layers of cold cuts, liver pate, and handcrafted fresh butter. Despite the higher price, customers queue daily for this legendary sandwich.'),
('T015','P004','fr','Boulangerie Huynh Hoa','Le banh mi le plus cher mais le plus savoureux.',
 'Voici la Boulangerie Huynh Hoa, l''une des plus célèbres boutiques de banh mi à Saigon. Un seul pain pèse près d''un demi-kilogramme, rempli de plusieurs couches de charcuterie, de pâté de foie et de beurre frais artisanal. Malgré le prix plus élevé, les clients font la queue chaque jour.'),
('T016','P004','ja',N'フィン・ホア・ベーカリー',N'最も高価だが最も価値のあるバインミー。',
 N'ここはフィン・ホア・ベーカリーで、サイゴンで最も有名なバインミーショップの一つです。一本のパンは約半キログラムもあり、コールドカット、レバーパテ、手作りフレッシュバターが何層にも重なっています。価格は高めですが、毎日行列が絶えません。');
 
-- ── POI 5: Bến Thành Market ─────────────────────────────
INSERT INTO POI_Translations (TranslationID, POIID, LanguageCode, DisplayName, ShortDescription, NarrationText) VALUES
('T017','P005','vi',N'Chợ Bến Thành',N'Biểu tượng trung tâm của thành phố Hồ Chí Minh.',
 N'Chào mừng bạn đến với Chợ Bến Thành, biểu tượng nổi tiếng nhất của Thành phố Hồ Chí Minh. Được xây dựng từ năm 1914, khu chợ này là nơi giao thoa của văn hóa, ẩm thực và mua sắm. Bạn có thể tìm thấy mọi thứ từ đặc sản địa phương đến hàng thủ công mỹ nghệ tại đây.'),
('T018','P005','en','Ben Thanh Market','The most iconic symbol of Ho Chi Minh City.',
 'Welcome to Ben Thanh Market, the most iconic landmark of Ho Chi Minh City. Built in 1914, this bustling market is a crossroads of culture, cuisine, and shopping. You can find everything from local specialties to handicrafts and souvenirs under one roof.'),
('T019','P005','fr','Marché Ben Thanh','Le symbole le plus emblématique de Hô Chi Minh-Ville.',
 'Bienvenue au Marché Ben Thanh, le monument le plus emblématique de Hô Chi Minh-Ville. Construit en 1914, ce marché animé est un carrefour de culture, de cuisine et de shopping. Vous pouvez tout y trouver, des spécialités locales aux artisanats et souvenirs.'),
('T020','P005','ja',N'ベンタイン市場',N'ホーチミン市最も象徴的なランドマーク。',
 N'ベンタイン市場へようこそ。1914年に建設されたこの活気ある市場は、ホーチミン市の最も象徴的なランドマークです。地元の特産品から手工芸品やお土産まで、あらゆるものが一箇所で見つかります。');
 
-- ── POI 6: Nhà thờ Đức Bà ───────────────────────────────
INSERT INTO POI_Translations (TranslationID, POIID, LanguageCode, DisplayName, ShortDescription, NarrationText) VALUES
('T021','P006','vi',N'Nhà thờ Đức Bà Sài Gòn',N'Công trình kiến trúc Pháp tiêu biểu tại trung tâm thành phố.',
 N'Bạn đang đứng trước Nhà thờ Đức Bà Sài Gòn, công trình kiến trúc Gothic nổi tiếng được người Pháp xây dựng từ năm 1863 đến 1880. Tòa nhà được xây dựng hoàn toàn bằng gạch nhập khẩu từ Marseille, Pháp. Đây là một trong những điểm du lịch hấp dẫn nhất tại trung tâm thành phố.'),
('T022','P006','en','Saigon Notre-Dame Cathedral','An iconic French Gothic cathedral in the city center.',
 'You are standing before the Saigon Notre-Dame Cathedral, a magnificent Gothic structure built by the French from 1863 to 1880. Constructed entirely with bricks imported from Marseille, France, this cathedral is one of the most visited tourist attractions in the city center.'),
('T023','P006','fr','Cathédrale Notre-Dame de Saïgon','Une cathédrale gothique française emblématique au centre-ville.',
 'Vous êtes devant la Cathédrale Notre-Dame de Saïgon, une magnifique structure gothique construite par les Français de 1863 à 1880. Entièrement construite avec des briques importées de Marseille, cette cathédrale est l''une des attractions touristiques les plus visitées du centre-ville.'),
('T024','P006','ja',N'サイゴン・ノートルダム大聖堂',N'市内中心部にあるフランス・ゴシック様式の大聖堂。',
 N'サイゴン・ノートルダム大聖堂へようこそ。1863年から1880年にかけてフランス人によって建設されたこの壮大なゴシック様式の建物は、フランスのマルセイユからのレンガのみで建設されました。市内中心部で最も訪問者の多い観光スポットの一つです。');
 
-- ── POI 7: Bưu điện Thành phố ───────────────────────────
INSERT INTO POI_Translations (TranslationID, POIID, LanguageCode, DisplayName, ShortDescription, NarrationText) VALUES
('T025','P007','vi',N'Bưu điện Trung tâm Sài Gòn',N'Bưu điện đẹp nhất Đông Nam Á thời Pháp thuộc.',
 N'Chào mừng bạn đến với Bưu điện Trung tâm Sài Gòn, công trình được thiết kế bởi kiến trúc sư Gustave Eiffel và hoàn thành năm 1891. Bên trong, bạn sẽ bị ấn tượng bởi mái vòm cao, những ô cửa kính màu và bản đồ cổ của Nam Kỳ. Đây là một trong những công trình kiến trúc thuộc địa đẹp nhất còn sót lại.'),
('T026','P007','en','Saigon Central Post Office','The most beautiful post office in Southeast Asia.',
 'Welcome to the Saigon Central Post Office, designed by the renowned architect Gustave Eiffel and completed in 1891. Inside, you will be impressed by the high vaulted ceiling, stained glass windows, and antique maps of Cochinchina. It remains one of the most beautiful colonial buildings in Southeast Asia.'),
('T027','P007','fr','Bureau de Poste Central de Saigon','Le plus beau bureau de poste d''Asie du Sud-Est.',
 'Bienvenue au Bureau de Poste Central de Saigon, conçu par le célèbre architecte Gustave Eiffel et achevé en 1891. À l''intérieur, vous serez impressionné par le haut plafond voûté, les vitraux et les cartes antiques de la Cochinchine. C''est l''un des plus beaux bâtiments coloniaux d''Asie du Sud-Est.'),
('T028','P007','ja',N'サイゴン中央郵便局',N'東南アジアで最も美しい郵便局。',
 N'サイゴン中央郵便局へようこそ。有名な建築家ギュスターヴ・エッフェルが設計し、1891年に完成しました。内部では、高いアーチ型天井、ステンドグラスの窓、コーチシナの古地図に圧倒されることでしょう。東南アジアに残る最も美しいコロニアル建築の一つです。');
 
-- ── POI 8: Hủ Tiếu Nam Vang Sa Đéc ─────────────────────
INSERT INTO POI_Translations (TranslationID, POIID, LanguageCode, DisplayName, ShortDescription, NarrationText) VALUES
('T029','P008','vi',N'Hủ Tiếu Nam Vang Sa Đéc',N'Hủ tiếu gốc Campuchia nổi tiếng đất Sài Gòn.',
 N'Bạn đang đứng trước quán Hủ Tiếu Nam Vang Sa Đéc. Đây là món hủ tiếu theo phong cách Campuchia với nước dùng trong veo được nấu từ xương heo, tôm khô và mực khô. Bánh hủ tiếu mềm mịn ăn kèm thịt bằm, tôm tươi và trứng cút tạo nên hương vị độc đáo khó cưỡng.'),
('T030','P008','en','Sa Dec Nam Vang Noodle','Famous Cambodian-style noodle soup in Saigon.',
 'You are standing before Sa Dec Nam Vang Noodle restaurant. This eatery serves Cambodian-style noodle soup with a clear broth made from pork bones, dried shrimp, and dried squid. The soft noodles are served with minced pork, fresh shrimp, and quail eggs for a uniquely irresistible flavor.'),
('T031','P008','fr','Nouilles Nam Vang Sa Dec','Soupe de nouilles de style cambodgien célèbre à Saigon.',
 'Vous êtes devant le restaurant de Nouilles Nam Vang Sa Dec. Ce restaurant propose une soupe de nouilles de style cambodgien avec un bouillon clair à base d''os de porc, de crevettes séchées et de calmars séchés. Les nouilles moelleuses sont servies avec du porc haché, des crevettes fraîches et des œufs de caille.'),
('T032','P008','ja',N'サーデック・ナムヴァン麺',N'サイゴンで有名なカンボジアスタイルの麺料理。',
 N'サーデック・ナムヴァン麺へようこそ。豚骨、干しエビ、干しイカから作る澄んだスープが特徴のカンボジアスタイルの麺料理を提供するレストランです。柔らかい麺に挽き肉、新鮮なエビ、ウズラの卵を合わせた独特の味わいがたまりません。');
 
-- ── POI 9: Chợ Bình Tây ─────────────────────────────────
INSERT INTO POI_Translations (TranslationID, POIID, LanguageCode, DisplayName, ShortDescription, NarrationText) VALUES
('T033','P009','vi',N'Chợ Bình Tây',N'Chợ lớn nhất và cổ kính nhất tại khu Chợ Lớn.',
 N'Chào mừng bạn đến với Chợ Bình Tây, còn được gọi là Chợ Lớn. Được xây dựng năm 1928 bởi thương nhân người Hoa Quách Đàm, đây là khu chợ đầu mối lớn nhất thành phố chuyên buôn bán hàng sỉ. Kiến trúc độc đáo pha trộn Trung Hoa và Pháp là điểm đặc trưng của ngôi chợ lịch sử này.'),
('T034','P009','en','Binh Tay Market','The largest and most historic market in Cholon district.',
 'Welcome to Binh Tay Market, also known as the Great Market. Built in 1928 by Chinese merchant Quach Dam, this is the city''s largest wholesale market. Its unique architecture blending Chinese and French styles is a hallmark of this historic marketplace in the heart of Cholon.'),
('T035','P009','fr','Marché Binh Tay','Le plus grand et le plus historique marché du quartier de Cholon.',
 'Bienvenue au Marché Binh Tay, également connu sous le nom de Grand Marché. Construit en 1928 par le marchand chinois Quach Dam, c''est le plus grand marché de gros de la ville. Son architecture unique mêlant styles chinois et français est la marque de ce marché historique.'),
('T036','P009','ja',N'ビンタイ市場',N'チョロン地区最大の歴史ある市場。',
 N'ビンタイ市場へようこそ。グレートマーケットとも呼ばれるこの市場は、1928年に中国人商人クワック・ダムによって建設されました。中国とフランスのスタイルが融合したユニークな建築が特徴の、チョロン地区最大の卸売市場です。');
 
-- ── POI 10: Dinh Độc Lập ────────────────────────────────
INSERT INTO POI_Translations (TranslationID, POIID, LanguageCode, DisplayName, ShortDescription, NarrationText) VALUES
('T037','P010','vi',N'Dinh Độc Lập',N'Công trình lịch sử gắn liền với thống nhất đất nước.',
 N'Bạn đang đứng trước Dinh Độc Lập, còn gọi là Hội trường Thống Nhất. Đây là nơi diễn ra sự kiện lịch sử ngày 30 tháng 4 năm 1975, đánh dấu sự thống nhất của Việt Nam. Tòa nhà được thiết kế bởi kiến trúc sư Ngô Viết Thụ và hoàn thành năm 1966. Bên trong lưu giữ nhiều hiện vật lịch sử quý giá.'),
('T038','P010','en','Independence Palace','A historic landmark tied to Vietnamese reunification.',
 'You are standing before Independence Palace, also known as Reunification Hall. This is where the historic event of April 30, 1975 took place, marking the reunification of Vietnam. Designed by architect Ngo Viet Thu and completed in 1966, the building preserves many valuable historical artifacts inside.'),
('T039','P010','fr','Palais de l''Indépendance','Un monument historique lié à la réunification du Vietnam.',
 'Vous êtes devant le Palais de l''Indépendance, également connu sous le nom de Salle de la Réunification. C''est ici que s''est déroulé l''événement historique du 30 avril 1975, marquant la réunification du Vietnam. Conçu par l''architecte Ngo Viet Thu et achevé en 1966, le bâtiment conserve de nombreux artefacts historiques précieux.'),
('T040','P010','ja',N'独立宮殿',N'ベトナム統一に関わる歴史的ランドマーク。',
 N'独立宮殿へようこそ。統一会堂とも呼ばれるこの場所は、1975年4月30日の歴史的な出来事が起きた場所で、ベトナム統一を象徴しています。建築家ゴー・ヴィエット・トゥーが設計し1966年に完成したこの建物には、多くの貴重な歴史的遺物が保存されています。');
GO
 
-- ══════════════════════════════════════════════════════════
-- DỮ LIỆU: QRCode — mỗi POI 1 mã
-- ══════════════════════════════════════════════════════════
INSERT INTO QRCode (QRID, POIID, QRValue) VALUES
('Q001', 'P001', 'QR_PHOHOA_001'),
('Q002', 'P002', 'QR_BUNBO_002'),
('Q003', 'P003', 'QR_COMTAM_003'),
('Q004', 'P004', 'QR_BANHMI_004'),
('Q005', 'P005', 'QR_BENTHANH_005'),
('Q006', 'P006', 'QR_NHATHO_006'),
('Q007', 'P007', 'QR_BUUDIEN_007'),
('Q008', 'P008', 'QR_HUTIEU_008'),
('Q009', 'P009', 'QR_BINHTAY_009'),
('Q010', 'P010', 'QR_DINHDIENLAP_010');
GO
 
-- ══════════════════════════════════════════════════════════
-- DỮ LIỆU: VisitHistory — dữ liệu test
-- ══════════════════════════════════════════════════════════
INSERT INTO VisitHistory (VisitID, UserID, POIID, ScanMethod, UserLat, UserLon, LanguageUsed) VALUES
('VH001', 'U003', 'P001', 'GPS_Trigger', 10.7875, 106.6911, 'vi'),
('VH002', 'U003', 'P004', 'QR_Scan',     10.7718, 106.6913, 'en'),
('VH003', NULL,   'P002', 'GPS_Trigger', 10.7794, 106.6952, 'vi'),
('VH004', 'U004', 'P006', 'QR_Scan',     10.7796, 106.6989, 'fr'),
('VH005', NULL,   'P005', 'GPS_Trigger', 10.7721, 106.6982, 'ja'),
('VH006', 'U003', 'P010', 'QR_Scan',     10.7768, 106.6955, 'vi');


INSERT INTO Users (UserID, Username, Email, PasswordHash, FullName, Role) VALUES
('U005', 'owner2', 'owner2@gmail.com', '123456', N'Lê Văn Owner 2', 'owner');