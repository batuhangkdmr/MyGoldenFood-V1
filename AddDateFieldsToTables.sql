-- =============================================
-- MyGoldenFood - Tarih Alanları Ekleme
-- Recipes ve RecipeCategories Tablolarına
-- =============================================

-- 1. RECIPES TABLOSUNA TARİH ALANLARI EKLE
-- =============================================

-- Tarih alanları ekle
ALTER TABLE Recipes 
ADD CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE();

ALTER TABLE Recipes 
ADD UpdatedDate DATETIME2 NULL;

-- 2. RECIPECATEGORIES TABLOSUNA TARİH ALANLARI EKLE
-- =============================================

-- Tarih alanları ekle
ALTER TABLE RecipeCategories 
ADD CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE();

ALTER TABLE RecipeCategories 
ADD UpdatedDate DATETIME2 NULL;

-- 3. MEVCUT VERİLERİ GÜNCELLE
-- =============================================

-- Mevcut tarifler için tarih alanlarını güncelle
UPDATE Recipes 
SET CreatedDate = GETDATE()
WHERE CreatedDate IS NULL;

-- Mevcut kategoriler için tarih alanlarını güncelle
UPDATE RecipeCategories 
SET CreatedDate = GETDATE()
WHERE CreatedDate IS NULL;

-- 4. KONTROL ET
-- =============================================

-- Eklenen alanları kontrol et
SELECT 
    'Recipes' as TableName,
    COUNT(*) as TotalRecords,
    COUNT(CreatedDate) as CreatedDateCount,
    COUNT(UpdatedDate) as UpdatedDateCount
FROM Recipes

UNION ALL

SELECT 
    'RecipeCategories' as TableName,
    COUNT(*) as TotalRecords,
    COUNT(CreatedDate) as CreatedDateCount,
    COUNT(UpdatedDate) as UpdatedDateCount
FROM RecipeCategories;

-- 5. BAŞARILI MESAJ
-- =============================================
PRINT '✅ Tarih alanları başarıyla eklendi!';
PRINT '📊 Recipes tablosuna CreatedDate ve UpdatedDate eklendi';
PRINT '📊 RecipeCategories tablosuna CreatedDate ve UpdatedDate eklendi';
PRINT '🔄 Mevcut veriler otomatik güncellendi';
