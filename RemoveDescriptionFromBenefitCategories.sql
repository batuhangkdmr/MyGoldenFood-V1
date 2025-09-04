-- BenefitCategories tablosundan Description alanını kaldır

-- 1. Önce Description alanını kaldır
ALTER TABLE BenefitCategories DROP COLUMN Description;

-- 2. BenefitCategoryTranslations tablosundan da Description alanını kaldır (eğer varsa)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'BenefitCategoryTranslations' AND COLUMN_NAME = 'Description')
BEGIN
    ALTER TABLE BenefitCategoryTranslations DROP COLUMN Description;
    PRINT 'BenefitCategoryTranslations tablosundan Description alanı kaldırıldı';
END
ELSE
BEGIN
    PRINT 'BenefitCategoryTranslations tablosunda Description alanı bulunamadı';
END

-- 3. Kontrol sorguları
SELECT 'Description alanı kaldırıldı' as Status;

-- 4. Mevcut kolonları kontrol et
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'BenefitCategories' 
ORDER BY ORDINAL_POSITION;

-- 5. BenefitCategoryTranslations tablosunu da kontrol et
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'BenefitCategoryTranslations' 
ORDER BY ORDINAL_POSITION;
