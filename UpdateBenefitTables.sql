-- Benefit ve BenefitCategory tablolarını güncelleme scripti
-- Tarifler tabloları için yapılan güncellemelerin aynısı

-- 1. Benefits tablosuna yeni kolonları ekle
ALTER TABLE Benefits ADD CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE();
ALTER TABLE Benefits ADD UpdatedDate DATETIME2 NULL;

-- SEO alanları
ALTER TABLE Benefits ADD SeoTitle NVARCHAR(500) NULL;
ALTER TABLE Benefits ADD SeoDescription NVARCHAR(1000) NULL;
ALTER TABLE Benefits ADD SeoKeywords NVARCHAR(1000) NULL;
ALTER TABLE Benefits ADD SeoSlug NVARCHAR(500) NULL;

-- Tarif detay alanları
ALTER TABLE Benefits ADD PreparationTime NVARCHAR(50) NULL;
ALTER TABLE Benefits ADD CookingTime NVARCHAR(50) NULL;
ALTER TABLE Benefits ADD Servings INT NULL;
ALTER TABLE Benefits ADD Difficulty NVARCHAR(50) NULL;

-- 2. BenefitCategories tablosuna yeni kolonları ekle
ALTER TABLE BenefitCategories ADD CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE();
ALTER TABLE BenefitCategories ADD UpdatedDate DATETIME2 NULL;

-- Alt kategori sistemi
ALTER TABLE BenefitCategories ADD ParentCategoryId INT NULL;
ALTER TABLE BenefitCategories ADD Level INT NOT NULL DEFAULT 0;
ALTER TABLE BenefitCategories ADD SortOrder INT NOT NULL DEFAULT 0;

-- 3. Foreign Key ekle (ParentCategoryId için)
ALTER TABLE BenefitCategories 
ADD CONSTRAINT FK_BenefitCategories_ParentCategory 
FOREIGN KEY (ParentCategoryId) REFERENCES BenefitCategories(Id);

-- 4. Unique Index ekle (SeoSlug için)
CREATE UNIQUE INDEX IX_Benefits_SeoSlug ON Benefits(SeoSlug) WHERE SeoSlug IS NOT NULL;

-- 5. Index'ler ekle (performans için)
CREATE INDEX IX_BenefitCategories_ParentCategoryId ON BenefitCategories(ParentCategoryId);
CREATE INDEX IX_BenefitCategories_Level ON BenefitCategories(Level);
CREATE INDEX IX_BenefitCategories_SortOrder ON BenefitCategories(SortOrder);

-- 6. Mevcut verileri güncelle (Level = 0 olarak ayarla)
UPDATE BenefitCategories SET Level = 0 WHERE Level IS NULL;

-- 7. Kontrol sorguları
SELECT 'Benefits tablosu güncellendi' as Status;
SELECT COUNT(*) as BenefitCount FROM Benefits;

SELECT 'BenefitCategories tablosu güncellendi' as Status;
SELECT COUNT(*) as BenefitCategoryCount FROM BenefitCategories;

-- 8. Yeni kolonları kontrol et
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Benefits' 
AND COLUMN_NAME IN ('CreatedDate', 'UpdatedDate', 'SeoTitle', 'SeoDescription', 'SeoKeywords', 'SeoSlug', 'PreparationTime', 'CookingTime', 'Servings', 'Difficulty');

SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'BenefitCategories' 
AND COLUMN_NAME IN ('CreatedDate', 'UpdatedDate', 'ParentCategoryId', 'Level', 'SortOrder');
