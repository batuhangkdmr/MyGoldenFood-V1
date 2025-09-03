-- =============================================
-- MyGoldenFood - Tablo Güncellemeleri
-- SEO ve Alt Kategori Desteği İçin
-- =============================================

-- 1. RECIPES TABLOSUNA YENİ ALANLAR EKLE
-- =============================================

-- Tarih alanları ekle
ALTER TABLE Recipes 
ADD CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE();

ALTER TABLE Recipes 
ADD UpdatedDate DATETIME2 NULL;

-- SEO alanları ekle
ALTER TABLE Recipes 
ADD SeoTitle NVARCHAR(255) NULL;

ALTER TABLE Recipes 
ADD SeoDescription NVARCHAR(500) NULL;

ALTER TABLE Recipes 
ADD SeoKeywords NVARCHAR(500) NULL;

ALTER TABLE Recipes 
ADD SeoSlug NVARCHAR(255) NULL;

-- Tarif detay alanları ekle
ALTER TABLE Recipes 
ADD PreparationTime NVARCHAR(100) NULL;

ALTER TABLE Recipes 
ADD CookingTime NVARCHAR(100) NULL;

ALTER TABLE Recipes 
ADD Servings INT NULL;

ALTER TABLE Recipes 
ADD Difficulty NVARCHAR(50) NULL;

-- 2. RECIPECATEGORIES TABLOSUNA YENİ ALANLAR EKLE
-- =============================================

-- Tarih alanları ekle
ALTER TABLE RecipeCategories 
ADD CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE();

ALTER TABLE RecipeCategories 
ADD UpdatedDate DATETIME2 NULL;

-- Alt kategori sistemi alanları ekle
ALTER TABLE RecipeCategories 
ADD ParentCategoryId INT NULL;

ALTER TABLE RecipeCategories 
ADD Level INT NOT NULL DEFAULT 0;

ALTER TABLE RecipeCategories 
ADD SortOrder INT NOT NULL DEFAULT 0;

-- 3. FOREIGN KEY EKLE (Self-referencing)
-- =============================================

ALTER TABLE RecipeCategories 
ADD CONSTRAINT FK_RecipeCategories_ParentCategory 
FOREIGN KEY (ParentCategoryId) REFERENCES RecipeCategories(Id) 
ON DELETE SET NULL;

-- 4. INDEX'LER EKLE (Performans için)
-- =============================================

-- SEO slug için unique index
CREATE UNIQUE INDEX IX_Recipes_SeoSlug ON Recipes(SeoSlug) WHERE SeoSlug IS NOT NULL;

-- Alt kategori sorguları için index
CREATE INDEX IX_RecipeCategories_ParentCategoryId ON RecipeCategories(ParentCategoryId);
CREATE INDEX IX_RecipeCategories_Level ON RecipeCategories(Level);
CREATE INDEX IX_RecipeCategories_SortOrder ON RecipeCategories(SortOrder);

-- 5. MEVCUT VERİLERİ GÜNCELLE
-- =============================================

-- Mevcut tarifler için SEO alanlarını otomatik oluştur
UPDATE Recipes 
SET 
    SeoTitle = Name + ' Tarifi | Dondurulmuş Gıda ile Lezzetli Tarif | My Golden Food',
    SeoDescription = Name + ' tarifi. Dondurulmuş gıda ile hazırlanan lezzetli ve pratik tarif. ' + 
                     CASE 
                         WHEN LEN(Content) > 150 THEN LEFT(Content, 150) + '...'
                         ELSE Content 
                     END,
    SeoKeywords = 'dondurulmuş gıda tarifi, ' + LOWER(Name) + ', donuk gıda tarifleri, pratik yemek tarifi, my golden food',
    SeoSlug = LOWER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
        Name, 'ç', 'c'), 'ğ', 'g'), 'ı', 'i'), 'ö', 'o'), 'ş', 's'), 'ü', 'u'), ' ', '-'), '(', ''), ')', ''), ',', '')
    ),
    CreatedDate = GETDATE()
WHERE SeoTitle IS NULL;

-- Mevcut kategoriler için tarih alanlarını güncelle
UPDATE RecipeCategories 
SET 
    CreatedDate = GETDATE(),
    Level = 0,  -- Tüm mevcut kategoriler ana kategori olarak işaretle
    SortOrder = Id  -- ID'ye göre sıralama
WHERE CreatedDate IS NULL;

-- 6. VERİ KONTROLÜ
-- =============================================

-- Eklenen alanları kontrol et
SELECT 
    'Recipes' as TableName,
    COUNT(*) as TotalRecords,
    COUNT(SeoTitle) as SeoTitleCount,
    COUNT(SeoSlug) as SeoSlugCount,
    COUNT(CreatedDate) as CreatedDateCount
FROM Recipes

UNION ALL

SELECT 
    'RecipeCategories' as TableName,
    COUNT(*) as TotalRecords,
    COUNT(ParentCategoryId) as ParentCategoryCount,
    COUNT(Level) as LevelCount,
    COUNT(CreatedDate) as CreatedDateCount
FROM RecipeCategories;

-- 7. BAŞARILI MESAJ
-- =============================================
PRINT '✅ Tablo güncellemeleri başarıyla tamamlandı!';
PRINT '📊 Recipes tablosuna SEO alanları eklendi';
PRINT '📊 RecipeCategories tablosuna alt kategori alanları eklendi';
PRINT '🔗 Foreign key ilişkileri kuruldu';
PRINT '📈 Performans indexleri oluşturuldu';
PRINT '🔄 Mevcut veriler otomatik güncellendi';
