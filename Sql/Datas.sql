CREATE DATABASE IF NOT EXISTS aliustadb;
USE aliustadb;

-- Kategori Tablosu
CREATE TABLE Kategori (
    ID INT AUTO_INCREMENT PRIMARY KEY,
    Adı VARCHAR(100)
);

-- Musteri Tablosu
CREATE TABLE Musteri (
    ID INT AUTO_INCREMENT PRIMARY KEY,
    Ad VARCHAR(100),
    Soyad VARCHAR(100),
    Telefon VARCHAR(20),
    EPosta VARCHAR(100)
);

-- Adres Tablosu
CREATE TABLE Adres (
    ID INT AUTO_INCREMENT PRIMARY KEY,
    Açıklama TEXT,
    MusteriID INT,
    FOREIGN KEY (MusteriID) REFERENCES Musteri(ID)
);

-- Ürün Tablosu
CREATE TABLE Urun (
    ID INT AUTO_INCREMENT PRIMARY KEY,
    Adı VARCHAR(100),
    Fiyat DECIMAL(10,2),
    Açıklama TEXT,
    KategoriID INT,
    FOREIGN KEY (KategoriID) REFERENCES Kategori(ID)
);

-- Malzeme Tablosu
CREATE TABLE Malzeme (
    ID INT AUTO_INCREMENT PRIMARY KEY,
    Adı VARCHAR(100),
    `Stok Miktarı` INT DEFAULT 0
);

-- Tedarikçi Tablosu
CREATE TABLE Tedarikçi (
    ID INT AUTO_INCREMENT PRIMARY KEY,
    Adı VARCHAR(100),
    Telefon VARCHAR(20),
    EPosta VARCHAR(100),
    MalzemeID INT,
    FOREIGN KEY (MalzemeID) REFERENCES Malzeme(ID)
);

-- Görev Tablosu
CREATE TABLE Görev (
    ID INT AUTO_INCREMENT PRIMARY KEY,
    Adı VARCHAR(100)
);

-- Personel Tablosu
CREATE TABLE Personel (
    ID INT AUTO_INCREMENT PRIMARY KEY,
    Ad VARCHAR(100),
    Soyad VARCHAR(100),
    Telefon VARCHAR(20),
    EPosta VARCHAR(100),
    Maaş DECIMAL(10,2),
    GörevID INT,
    FOREIGN KEY (GörevID) REFERENCES Görev(ID)
);

-- Admin Tablosu
CREATE TABLE Admin (
    ID INT AUTO_INCREMENT PRIMARY KEY,
    Adı VARCHAR(100) UNIQUE,
    Şifre VARCHAR(100)
);

-- UrunMalzeme Tablosu
CREATE TABLE UrunMalzeme (
    ID INT AUTO_INCREMENT PRIMARY KEY,
    UrunID INT,
    MalzemeID INT,
    Miktar INT,
    FOREIGN KEY (UrunID) REFERENCES Urun(ID),
    FOREIGN KEY (MalzemeID) REFERENCES Malzeme(ID)
);

-- Sipariş Tablosu
CREATE TABLE Sipariş (
    ID INT AUTO_INCREMENT PRIMARY KEY,
    MusteriID INT,
    UrunID INT,
    UstaID INT,
    KuryeID INT,
    Miktar INT,
    Fiyat DECIMAL(10,2),
    Tarih DATETIME,
    AdresID INT, 
    FOREIGN KEY (MusteriID) REFERENCES Musteri(ID),
    FOREIGN KEY (UrunID) REFERENCES Urun(ID),
    FOREIGN KEY (UstaID) REFERENCES Personel(ID),
    FOREIGN KEY (KuryeID) REFERENCES Personel(ID),
    FOREIGN KEY (AdresID) REFERENCES Adres(ID)
);

-- Yorum Tablosu
CREATE TABLE Yorum (
    ID INT AUTO_INCREMENT PRIMARY KEY,
    MusteriID INT,
    SiparişID INT,
    Puan INT,
    Açıklama TEXT,
    Tarih DATETIME,
    FOREIGN KEY (MusteriID) REFERENCES Musteri(ID),
    FOREIGN KEY (SiparişID) REFERENCES Sipariş(ID)
);

CREATE TABLE Alim (
    ID INT PRIMARY KEY AUTO_INCREMENT,
    TedarikciID INT,
    Tarih DATE,
    Miktar INT,
    MalzemeID INT,
    FOREIGN KEY (TedarikciID) REFERENCES Tedarikçi(ID),
    FOREIGN KEY (MalzemeID) REFERENCES Malzeme(ID)
);


-- Kategorileri ekle
INSERT INTO Kategori (Adı) VALUES 
('Lahmacun'), ('Pide'), ('Kebap'), ('Salata'), ('İçecek');

-- Görevleri ekle
INSERT INTO Görev (Adı) VALUES 
('Kurye'), ('Usta');

-- Personel ekle (5 kurye, 5 usta)
-- Kurye Personel IDs: 1-5
-- Usta Personel IDs: 6-10
INSERT INTO Personel (Ad, Soyad, Telefon, EPosta, Maaş, GörevID) VALUES
-- Kuryeler
('Ahmet', 'Koç', '5552222221', 'ahmet@aliusta.com', 9000, 1),
('Murat', 'Yıldız', '5552222222', 'murat@aliusta.com', 9200, 1),
('Hakan', 'Taş', '5552222223', 'hakan@aliusta.com', 8800, 1),
('Okan', 'Kara', '5552222224', 'okan@aliusta.com', 8500, 1),
('Cem', 'Su', '5552222225', 'cem@aliusta.com', 9100, 1),
-- Ustalar
('Mustafa', 'Usta', '5553333331', 'mustafa@aliusta.com', 12000, 2),
('İbrahim', 'Usta', '5553333332', 'ibrahim@aliusta.com', 12500, 2),
('Osman', 'Usta', '5553333333', 'osman@aliusta.com', 11800, 2),
('Hüseyin', 'Usta', '5553333334', 'huseyin@aliusta.com', 11500, 2),
('Kemal', 'Usta', '5553333335', 'kemal@aliusta.com', 12200, 2);

-- Malzemeleri ekle
INSERT INTO Malzeme (Adı, `Stok Miktarı`) VALUES
('Lahmacun Hamuru', 500), ('Kıyma', 300), ('Domates', 200), ('Biber', 150), ('Soğan', 180),
('Maydanoz', 100), ('Limon', 120), ('Nane', 80), ('Sumak', 50), ('Pide Hamuru', 400),
('Kuşbaşı Et', 250), ('Tavuk', 200), ('Yeşillik', 150), ('Ayran', 300), ('Kola', 250), ('Su', 500);

-- Tedarikçileri ekle
INSERT INTO Tedarikçi (Adı, Telefon, EPosta, MalzemeID) VALUES
('Hamur Tedarik', '5554440001', 'hamur@tedarik.com', 1),
('Et Ürünleri A.Ş.', '5554440002', 'et@tedarik.com', 2),
('Meyve Sebze Paz.', '5554440003', 'sebze@tedarik.com', 3),
('Meyve Sebze Paz.', '5554440004', 'sebze@tedarik.com', 4),
('Meyve Sebze Paz.', '5554440005', 'sebze@tedarik.com', 5),
('Baharatçı Hasan', '5554440006', 'baharat@tedarik.com', 9),
('Pide Hamuru Ltd.', '5554440007', 'pide@tedarik.com', 10),
('Et Ürünleri A.Ş.', '5554440008', 'et@tedarik.com', 11),
('Tavukçuluk Şti.', '5554440009', 'tavuk@tedarik.com', 12),
('İçecek Dağıtım', '5554440010', 'icecek@tedarik.com', 14),
('İçecek Dağıtım', '5554440011', 'icecek@tedarik.com', 15),
('Su Şirketi', '5554440012', 'su@tedarik.com', 16);

-- Ürünleri ekle
INSERT INTO Urun (Adı, Fiyat, Açıklama, KategoriID) VALUES
('Klasik Lahmacun', 45.00, 'Geleneksel lezzet', 1), ('Acılı Lahmacun', 50.00, 'Biber oranı yüksek', 1),
('Kaşarlı Lahmacun', 55.00, 'Üzeri kaşarlı', 1), ('Karışık Lahmacun', 60.00, 'Et ve tavuk karışık', 1),
('Kıymalı Pide', 80.00, 'Kıymalı geleneksel pide', 2), ('Kuşbaşılı Pide', 90.00, 'Kuşbaşı etli pide', 2),
('Karışık Pide', 100.00, 'Et ve tavuk karışık', 2), ('Kaşarlı Pide', 70.00, 'Sade kaşarlı pide', 2),
('Adana Kebap', 120.00, 'Acılı adana kebabı', 3), ('Urfa Kebap', 120.00, 'Acısız urfa kebabı', 3),
('Tavuk Şiş', 100.00, 'Tavuk şiş kebap', 3), ('Çoban Salata', 40.00, 'Geleneksel çoban salata', 4),
('Mevsim Salata', 45.00, 'Taze mevsim yeşillikleri', 4), ('Ayran', 15.00, 'Doğal ayran', 5),
('Kola', 20.00, 'Soğuk kola', 5), ('Su', 8.00, 'Doğal kaynak suyu', 5);

-- Ürün-Malzeme ilişkilerini ekle
INSERT INTO UrunMalzeme (UrunID, MalzemeID, Miktar) VALUES
(1, 1, 1), (1, 2, 1), (1, 3, 1), (1, 4, 1), (1, 5, 1), (1, 6, 1),
(2, 1, 1), (2, 2, 1), (2, 3, 1), (2, 4, 2), (2, 5, 1), (2, 6, 1),
(3, 1, 1), (3, 2, 1), (3, 3, 1), (3, 4, 1), (3, 5, 1), (3, 6, 1),
(5, 10, 1), (5, 2, 1), (5, 3, 1), (5, 4, 1), (5, 5, 1),
(6, 10, 1), (6, 11, 1), (6, 3, 1), (6, 4, 1), (6, 5, 1),
(9, 11, 1), (9, 4, 1), (9, 5, 1), (9, 7, 1),
(12, 3, 1), (12, 4, 1), (12, 5, 1), (12, 6, 1), (12, 7, 1),
(14, 14, 1), (15, 15, 1), (16, 16, 1);

-- Müşterileri ekle (25 Müşteri)
INSERT INTO Musteri (Ad, Soyad, Telefon, EPosta) VALUES
('Ahmet', 'Yılmaz', '5551234567', 'ahmet.yilmaz@email.com'), ('Mehmet', 'Kara', '5551234568', 'mehmet.kara@email.com'),
('Ayşe', 'Demir', '5551234569', 'ayse.demir@email.com'), ('Fatma', 'Şahin', '5551234570', 'fatma.sahin@email.com'),
('Zeynep', 'Arslan', '5551234571', 'zeynep.arslan@email.com'), ('Ali', 'Kaya', '5551234572', 'ali.kaya@email.com'),
('Veli', 'Taş', '5551234573', 'veli.tas@email.com'), ('Hasan', 'Yıldız', '5551234574', 'hasan.yildiz@email.com'),
('Hüseyin', 'Koç', '5551234575', 'huseyin.koc@email.com'), ('İbrahim', 'Su', '5551234576', 'ibrahim.su@email.com'),
('Osman', 'Ateş', '5551234577', 'osman.ates@email.com'), ('Kemal', 'Toprak', '5551234578', 'kemal.toprak@email.com'),
('Murat', 'Bulut', '5551234579', 'murat.bulut@email.com'), ('Okan', 'Deniz', '5551234580', 'okan.deniz@email.com'),
('Cem', 'Yılmaz', '5551234581', 'cem.yilmaz@email.com'), ('Selin', 'Akar', '5551234582', 'selin.akar@email.com'),
('Elif', 'Yücel', '5551234583', 'elif.yucel@email.com'), ('Deniz', 'Korkmaz', '5551234584', 'deniz.korkmaz@email.com'),
('Can', 'Sever', '5551234585', 'can.sever@email.com'), ('Burak', 'Güneş', '5551234586', 'burak.gunes@email.com'),
('Ece', 'Ay', '5551234587', 'ece.ay@email.com'), ('Gizem', 'Yıldırım', '5551234588', 'gizem.yildirim@email.com'),
('Kaan', 'Şimşek', '5551234589', 'kaan.simsek@email.com'), ('Lale', 'Aydın', '5551234590', 'lale.aydin@email.com'),
('Nur', 'Gök', '5551234591', 'nur.gok@email.com');

-- Adresleri ekle (30 adres, müşterilere atanmış şekilde)
INSERT INTO Adres (Açıklama, MusteriID) VALUES
('Atatürk Mahallesi, Karanfil Sokak No: 1 Daire: 2, Çankaya, Ankara', 1),
('Cumhuriyet Caddesi, Zambak Apartmanı No: 10, Şişli, İstanbul', 2),
('İnönü Bulvarı, Menekşe Sitesi A Blok No: 5, Konak, İzmir', 3),
('Fatih Sultan Mehmet Sokak No: 12 Daire: 8, Osmangazi, Bursa', 4),
('Mevlana Caddesi, Lale Apartmanı No: 33, Muratpaşa, Antalya', 5),
('Barbaros Hayrettin Paşa Mahallesi, Deniz Sokak No: 7, Kadıköy, İstanbul', 6),
('Yıldırım Beyazıt Caddesi No: 21, Nilüfer, Bursa', 7),
('Kazım Karabekir Paşa Sokak No: 4 Daire: 1, Keçiören, Ankara', 8),
('Adnan Menderes Bulvarı No: 55, Seyhan, Adana', 9),
('Turgut Özal Mahallesi, Papatya Sokak No: 19, Melikgazi, Kayseri', 10),
('Alparslan Türkeş Caddesi No: 8, Bornova, İzmir', 11),
('Necmettin Erbakan Sokak No: 14 Daire: 6, Pendik, İstanbul', 12),
('Mustafa Kemal Paşa Bulvarı No: 100, İlkadım, Samsun', 13),
('Süleyman Demirel Caddesi No: 2, Odunpazarı, Eskişehir', 14),
('Bülent Ecevit Mahallesi, Güneş Sokak No: 9, Tepebaşı, Eskişehir', 15),
('Deniz Gezmiş Sokak No: 16, Karşıyaka, İzmir', 16),
('Mahir Çayan Caddesi No: 22, Ataşehir, İstanbul', 17),
('İbrahim Kaypakkaya Bulvarı No: 3, Şehitkamil, Gaziantep', 18),
('Uğur Mumcu Sokak No: 11 Daire: 4, Yenimahalle, Ankara', 19),
('Ahmet Taner Kışlalı Caddesi No: 6, Çukurova, Adana', 20),
('Bahriye Üçok Mahallesi, Begonya Sokak No: 17, Maltepe, İstanbul', 21),
('Muammer Aksoy Caddesi No: 25, Buca, İzmir', 22),
('Çetin Emeç Bulvarı No: 7 Daire: 10, Balçova, İzmir', 23),
('Abdi İpekçi Sokak No: 13, Beşiktaş, İstanbul', 24),
('Hrant Dink Caddesi No: 1, Bakırköy, İstanbul', 25),
-- İlk 5 müşteriye ikinci adresler
('İstiklal Caddesi, Palmiye Apartmanı No: 40, Beyoğlu, İstanbul', 1), 
('Gazi Mustafa Kemal Bulvarı No: 60, Kızılay, Ankara', 2),         
('Talatpaşa Bulvarı, Çınar Sitesi B Blok No: 15, Alsancak, İzmir', 3), 
('Yeşilırmak Sokak No: 8 Daire: 5, Altındağ, Ankara', 4),          
('Akdeniz Caddesi, Portakal Çiçeği Apartmanı No: 23, Konyaaltı, Antalya', 5);

-- 100 sipariş ekle (AdresID'ler müşteriye ait olacak şekilde düzeltildi)
INSERT INTO Sipariş (MusteriID, UrunID, UstaID, KuryeID, Miktar, Fiyat, Tarih, AdresID)
SELECT
    gd.MusteriID_final,
    gd.UrunID_uretilen,
    gd.UstaID_uretilen,
    gd.KuryeID_uretilen,
    gd.Miktar_uretilen,  
    (SELECT Fiyat FROM Urun WHERE ID = gd.UrunID_uretilen) * gd.Miktar_uretilen AS Fiyat_hesaplanan, 
    gd.Tarih_uretilen,
    gd.AdresID_final
FROM
    (SELECT
        m.ID AS MusteriID_final,
        -- Her müşteri için rastgele bir adresini seç
        (SELECT a.ID FROM Adres a WHERE a.MusteriID = m.ID ORDER BY RAND() LIMIT 1) AS AdresID_final,
        FLOOR(1 + RAND() * 16) AS UrunID_uretilen,     -- Ürün IDs 1-16
        (6 + FLOOR(RAND() * 5)) AS UstaID_uretilen,    -- Usta (Personel IDs 6-10)
        (1 + FLOOR(RAND() * 5)) AS KuryeID_uretilen,   -- Kurye (Personel IDs 1-5)
        FLOOR(1 + RAND() * 5) AS Miktar_uretilen,     
        DATE_ADD('2024-01-01', INTERVAL FLOOR(RAND() * 365) DAY) +
         INTERVAL FLOOR(RAND() * 24) HOUR +
         INTERVAL FLOOR(RAND() * 60) MINUTE AS Tarih_uretilen
    FROM
        -- Rastgele 100 müşteri seç (tekrar edebilirler)
        -- Bu kısım, 100 sipariş oluşturmak için bir numara üreteci ile birleştirilmiş müşteri tablosunu kullanır.
        -- Her bir "numbers_generator" satırı için rastgele bir müşteri seçilir.
        (
            SELECT ID FROM Musteri ORDER BY RAND() LIMIT 100 -- Eğer 100'den az müşteri varsa, bu müşterilerin bazıları birden fazla sipariş verecektir.
                                                            -- Daha fazla çeşitlilik için, bu join'i ve aşağıdaki limit'i dikkatlice ayarlayın.
        ) m 
        -- 100 satır oluşturmak için bir numara üreteci (bu, her müşteri için birden fazla sipariş oluşturmaya yardımcı olur)
        -- VEYA basitçe 100 kez müşteri seçmek için
        JOIN (
            SELECT 1 AS n UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5 UNION ALL
            SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9 UNION ALL SELECT 10 UNION ALL
            SELECT 11 UNION ALL SELECT 12 UNION ALL SELECT 13 UNION ALL SELECT 14 UNION ALL SELECT 15 UNION ALL
            SELECT 16 UNION ALL SELECT 17 UNION ALL SELECT 18 UNION ALL SELECT 19 UNION ALL SELECT 20 UNION ALL
            SELECT 21 UNION ALL SELECT 22 UNION ALL SELECT 23 UNION ALL SELECT 24 UNION ALL SELECT 25 UNION ALL
            SELECT 26 UNION ALL SELECT 27 UNION ALL SELECT 28 UNION ALL SELECT 29 UNION ALL SELECT 30 UNION ALL
            SELECT 31 UNION ALL SELECT 32 UNION ALL SELECT 33 UNION ALL SELECT 34 UNION ALL SELECT 35 UNION ALL
            SELECT 36 UNION ALL SELECT 37 UNION ALL SELECT 38 UNION ALL SELECT 39 UNION ALL SELECT 40 UNION ALL
            SELECT 41 UNION ALL SELECT 42 UNION ALL SELECT 43 UNION ALL SELECT 44 UNION ALL SELECT 45 UNION ALL
            SELECT 46 UNION ALL SELECT 47 UNION ALL SELECT 48 UNION ALL SELECT 49 UNION ALL SELECT 50 UNION ALL
            SELECT 51 UNION ALL SELECT 52 UNION ALL SELECT 53 UNION ALL SELECT 54 UNION ALL SELECT 55 UNION ALL
            SELECT 56 UNION ALL SELECT 57 UNION ALL SELECT 58 UNION ALL SELECT 59 UNION ALL SELECT 60 UNION ALL
            SELECT 61 UNION ALL SELECT 62 UNION ALL SELECT 63 UNION ALL SELECT 64 UNION ALL SELECT 65 UNION ALL
            SELECT 66 UNION ALL SELECT 67 UNION ALL SELECT 68 UNION ALL SELECT 69 UNION ALL SELECT 70 UNION ALL
            SELECT 71 UNION ALL SELECT 72 UNION ALL SELECT 73 UNION ALL SELECT 74 UNION ALL SELECT 75 UNION ALL
            SELECT 76 UNION ALL SELECT 77 UNION ALL SELECT 78 UNION ALL SELECT 79 UNION ALL SELECT 80 UNION ALL
            SELECT 81 UNION ALL SELECT 82 UNION ALL SELECT 83 UNION ALL SELECT 84 UNION ALL SELECT 85 UNION ALL
            SELECT 86 UNION ALL SELECT 87 UNION ALL SELECT 88 UNION ALL SELECT 89 UNION ALL SELECT 90 UNION ALL
            SELECT 91 UNION ALL SELECT 92 UNION ALL SELECT 93 UNION ALL SELECT 94 UNION ALL SELECT 95 UNION ALL
            SELECT 96 UNION ALL SELECT 97 UNION ALL SELECT 98 UNION ALL SELECT 99 UNION ALL SELECT 100
        ) AS numbers_generator ON 1=1
        ORDER BY RAND() -- Sonuçları karıştırarak daha rastgele müşteri/adres çiftleri elde et
        LIMIT 100 -- Toplamda 100 sipariş oluştur
    ) AS gd;

-- Yorumları ekle (rastgele 50 yorum)
INSERT INTO Yorum (MusteriID, SiparişID, Puan, Açıklama, Tarih)
SELECT 
    s.MusteriID,
    s.ID,
    FLOOR(1 + RAND() * 5) AS Puan,
    CASE 
        WHEN FLOOR(1 + RAND() * 5) = 1 THEN 'Çok lezzetli, kesinlikle tavsiye ederim!'
        WHEN FLOOR(1 + RAND() * 5) = 2 THEN 'Güzel ama biraz geç geldi.'
        WHEN FLOOR(1 + RAND() * 5) = 3 THEN 'Harika lezzet, teşekkürler!'
        WHEN FLOOR(1 + RAND() * 5) = 4 THEN 'Orta karar, beklediğim gibi değildi.'
        ELSE 'Siparişim yanlış geldi, hayal kırıklığı...'
    END AS Açıklama,
    s.Tarih + INTERVAL FLOOR(1 + RAND() * 24) HOUR -- Yorum tarihi siparişten sonra
FROM 
    (SELECT * FROM Sipariş ORDER BY RAND() LIMIT 50) s; -- Yorum yapılacak 50 rastgele sipariş seç

-- Admin ekle
INSERT INTO Admin (Adı, Şifre) VALUES 
('AliUsta','123');

-- Alim ekle
INSERT INTO Alim (TedarikciID, Tarih, Miktar, MalzemeID) VALUES
(1, '2024-01-10', 100, 1),
(2, '2024-01-12', 50, 2),
(3, '2024-01-15', 75, 3),
(4, '2024-01-18', 60, 4),
(5, '2024-01-20', 80, 5),
(6, '2024-01-22', 20, 9),
(7, '2024-01-25', 90, 10),
(8, '2024-01-28', 40, 11),
(9, '2024-02-01', 70, 12),
(10, '2024-02-05', 120, 14),
(11, '2024-02-08', 100, 15),
(12, '2024-02-10', 200, 16),
(1, '2024-02-15', 150, 1),
(2, '2024-02-18', 60, 2),
(3, '2024-02-20', 80, 3),
(8, '2024-03-01', 55, 11),
(10, '2024-03-05', 150, 14);