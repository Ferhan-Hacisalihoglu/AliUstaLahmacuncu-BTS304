USE aliustadb;

DELIMITER //

CREATE PROCEDURE GetHomePageDashboardStats (
    IN p_OneYearAgo DATETIME
)
BEGIN
    SELECT
        (SELECT IFNULL(AVG(Puan), 0) FROM Yorum) AS AverageRating,
        (SELECT COUNT(*) FROM Urun) AS TotalProducts,
        (SELECT COUNT(*) FROM Personel) AS TotalStaff,
        (SELECT COUNT(DISTINCT MusteriID)
         FROM Sipariş
         WHERE Tarih >= p_OneYearAgo) AS ActiveCustomersLastYear;
END //

DELIMITER ;

-- 1. Stored Procedure to get admin password by username
DELIMITER //
CREATE PROCEDURE sp_GetAdminPasswordByUsername(
    IN p_Username VARCHAR(255)
)
BEGIN
    SELECT Şifre FROM Admin WHERE Adı = p_Username;
END //
DELIMITER ;

-- 2. Stored Procedure to check if an admin username exists
DELIMITER //
CREATE PROCEDURE sp_CheckAdminExists(
    IN p_Username VARCHAR(255)
)
BEGIN
    SELECT COUNT(*) FROM Admin WHERE Adı = p_Username;
END //
DELIMITER ;

-- 3. Stored Procedure to register a new admin
DELIMITER //
CREATE PROCEDURE sp_RegisterAdmin(
    IN p_Username VARCHAR(255),
    IN p_Password VARCHAR(255) -- Consider hashing passwords before storing!
)
BEGIN
    INSERT INTO Admin (Adı, Şifre) VALUES (p_Username, p_Password);
END //
DELIMITER ;

-- 4. Stored Procedure to change an admin's password
DELIMITER //
CREATE PROCEDURE sp_ChangeAdminPassword(
    IN p_Username VARCHAR(255),
    IN p_NewPassword VARCHAR(255) -- Consider hashing!
)
BEGIN
    UPDATE Admin SET Şifre = p_NewPassword WHERE Adı = p_Username;
END //
DELIMITER ;

-- Stored Procedure for GetMusterilerSelectList
DELIMITER //
CREATE PROCEDURE sp_GetMusterilerForSelectList()
BEGIN
    SELECT ID, CONCAT(Ad, ' ', Soyad) AS AdSoyad
    FROM Musteri
    ORDER BY Ad, Soyad;
END //
DELIMITER ;

-- Stored Procedure for Index (Get All Adresler with Musteri info)
DELIMITER //
CREATE PROCEDURE sp_GetAllAdreslerWithMusteri()
BEGIN
    SELECT a.ID, a.Açıklama, a.MusteriID, CONCAT(m.Ad, ' ', m.Soyad) AS MusteriAdiSoyadi
    FROM Adres a
    JOIN Musteri m ON a.MusteriID = m.ID
    ORDER BY m.Ad, m.Soyad, a.ID;
END //
DELIMITER ;

-- Stored Procedure for Create Adres
DELIMITER //
CREATE PROCEDURE sp_CreateAdres(
    IN p_Aciklama TEXT,
    IN p_MusteriID INT
)
BEGIN
    INSERT INTO Adres (Açıklama, MusteriID)
    VALUES (p_Aciklama, p_MusteriID);
END //
DELIMITER ;

-- Stored Procedure for Edit (GET - Get Adres by ID)
DELIMITER //
CREATE PROCEDURE sp_GetAdresById(
    IN p_ID INT
)
BEGIN
    SELECT ID, Açıklama, MusteriID
    FROM Adres
    WHERE ID = p_ID;
END //
DELIMITER ;

-- Stored Procedure for Edit (POST - Update Adres)
DELIMITER //
CREATE PROCEDURE sp_UpdateAdres(
    IN p_ID INT,
    IN p_Aciklama TEXT,
    IN p_MusteriID INT
)
BEGIN
    UPDATE Adres
    SET Açıklama = p_Aciklama, MusteriID = p_MusteriID
    WHERE ID = p_ID;
END //
DELIMITER ;

-- Stored Procedure for Delete (GET - Get Adres by ID with Musteri info for confirmation view)
DELIMITER //
CREATE PROCEDURE sp_GetAdresForDeleteViewById(
    IN p_ID INT
)
BEGIN
    SELECT a.ID, a.Açıklama, CONCAT(m.Ad, ' ', m.Soyad) AS MusteriAdiSoyadi
    FROM Adres a
    JOIN Musteri m ON a.MusteriID = m.ID
    WHERE a.ID = p_ID;
END //
DELIMITER ;

-- Stored Procedure for DeleteConfirmed (POST - Delete Adres by ID)
DELIMITER //
CREATE PROCEDURE sp_DeleteAdresById(
    IN p_ID INT
)
BEGIN
    DELETE FROM Adres
    WHERE ID = p_ID;
END //
DELIMITER ;

-- 1. To get all Alim records with details for the Index view
DELIMITER //
CREATE PROCEDURE sp_GetAllAlimlarWithDetails()
BEGIN
    SELECT a.ID, t.`Adı` AS TedarikciAdi, m.`Adı` AS MalzemeAdi, a.Miktar, a.Tarih, a.TedarikciID, a.MalzemeID
    FROM Alim a
    INNER JOIN Tedarikçi t ON a.TedarikciID = t.ID
    INNER JOIN Malzeme m ON a.MalzemeID = m.ID
    ORDER BY a.Tarih DESC, a.ID DESC;
END //
DELIMITER ;

-- 2. To get Tedarikciler for the dropdown list in Create view
DELIMITER //
CREATE PROCEDURE sp_GetTedarikcilerForSelectListWithMalzeme()
BEGIN
    SELECT T.ID, T.`Adı` AS TedarikciAdi, M.`Adı` AS MalzemeAdi, T.MalzemeID
    FROM Tedarikçi T
    INNER JOIN Malzeme M ON T.MalzemeID = M.ID
    ORDER BY T.`Adı`;
END //
DELIMITER ;

-- 3. To process Alim creation (includes getting MalzemeID, inserting Alim, updating Stok)
-- This SP will handle the transaction internally.
DELIMITER //
CREATE PROCEDURE sp_ProcessAlimCreation(
    IN p_TedarikciID INT,
    IN p_Miktar INT,
    IN p_Tarih DATETIME,
    OUT p_Success TINYINT(1),       -- 1 for success, 0 for failure
    OUT p_ErrorMessage VARCHAR(255) -- Error message if p_Success is 0
)
BEGIN
    DECLARE v_MalzemeID INT;
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SET p_Success = 0;
        SET p_ErrorMessage = 'Veritabanı işlemi sırasında beklenmedik bir hata oluştu.';
        -- SELECT 'SQLEXCEPTION occurred, rolling back' AS DebugMessage; -- For debugging
    END;

    SET p_Success = 0; -- Default to failure
    SET p_ErrorMessage = '';

    START TRANSACTION;

    -- Get MalzemeID from Tedarikçi table
    SELECT MalzemeID INTO v_MalzemeID FROM Tedarikçi WHERE ID = p_TedarikciID;

    IF v_MalzemeID IS NULL THEN
        SET p_ErrorMessage = 'Seçilen tedarikçi için malzeme ID bulunamadı. Lütfen tedarikçi ayarlarını kontrol edin.';
        ROLLBACK;
    ELSE
        -- Insert into Alim table
        INSERT INTO Alim (TedarikciID, MalzemeID, Miktar, Tarih)
        VALUES (p_TedarikciID, v_MalzemeID, p_Miktar, p_Tarih);

        -- Update Malzeme Stok
        UPDATE Malzeme SET Stok = Stok + p_Miktar WHERE ID = v_MalzemeID;

        IF ROW_COUNT() = 0 THEN
            -- This means the Malzeme record with v_MalzemeID was not found or not updated
            SET p_ErrorMessage = 'Malzeme stoğu güncellenemedi. İlgili malzeme kaydı bulunamadı.';
            ROLLBACK;
        ELSE
            COMMIT;
            SET p_Success = 1;
            SET p_ErrorMessage = 'Alım başarıyla eklendi ve malzeme stoğu güncellendi.';
        END IF;
    END IF;
END //
DELIMITER ;

-- 4. To get a single Alim record by ID with details (for Delete confirmation view)
DELIMITER //
CREATE PROCEDURE sp_GetAlimByIdWithDetails(
    IN p_AlimID INT
)
BEGIN
    SELECT a.ID, t.`Adı` AS TedarikciAdi, m.`Adı` AS MalzemeAdi, a.Miktar, a.Tarih, a.TedarikciID, a.MalzemeID
    FROM Alim a
    INNER JOIN Tedarikçi t ON a.TedarikciID = t.ID
    INNER JOIN Malzeme m ON a.MalzemeID = m.ID
    WHERE a.ID = p_AlimID;
END //
DELIMITER ;

-- 5. To process Alim deletion (includes getting Alim info, deleting Alim, reverting Stok)
-- This SP will handle the transaction internally.
DELIMITER //
CREATE PROCEDURE sp_ProcessAlimDeletion(
    IN p_AlimID INT,
    OUT p_Success TINYINT(1),       -- 1 for success, 0 for failure
    OUT p_ErrorMessage VARCHAR(255) -- Error message if p_Success is 0
)
BEGIN
    DECLARE v_Miktar INT;
    DECLARE v_MalzemeID INT;
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SET p_Success = 0;
        SET p_ErrorMessage = 'Veritabanı silme işlemi sırasında beklenmedik bir hata oluştu.';
    END;

    SET p_Success = 0; -- Default to failure
    SET p_ErrorMessage = '';

    START TRANSACTION;

    -- Get Miktar and MalzemeID from the Alim record to be deleted
    SELECT Miktar, MalzemeID INTO v_Miktar, v_MalzemeID FROM Alim WHERE ID = p_AlimID;

    IF v_Miktar IS NULL OR v_MalzemeID IS NULL THEN
        SET p_ErrorMessage = 'Silinecek alım kaydı bulunamadı.';
        ROLLBACK; -- Or COMMIT if you consider "not found" a success for deletion intent
    ELSE
        -- Delete from Alim table
        DELETE FROM Alim WHERE ID = p_AlimID;

        IF ROW_COUNT() = 0 THEN
             SET p_ErrorMessage = 'Alım kaydı silinemedi (muhtemelen zaten silinmiş veya bulunamadı).';
             ROLLBACK;
        ELSE
            -- Update Malzeme Stok (decrease by the amount of the deleted Alim)
            UPDATE Malzeme SET Stok = Stok - v_Miktar WHERE ID = v_MalzemeID;

            IF ROW_COUNT() = 0 THEN
                -- This is a problematic state: Alim deleted, but Stok not updated.
                -- The transaction will be rolled back by the EXIT HANDLER if this UPDATE fails due to SQL error.
                -- If it fails because MalzemeID is not found, this specific message is better.
                SET p_ErrorMessage = 'Alım kaydı silindi, ancak malzeme stoğu güncellenemedi (ilgili malzeme bulunamadı).';
                ROLLBACK;
            ELSE
                COMMIT;
                SET p_Success = 1;
                SET p_ErrorMessage = 'Alım başarıyla silindi ve malzeme stoğu güncellendi.';
            END IF;
        END IF;
    END IF;
END //
DELIMITER ;

-- 6. To get Malzeme details for a specific Tedarikci (for AJAX call)
DELIMITER //
CREATE PROCEDURE sp_GetMalzemeDetailsByTedarikciId(
    IN p_TedarikciID INT
)
BEGIN
    SELECT M.ID, M.`Adı` AS Adi -- Ensuring output column name 'Adi' matches MalzemeViewModel
    FROM Malzeme M
    INNER JOIN Tedarikçi T ON M.ID = T.MalzemeID
    WHERE T.ID = p_TedarikciID;
END //
DELIMITER ;

-- 1. Get all categories
DELIMITER //
CREATE PROCEDURE sp_GetAllKategoriler()
BEGIN
    SELECT ID, Adı FROM Kategori ORDER BY Adı;
END //
DELIMITER ;

-- 2. Create a new category
DELIMITER //
CREATE PROCEDURE sp_CreateKategori(
    IN p_Adi VARCHAR(255) -- Adjust size as per your Kategori.Adı column
)
BEGIN
    INSERT INTO Kategori (Adı) VALUES (p_Adi);
END //
DELIMITER ;

-- 3. Get a category by ID
DELIMITER //
CREATE PROCEDURE sp_GetKategoriById(
    IN p_ID INT
)
BEGIN
    SELECT ID, Adı FROM Kategori WHERE ID = p_ID;
END //
DELIMITER ;

-- 4. Update an existing category
DELIMITER //
CREATE PROCEDURE sp_UpdateKategori(
    IN p_ID INT,
    IN p_Adi VARCHAR(255) -- Adjust size
)
BEGIN
    UPDATE Kategori SET Adı = p_Adi WHERE ID = p_ID;
END //
DELIMITER ;

-- 5. Delete a category by ID
DELIMITER //
CREATE PROCEDURE sp_DeleteKategoriById(
    IN p_ID INT
)
BEGIN
    DELETE FROM Kategori WHERE ID = p_ID;
END //
DELIMITER ;

-- 1. Get all malzemeler with stok
DELIMITER //
CREATE PROCEDURE sp_GetAllMalzemeler()
BEGIN
    SELECT ID, Adı, `Stok Miktarı`
    FROM Malzeme
    ORDER BY Adı;
END //
DELIMITER ;

-- 2. Create a new malzeme
-- Stok Miktarı is often initialized to 0 or handled by other processes (like Alim)
DELIMITER //
CREATE PROCEDURE sp_CreateMalzeme(
    IN p_Adi VARCHAR(255) -- Adjust size as per your Malzeme.Adı column
)
BEGIN
    INSERT INTO Malzeme (Adı) VALUES (p_Adi);
    -- If you want to initialize Stok Miktarı to 0 explicitly:
    -- INSERT INTO Malzeme (Adı, `Stok Miktarı`) VALUES (p_Adi, 0);
END //
DELIMITER ;

-- 3. Get a malzeme by ID (for Edit view, only Adı is needed as per current code)
DELIMITER //
CREATE PROCEDURE sp_GetMalzemeByIdForEdit(
    IN p_ID INT
)
BEGIN
    SELECT ID, Adı
    FROM Malzeme
    WHERE ID = p_ID;
END //
DELIMITER ;

-- 4. Update an existing malzeme's name
DELIMITER //
CREATE PROCEDURE sp_UpdateMalzemeAdi(
    IN p_ID INT,
    IN p_Adi VARCHAR(255) -- Adjust size
)
BEGIN
    UPDATE Malzeme SET Adı = p_Adi WHERE ID = p_ID;
END //
DELIMITER ;

-- 5. Get a malzeme by ID with stok (for Delete confirmation view)
DELIMITER //
CREATE PROCEDURE sp_GetMalzemeByIdForDelete(
    IN p_ID INT
)
BEGIN
    SELECT ID, Adı, `Stok Miktarı`
    FROM Malzeme
    WHERE ID = p_ID;
END //
DELIMITER ;

-- 6. Delete a malzeme by ID
DELIMITER //
CREATE PROCEDURE sp_DeleteMalzemeById(
    IN p_ID INT
)
BEGIN
    DELETE FROM Malzeme WHERE ID = p_ID;
END //
DELIMITER ;

-- 1. Get all musteriler
DELIMITER //
CREATE PROCEDURE sp_GetAllMusteriler()
BEGIN
    SELECT ID, Ad, Soyad, Telefon, EPosta
    FROM Musteri
    ORDER BY Soyad, Ad;
END //
DELIMITER ;

-- 2. Create a new musteri
DELIMITER //
CREATE PROCEDURE sp_CreateMusteri(
    IN p_Ad VARCHAR(100),
    IN p_Soyad VARCHAR(100),
    IN p_Telefon VARCHAR(20),  -- Or appropriate type/size
    IN p_EPosta VARCHAR(100) -- Or appropriate type/size
)
BEGIN
    INSERT INTO Musteri (Ad, Soyad, Telefon, EPosta)
    VALUES (p_Ad, p_Soyad, p_Telefon, p_EPosta);
END //
DELIMITER ;

-- 3. Get a musteri by ID (for Edit and Delete confirmation views)
DELIMITER //
CREATE PROCEDURE sp_GetMusteriById(
    IN p_ID INT
)
BEGIN
    SELECT ID, Ad, Soyad, Telefon, EPosta
    FROM Musteri
    WHERE ID = p_ID;
END //
DELIMITER ;

-- 4. Update an existing musteri
DELIMITER //
CREATE PROCEDURE sp_UpdateMusteri(
    IN p_ID INT,
    IN p_Ad VARCHAR(100),
    IN p_Soyad VARCHAR(100),
    IN p_Telefon VARCHAR(20),
    IN p_EPosta VARCHAR(100)
)
BEGIN
    UPDATE Musteri
    SET Ad = p_Ad, Soyad = p_Soyad, Telefon = p_Telefon, EPosta = p_EPosta
    WHERE ID = p_ID;
END //
DELIMITER ;

-- 5. Delete a musteri by ID
DELIMITER //
CREATE PROCEDURE sp_DeleteMusteriById(
    IN p_ID INT
)
BEGIN
    DELETE FROM Musteri WHERE ID = p_ID;
END //
DELIMITER ;

-- 1. Get all Görevler for SelectList
DELIMITER //
CREATE PROCEDURE sp_GetGorevlerForSelectList()
BEGIN
    SELECT ID, Adı FROM Görev ORDER BY Adı;
END //
DELIMITER ;

-- 2. Get all Personeller with Görev details
DELIMITER //
CREATE PROCEDURE sp_GetAllPersonellerWithGorev()
BEGIN
    SELECT p.ID, p.Ad, p.Soyad, p.Telefon, p.EPosta, p.Maaş, p.GörevID, g.Adı AS GorevAdi
    FROM Personel p
    LEFT JOIN Görev g ON p.GörevID = g.ID
    ORDER BY p.Soyad, p.Ad;
END //
DELIMITER ;

-- 3. Create a new Personel
DELIMITER //
CREATE PROCEDURE sp_CreatePersonel(
    IN p_Ad VARCHAR(100),
    IN p_Soyad VARCHAR(100),
    IN p_Telefon VARCHAR(20),
    IN p_EPosta VARCHAR(100),
    IN p_Maas DECIMAL(10,2), -- Adjust precision and scale as needed
    IN p_GorevID INT
)
BEGIN
    INSERT INTO Personel (Ad, Soyad, Telefon, EPosta, Maaş, GörevID)
    VALUES (p_Ad, p_Soyad, p_Telefon, p_EPosta, p_Maas, p_GorevID);
END //
DELIMITER ;

-- 4. Get Personel by ID (for Edit view, without GorevAdi as GorevlerListesi is populated separately)
DELIMITER //
CREATE PROCEDURE sp_GetPersonelByIdForEdit(
    IN p_ID INT
)
BEGIN
    SELECT ID, Ad, Soyad, Telefon, EPosta, Maaş, GörevID
    FROM Personel
    WHERE ID = p_ID;
END //
DELIMITER ;

-- 5. Update an existing Personel
DELIMITER //
CREATE PROCEDURE sp_UpdatePersonel(
    IN p_ID INT,
    IN p_Ad VARCHAR(100),
    IN p_Soyad VARCHAR(100),
    IN p_Telefon VARCHAR(20),
    IN p_EPosta VARCHAR(100),
    IN p_Maas DECIMAL(10,2),
    IN p_GorevID INT
)
BEGIN
    UPDATE Personel
    SET Ad = p_Ad, Soyad = p_Soyad, Telefon = p_Telefon, EPosta = p_EPosta, Maaş = p_Maas, GörevID = p_GorevID
    WHERE ID = p_ID;
END //
DELIMITER ;

-- 6. Get Personel by ID with GorevAdi (for Delete confirmation view)
DELIMITER //
CREATE PROCEDURE sp_GetPersonelByIdForDelete(
    IN p_ID INT
)
BEGIN
    SELECT p.ID, p.Ad, p.Soyad, p.Telefon, p.EPosta, p.Maaş, g.Adı AS GorevAdi
    FROM Personel p
    LEFT JOIN Görev g ON p.GörevID = g.ID
    WHERE p.ID = p_ID;
END //
DELIMITER ;

-- 7. Delete Personel by ID
DELIMITER //
CREATE PROCEDURE sp_DeletePersonelById(
    IN p_ID INT
)
BEGIN
    DELETE FROM Personel WHERE ID = p_ID;
END //
DELIMITER ;

-- 1. For GetMusterilerSelectList
DELIMITER //
CREATE PROCEDURE sp_GetMusterilerForSiparisSelectList()
BEGIN
    SELECT ID, CONCAT(Ad, ' ', Soyad) AS AdSoyad FROM Musteri ORDER BY Ad, Soyad;
END //
DELIMITER ;

-- 2. For GetUrunlerSelectList
DELIMITER //
CREATE PROCEDURE sp_GetUrunlerForSiparisSelectList()
BEGIN
    SELECT ID, Adı FROM Urun ORDER BY Adı;
END //
DELIMITER ;

-- 3. For GetAdreslerByMusteri (AJAX)
DELIMITER //
CREATE PROCEDURE sp_GetAdreslerByMusteriIdForSiparis(
    IN p_MusteriID INT
)
BEGIN
    SELECT ID, Açıklama FROM Adres WHERE MusteriID = p_MusteriID ORDER BY ID;
END //
DELIMITER ;

-- 4. For GetUrunFiyati (AJAX)
DELIMITER //
CREATE PROCEDURE sp_GetUrunFiyatiByIdForSiparis(
    IN p_UrunID INT
)
BEGIN
    SELECT Fiyat FROM Urun WHERE ID = p_UrunID;
END //
DELIMITER ;

-- 5. To get Personel IDs by Görev Adı (replaces internal GetPersonelIDsByGorev)
DELIMITER //
CREATE PROCEDURE sp_GetPersonelIDsByGorevAdi(
    IN p_GorevAdi VARCHAR(255)
)
BEGIN
    SELECT p.ID
    FROM Personel p
    JOIN Görev g ON p.GörevID = g.ID
    WHERE g.Adı = p_GorevAdi;
END //
DELIMITER ;

-- 6. For Index - Get All Siparisler with Details
DELIMITER //
CREATE PROCEDURE sp_GetAllSiparislerWithDetails()
BEGIN
    SELECT s.ID, s.Miktar, s.Fiyat, s.Tarih,
           CONCAT(m.Ad, ' ', m.Soyad) AS MusteriAdiSoyadi,
           u.Adı AS UrunAdi,
           CONCAT(p_usta.Ad, ' ', p_usta.Soyad) AS UstaAdiSoyadi,
           CONCAT(p_kurye.Ad, ' ', p_kurye.Soyad) AS KuryeAdiSoyadi,
           a.Açıklama AS AdresAciklamasi
    FROM Sipariş s
    JOIN Musteri m ON s.MusteriID = m.ID
    JOIN Urun u ON s.UrunID = u.ID
    JOIN Personel p_usta ON s.UstaID = p_usta.ID
    JOIN Personel p_kurye ON s.KuryeID = p_kurye.ID
    JOIN Adres a ON s.AdresID = a.ID
    ORDER BY s.Tarih DESC;
END //
DELIMITER ;

-- 7. For Processing Siparis Creation (THE BIG ONE)
DELIMITER //
CREATE PROCEDURE sp_ProcessSiparisCreation(
    IN p_MusteriID INT,
    IN p_UrunID INT,
    IN p_AdresID INT,
    IN p_SiparisMiktar INT,
    IN p_Tarih DATETIME,
    OUT p_GeneratedSiparisID INT,
    OUT p_Success TINYINT(1),
    OUT p_ErrorMessage VARCHAR(1024)
)
proc_main_sp_ProcessSiparisCreation: BEGIN -- Added label
    DECLARE v_UstaGorevID INT;
    DECLARE v_KuryeGorevID INT;
    DECLARE v_SelectedUstaID INT;
    DECLARE v_SelectedKuryeID INT;
    DECLARE v_MalzemeID_UM INT;       -- MalzemeID from UrunMalzeme
    DECLARE v_Miktar_UM INT;          -- Miktar from UrunMalzeme (per unit of product)
    DECLARE v_GerekliToplamMalzemeMiktari INT;
    DECLARE v_MevcutStokMalzeme INT;
    DECLARE v_UrunBirimFiyat DECIMAL(18,2);
    DECLARE v_HesaplananToplamFiyat DECIMAL(18,2);
    DECLARE v_MalzemeAdiTemp VARCHAR(255);
    DECLARE done_um INT DEFAULT FALSE; -- Cursor flag for UrunMalzeme

    -- Cursor for iterating through UrunMalzeme for the given UrunID
    DECLARE cur_UrunMalzeme CURSOR FOR
        SELECT um.MalzemeID, um.Miktar
        FROM UrunMalzeme um
        WHERE um.UrunID = p_UrunID;
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done_um = TRUE;

    SET p_Success = 0; -- Default to failure
    SET p_ErrorMessage = '';
    SET p_GeneratedSiparisID = 0;

    START TRANSACTION;

    -- Get Görev IDs for Usta and Kurye
    SELECT ID INTO v_UstaGorevID FROM Görev WHERE Adı = 'Usta' LIMIT 1;
    SELECT ID INTO v_KuryeGorevID FROM Görev WHERE Adı = 'Kurye' LIMIT 1;

    IF v_UstaGorevID IS NULL THEN
        SET p_ErrorMessage = 'Usta görevi sistemde tanımlı değil.';
        ROLLBACK;
        LEAVE proc_main_sp_ProcessSiparisCreation; -- Fixed
    END IF;
    IF v_KuryeGorevID IS NULL THEN
        SET p_ErrorMessage = 'Kurye görevi sistemde tanımlı değil.';
        ROLLBACK;
        LEAVE proc_main_sp_ProcessSiparisCreation; -- Fixed
    END IF;

    -- Select Random Usta
    SELECT ID INTO v_SelectedUstaID FROM Personel WHERE GörevID = v_UstaGorevID ORDER BY RAND() LIMIT 1;
    IF v_SelectedUstaID IS NULL THEN
        SET p_ErrorMessage = 'Sipariş için atanabilecek uygun usta bulunamadı.';
        ROLLBACK;
        LEAVE proc_main_sp_ProcessSiparisCreation; -- Fixed
    END IF;

    -- Select Random Kurye
    SELECT ID INTO v_SelectedKuryeID FROM Personel WHERE GörevID = v_KuryeGorevID ORDER BY RAND() LIMIT 1;
    IF v_SelectedKuryeID IS NULL THEN
        SET p_ErrorMessage = 'Sipariş için atanabilecek uygun kurye bulunamadı.';
        ROLLBACK;
        LEAVE proc_main_sp_ProcessSiparisCreation; -- Fixed
    END IF;

    -- Check Malzeme Stok
    OPEN cur_UrunMalzeme;
    stok_check_loop: LOOP
        FETCH cur_UrunMalzeme INTO v_MalzemeID_UM, v_Miktar_UM;
        IF done_um THEN
            LEAVE stok_check_loop;
        END IF;

        SET v_GerekliToplamMalzemeMiktari = v_Miktar_UM * p_SiparisMiktar;
        SELECT `Stok Miktarı` INTO v_MevcutStokMalzeme FROM Malzeme WHERE ID = v_MalzemeID_UM;

        IF v_MevcutStokMalzeme IS NULL THEN -- Malzeme not found in Malzeme table (data integrity issue)
            SELECT Adı INTO v_MalzemeAdiTemp FROM Malzeme WHERE ID = v_MalzemeID_UM; -- Try to get name for error
            SET p_ErrorMessage = CONCAT('Ürün reçetesindeki malzeme (ID: ', v_MalzemeID_UM, IF(v_MalzemeAdiTemp IS NULL, '', CONCAT(' - ', v_MalzemeAdiTemp)) ,') stokta bulunamadı.');
            ROLLBACK;
            CLOSE cur_UrunMalzeme;
            LEAVE proc_main_sp_ProcessSiparisCreation; -- Fixed
        END IF;

        IF v_MevcutStokMalzeme < v_GerekliToplamMalzemeMiktari THEN
            SELECT Adı INTO v_MalzemeAdiTemp FROM Malzeme WHERE ID = v_MalzemeID_UM;
            SET p_ErrorMessage = CONCAT(v_MalzemeAdiTemp, ' için yeterli stok yok. İhtiyaç: ', v_GerekliToplamMalzemeMiktari, ', Mevcut: ', v_MevcutStokMalzeme, '.');
            ROLLBACK;
            CLOSE cur_UrunMalzeme;
            LEAVE proc_main_sp_ProcessSiparisCreation; -- Fixed
        END IF;
    END LOOP;
    CLOSE cur_UrunMalzeme;
    SET done_um = FALSE; -- Reset for next cursor usage

    -- Calculate Sipariş Fiyatı
    SELECT Fiyat INTO v_UrunBirimFiyat FROM Urun WHERE ID = p_UrunID;
    IF v_UrunBirimFiyat IS NULL THEN
        SET p_ErrorMessage = 'Seçilen ürünün fiyat bilgisi bulunamadı.';
        ROLLBACK;
        LEAVE proc_main_sp_ProcessSiparisCreation; -- Fixed
    END IF;
    SET v_HesaplananToplamFiyat = v_UrunBirimFiyat * p_SiparisMiktar;

    -- Insert into Sipariş table
    INSERT INTO Sipariş (MusteriID, UrunID, UstaID, KuryeID, Miktar, Fiyat, Tarih, AdresID)
    VALUES (p_MusteriID, p_UrunID, v_SelectedUstaID, v_SelectedKuryeID, p_SiparisMiktar, v_HesaplananToplamFiyat, p_Tarih, p_AdresID);
    SET p_GeneratedSiparisID = LAST_INSERT_ID();

    IF p_GeneratedSiparisID = 0 THEN
         SET p_ErrorMessage = 'Sipariş kaydı veritabanına eklenemedi.';
         ROLLBACK;
         LEAVE proc_main_sp_ProcessSiparisCreation; -- Fixed
    END IF;

    -- Deduct Malzeme Stok
    OPEN cur_UrunMalzeme; -- Re-open cursor to update stocks
    stok_deduct_loop: LOOP
        FETCH cur_UrunMalzeme INTO v_MalzemeID_UM, v_Miktar_UM;
        IF done_um THEN
            LEAVE stok_deduct_loop;
        END IF;
        SET v_GerekliToplamMalzemeMiktari = v_Miktar_UM * p_SiparisMiktar;
        UPDATE Malzeme SET `Stok Miktarı` = `Stok Miktarı` - v_GerekliToplamMalzemeMiktari
        WHERE ID = v_MalzemeID_UM;
    END LOOP;
    CLOSE cur_UrunMalzeme;

    COMMIT;
    SET p_Success = 1;
    SET p_ErrorMessage = 'Sipariş başarıyla oluşturuldu.';

END proc_main_sp_ProcessSiparisCreation // -- Added label
DELIMITER ;


-- 8. For Get Siparis Details (Delete Confirmation View)
DELIMITER //
CREATE PROCEDURE sp_GetSiparisByIdForDeleteView(
    IN p_SiparisID INT
)
BEGIN
    SELECT s.ID, s.Miktar, s.Fiyat, s.Tarih,
           CONCAT(m.Ad, ' ', m.Soyad) AS MusteriAdiSoyadi,
           u.Adı AS UrunAdi,
           CONCAT(p_usta.Ad, ' ', p_usta.Soyad) AS UstaAdiSoyadi,
           CONCAT(p_kurye.Ad, ' ', p_kurye.Soyad) AS KuryeAdiSoyadi,
           a.Açıklama AS AdresAciklamasi
    FROM Sipariş s
    JOIN Musteri m ON s.MusteriID = m.ID
    JOIN Urun u ON s.UrunID = u.ID
    JOIN Personel p_usta ON s.UstaID = p_usta.ID
    JOIN Personel p_kurye ON s.KuryeID = p_kurye.ID
    JOIN Adres a ON s.AdresID = a.ID
    WHERE s.ID = p_SiparisID;
END //
DELIMITER ;


-- 9. For Processing Siparis Deletion (including stock revert)
DELIMITER //
CREATE PROCEDURE sp_ProcessSiparisDeletion(
    IN p_SiparisID INT,
    OUT p_Success TINYINT(1),
    OUT p_ErrorMessage VARCHAR(255)
)
proc_main_sp_ProcessSiparisDeletion: BEGIN -- Added label
    DECLARE v_UrunID_Siparis INT;
    DECLARE v_Miktar_Siparis INT;
    DECLARE v_MalzemeID_UM INT;
    DECLARE v_Miktar_UM INT;
    DECLARE v_IadeEdilecekMiktar INT;
    DECLARE done_um_delete INT DEFAULT FALSE;

    DECLARE cur_UrunMalzeme_Delete CURSOR FOR
        SELECT um.MalzemeID, um.Miktar
        FROM UrunMalzeme um
        WHERE um.UrunID = v_UrunID_Siparis;
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done_um_delete = TRUE;

    SET p_Success = 0;
    SET p_ErrorMessage = '';

    START TRANSACTION;

    -- Get Siparis details for stock revert
    SELECT UrunID, Miktar INTO v_UrunID_Siparis, v_Miktar_Siparis
    FROM Sipariş WHERE ID = p_SiparisID;

    IF v_UrunID_Siparis IS NULL THEN
        SET p_ErrorMessage = 'Silinecek sipariş bulunamadı.';
        ROLLBACK; -- No need to proceed if siparis doesn't exist
        LEAVE proc_main_sp_ProcessSiparisDeletion; -- Fixed
    END IF;

    -- Revert Malzeme Stok
    IF v_UrunID_Siparis > 0 AND v_Miktar_Siparis > 0 THEN
        OPEN cur_UrunMalzeme_Delete;
        stok_revert_loop: LOOP
            FETCH cur_UrunMalzeme_Delete INTO v_MalzemeID_UM, v_Miktar_UM;
            IF done_um_delete THEN
                LEAVE stok_revert_loop;
            END IF;
            SET v_IadeEdilecekMiktar = v_Miktar_UM * v_Miktar_Siparis;
            UPDATE Malzeme SET `Stok Miktarı` = `Stok Miktarı` + v_IadeEdilecekMiktar
            WHERE ID = v_MalzemeID_UM;
        END LOOP;
        CLOSE cur_UrunMalzeme_Delete;
    END IF;

    -- Delete Sipariş
    DELETE FROM Sipariş WHERE ID = p_SiparisID;
    IF ROW_COUNT() = 0 THEN
        -- This case means siparis was found for stock revert, but couldn't be deleted now.
        -- This is an inconsistent state.
        SET p_ErrorMessage = 'Sipariş silinemedi (belki başka bir işlem tarafından silindi). Stoklar iade edildi.';
        ROLLBACK; -- Rollback stock changes if delete fails
        LEAVE proc_main_sp_ProcessSiparisDeletion; -- Fixed
    END IF;

    COMMIT;
    SET p_Success = 1;
    SET p_ErrorMessage = 'Sipariş başarıyla silindi ve malzeme stokları güncellendi.';

END proc_main_sp_ProcessSiparisDeletion // -- Added label
DELIMITER ;

-- 1. For GetMalzemelerSelectList
DELIMITER //
CREATE PROCEDURE sp_GetMalzemelerForTedarikciSelectList()
BEGIN
    SELECT ID, Adı FROM Malzeme ORDER BY Adı;
END //
DELIMITER ;

-- 2. For Index - Get All Tedarikciler with Malzeme details
DELIMITER //
CREATE PROCEDURE sp_GetAllTedarikcilerWithMalzeme()
BEGIN
    SELECT t.ID, t.Adı, t.Telefon, t.EPosta, t.MalzemeID, m.Adı AS MalzemeAdi
    FROM Tedarikçi t
    LEFT JOIN Malzeme m ON t.MalzemeID = m.ID
    ORDER BY t.Adı;
END //
DELIMITER ;

-- 3. For Create Tedarikci
DELIMITER //
CREATE PROCEDURE sp_CreateTedarikci(
    IN p_Adi VARCHAR(255),
    IN p_Telefon VARCHAR(20),
    IN p_EPosta VARCHAR(100),
    IN p_MalzemeID INT
)
BEGIN
    INSERT INTO Tedarikçi (Adı, Telefon, EPosta, MalzemeID)
    VALUES (p_Adi, p_Telefon, p_EPosta, p_MalzemeID);
END //
DELIMITER ;

-- 4. For Edit (GET) - Get Tedarikci by ID (without MalzemeAdi)
DELIMITER //
CREATE PROCEDURE sp_GetTedarikciByIdForEdit(
    IN p_ID INT
)
BEGIN
    SELECT ID, Adı, Telefon, EPosta, MalzemeID
    FROM Tedarikçi
    WHERE ID = p_ID;
END //
DELIMITER ;

-- 5. For Edit (POST) - Update Tedarikci
DELIMITER //
CREATE PROCEDURE sp_UpdateTedarikci(
    IN p_ID INT,
    IN p_Adi VARCHAR(255),
    IN p_Telefon VARCHAR(20),
    IN p_EPosta VARCHAR(100),
    IN p_MalzemeID INT
)
BEGIN
    UPDATE Tedarikçi
    SET Adı = p_Adi, Telefon = p_Telefon, EPosta = p_EPosta, MalzemeID = p_MalzemeID
    WHERE ID = p_ID;
END //
DELIMITER ;

-- 6. For Delete (GET) - Get Tedarikci by ID with MalzemeAdi for confirmation view
DELIMITER //
CREATE PROCEDURE sp_GetTedarikciByIdForDeleteView(
    IN p_ID INT
)
BEGIN
    SELECT t.ID, t.Adı, t.Telefon, t.EPosta, m.Adı AS MalzemeAdi
    FROM Tedarikçi t
    LEFT JOIN Malzeme m ON t.MalzemeID = m.ID
    WHERE t.ID = p_ID;
END //
DELIMITER ;

-- 7. For Delete (POST) - Delete Tedarikci by ID
DELIMITER //
CREATE PROCEDURE sp_DeleteTedarikciById(
    IN p_ID INT
)
BEGIN
    DELETE FROM Tedarikçi WHERE ID = p_ID;
END //
DELIMITER ;

-- 4. For Index - Get All Urunler with Kategori
DELIMITER //
CREATE PROCEDURE sp_GetAllUrunlerWithKategori()
BEGIN
    SELECT u.ID, u.Adı, u.Fiyat, u.Açıklama, u.KategoriID, k.Adı AS KategoriAdi
    FROM Urun u
    LEFT JOIN Kategori k ON u.KategoriID = k.ID
    ORDER BY u.Adı;
END //
DELIMITER ;

-- 5. For Create Urun (returns new UrunID)
DELIMITER //
CREATE PROCEDURE sp_CreateUrun(
    IN p_Adi VARCHAR(255),
    IN p_Fiyat DECIMAL(18,2),
    IN p_Aciklama TEXT,
    IN p_KategoriID INT,
    OUT p_NewUrunID INT
)
BEGIN
    INSERT INTO Urun (Adı, Fiyat, Açıklama, KategoriID)
    VALUES (p_Adi, p_Fiyat, p_Aciklama, p_KategoriID);
    SET p_NewUrunID = LAST_INSERT_ID();
END //
DELIMITER ;

-- 6. For Create UrunMalzeme (used in a loop after Urun is created)
DELIMITER //
CREATE PROCEDURE sp_AddUrunMalzeme(
    IN p_UrunID INT,
    IN p_MalzemeID INT,
    IN p_Miktar INT
)
BEGIN
    INSERT INTO UrunMalzeme (UrunID, MalzemeID, Miktar)
    VALUES (p_UrunID, p_MalzemeID, p_Miktar);
END //
DELIMITER ;

-- 7. For Edit (GET) - Get Urun details
DELIMITER //
CREATE PROCEDURE sp_GetUrunByIdForEditView(
    IN p_UrunID INT
)
BEGIN
    SELECT ID, Adı, Fiyat, Açıklama, KategoriID FROM Urun WHERE ID = p_UrunID;
END //
DELIMITER ;

-- 8. For Edit (POST) - Update Urun details
DELIMITER //
CREATE PROCEDURE sp_UpdateUrun(
    IN p_ID INT,
    IN p_Adi VARCHAR(255),
    IN p_Fiyat DECIMAL(18,2),
    IN p_Aciklama TEXT,
    IN p_KategoriID INT
)
BEGIN
    UPDATE Urun
    SET Adı = p_Adi, Fiyat = p_Fiyat, Açıklama = p_Aciklama, KategoriID = p_KategoriID
    WHERE ID = p_ID;
END //
DELIMITER ;

-- 9. For AddMalzemeToUrun - Check if Malzeme already exists for Urun
DELIMITER //
CREATE PROCEDURE sp_CheckUrunMalzemeExists(
    IN p_UrunID INT,
    IN p_MalzemeID INT
)
BEGIN
    SELECT COUNT(*) FROM UrunMalzeme WHERE UrunID = p_UrunID AND MalzemeID = p_MalzemeID;
END //
DELIMITER ;
-- (sp_AddUrunMalzeme can be reused for adding)

-- 10. For RemoveMalzemeFromUrun - Delete UrunMalzeme by its ID
DELIMITER //
CREATE PROCEDURE sp_RemoveUrunMalzemeById(
    IN p_UrunMalzemeID INT
)
BEGIN
    DELETE FROM UrunMalzeme WHERE ID = p_UrunMalzemeID;
END //
DELIMITER ;

-- 11. For Delete (GET) - Get Urun details for confirmation view
DELIMITER //
CREATE PROCEDURE sp_GetUrunByIdForDeleteView(
    IN p_UrunID INT
)
BEGIN
    SELECT u.ID, u.Adı, u.Fiyat, u.Açıklama, k.Adı AS KategoriAdi
    FROM Urun u
    LEFT JOIN Kategori k ON u.KategoriID = k.ID
    WHERE u.ID = p_UrunID;
END //
DELIMITER ;

-- 12. For DeleteConfirmed (POST) - Transactional Delete
DELIMITER //
CREATE PROCEDURE sp_DeleteUrunAndMalzemeleri(
    IN p_UrunID INT,
    OUT p_Success TINYINT(1),
    OUT p_ErrorMessage VARCHAR(255)
)
proc_main_sp_DeleteUrunAndMalzemeleri: BEGIN -- Added label
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SET p_Success = 0;
        SET p_ErrorMessage = 'Veritabanı hatası oluştu, işlem geri alındı.';
    END;

    SET p_Success = 0; -- Default to failure
    SET p_ErrorMessage = '';

    START TRANSACTION;

    -- Check if Urun is used in Sipariş
    IF (SELECT COUNT(*) FROM Sipariş WHERE UrunID = p_UrunID) > 0 THEN
        SET p_ErrorMessage = 'Bu ürün siparişlerde kullanıldığı için silinemez.';
        ROLLBACK;
        LEAVE proc_main_sp_DeleteUrunAndMalzemeleri; -- Fixed
    END IF;

    DELETE FROM UrunMalzeme WHERE UrunID = p_UrunID;
    -- No need to check rows affected for UrunMalzeme, as an Urun might not have any.

    DELETE FROM Urun WHERE ID = p_UrunID;
    IF ROW_COUNT() > 0 THEN
        COMMIT;
        SET p_Success = 1;
        SET p_ErrorMessage = 'Ürün ve bağlı malzemeleri başarıyla silindi.';
    ELSE
        SET p_ErrorMessage = 'Ürün silinemedi veya bulunamadı.';
        ROLLBACK;
    END IF;
END proc_main_sp_DeleteUrunAndMalzemeleri // -- Added label
DELIMITER ;

-- 3. For Index - Get All Yorumlar with Details
DELIMITER //
CREATE PROCEDURE sp_GetAllYorumlarWithDetails()
BEGIN
    SELECT y.ID, y.Puan, y.Açıklama, y.Tarih,
           y.MusteriID, CONCAT(m.Ad, ' ', m.Soyad) AS MusteriAdiSoyadi,
           y.SiparişID, u.Adı as SiparisUrunAdi
    FROM Yorum y
    JOIN Musteri m ON y.MusteriID = m.ID
    JOIN Sipariş s ON y.SiparişID = s.ID
    JOIN Urun u ON s.UrunID = u.ID
    ORDER BY y.Tarih DESC;
END //
DELIMITER ;

-- 4. For Create Yorum (includes check for existing comment)
DELIMITER //
CREATE PROCEDURE sp_CreateYorumWithCheck(
    IN p_MusteriID INT,
    IN p_SiparisID INT,
    IN p_Puan INT,
    IN p_Aciklama TEXT,
    IN p_Tarih DATETIME,
    OUT p_Success TINYINT(1),
    OUT p_ErrorMessage VARCHAR(512)
)
proc_main_sp_CreateYorumWithCheck: BEGIN -- Added label
    DECLARE v_UrunID_From_Siparis INT;
    DECLARE v_ExistingCommentCount INT;

    SET p_Success = 0; -- Default to failure
    SET p_ErrorMessage = '';

    START TRANSACTION;

    -- Get UrunID from SiparisID
    SELECT UrunID INTO v_UrunID_From_Siparis FROM Sipariş WHERE ID = p_SiparisID LIMIT 1;

    IF v_UrunID_From_Siparis IS NULL THEN
        SET p_ErrorMessage = 'Geçersiz sipariş seçimi, siparişe ait ürün bulunamadı.';
        ROLLBACK;
        LEAVE proc_main_sp_CreateYorumWithCheck; -- Fixed
    END IF;

    -- Check if Musteri has already commented on this Urun (via any Siparis of that Urun)
    SELECT COUNT(y.ID) INTO v_ExistingCommentCount
    FROM Yorum y
    JOIN Sipariş s ON y.SiparişID = s.ID
    WHERE y.MusteriID = p_MusteriID AND s.UrunID = v_UrunID_From_Siparis;

    IF v_ExistingCommentCount > 0 THEN
        SET p_ErrorMessage = 'Bu ürün için zaten bir yorum yapmışsınız (farklı bir sipariş üzerinden olabilir).';
        ROLLBACK;
        LEAVE proc_main_sp_CreateYorumWithCheck; -- Fixed
    END IF;

    -- Insert Yorum
    INSERT INTO Yorum (MusteriID, SiparişID, Puan, Açıklama, Tarih)
    VALUES (p_MusteriID, p_SiparisID, p_Puan, p_Aciklama, p_Tarih);

    IF ROW_COUNT() > 0 THEN
        COMMIT;
        SET p_Success = 1;
        SET p_ErrorMessage = 'Yorum başarıyla eklendi.';
    ELSE
        SET p_ErrorMessage = 'Yorum eklenirken bir veritabanı hatası oluştu.';
        ROLLBACK;
    END IF;

END proc_main_sp_CreateYorumWithCheck // -- Added label
DELIMITER ;

-- 5. For Delete (GET) - Get Yorum details for confirmation view
DELIMITER //
CREATE PROCEDURE sp_GetYorumByIdForDeleteView(
    IN p_YorumID INT
)
BEGIN
    SELECT y.ID, y.Puan, y.Açıklama, y.Tarih,
           CONCAT(m.Ad, ' ', m.Soyad) AS MusteriAdiSoyadi,
           s.ID AS SiparisIDnum, u.Adı AS SiparisUrunAdi
    FROM Yorum y
    JOIN Musteri m ON y.MusteriID = m.ID
    JOIN Sipariş s ON y.SiparişID = s.ID
    JOIN Urun u ON s.UrunID = u.ID
    WHERE y.ID = p_YorumID;
END //
DELIMITER ;

-- 6. For DeleteConfirmed (POST) - Delete Yorum by ID
DELIMITER //
CREATE PROCEDURE sp_DeleteYorumById(
    IN p_YorumID INT
)
BEGIN
    DELETE FROM Yorum WHERE ID = p_YorumID;
END //
DELIMITER ;