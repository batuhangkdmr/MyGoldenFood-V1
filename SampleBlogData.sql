-- Örnek Blog Yazıları - My Golden Food
-- Bu script'i SQL Server Management Studio'da çalıştırın

-- Blog 1: Donuk Gıda ile Sağlıklı Beslenme
INSERT INTO [Blogs] ([Title], [Content], [Summary], [Category], [Tags], [Author], [CreatedDate], [IsPublished], [ViewCount], [SeoTitle], [SeoDescription], [SeoKeywords], [SeoUrl])
VALUES (
    'Donuk Gıda ile Sağlıklı Beslenme: Besin Değerini Koruyan Teknoloji',
    '<h2>Donuk Gıda Nedir?</h2>
    <p>Donuk gıda, modern teknoloji sayesinde besin değerini koruyarak uzun süre saklanabilen gıda ürünleridir. Flash freezing teknolojisi kullanılarak üretilen bu ürünler, taze gıdaların tüm vitamin ve minerallerini korur.</p>
    
    <h3>Donuk Gıdaların Avantajları</h3>
    <ul>
        <li>Besin değeri korunur</li>
        <li>Uzun raf ömrü</li>
        <li>Mevsim dışı erişim</li>
        <li>Kolay saklama</li>
    </ul>
    
    <h3>Hangi Gıdalar Donuk Olarak Saklanabilir?</h3>
    <p>Meyveler, sebzeler, et ürünleri ve deniz ürünleri donuk gıda olarak saklanabilir. Özellikle dondurulmuş meyve ve dondurulmuş sebze ürünleri, taze alternatiflerine göre daha uzun süre saklanabilir.</p>',
    
    'Donuk gıdaların besin değerini koruyarak sağlıklı beslenme yolları ve flash freezing teknolojisinin avantajları hakkında detaylı bilgi.',
    
    'Sağlık & Beslenme',
    
    'donuk gıda, sağlık, beslenme, dondurulmuş meyve, flash freezing, vitamin, mineral',
    
    'Admin',
    
    GETDATE(),
    
    1,
    
    0,
    
    'Donuk Gıda ile Sağlıklı Beslenme - Flash Freezing Teknolojisi',
    
    'Donuk gıdaların besin değerini koruyarak sağlıklı beslenme yolları. Flash freezing teknolojisi ile üretilen donuk gıda ürünlerinin avantajları ve kullanım alanları.',
    
    'donuk gıda, sağlıklı beslenme, flash freezing, dondurulmuş meyve, dondurulmuş sebze, besin değeri, vitamin, mineral',
    
    'donuk-gida-ile-saglikli-beslenme'
);

-- Blog 2: Dondurulmuş Sebzelerin Faydaları
INSERT INTO [Blogs] ([Title], [Content], [Summary], [Category], [Tags], [Author], [CreatedDate], [IsPublished], [ViewCount], [SeoTitle], [SeoDescription], [SeoKeywords], [SeoUrl])
VALUES (
    'Dondurulmuş Sebzelerin Faydaları: Flash Freezing Teknolojisi',
    '<h2>Flash Freezing Teknolojisi Nedir?</h2>
    <p>Flash freezing, sebzeleri çok hızlı bir şekilde dondurarak hücre yapısını koruyan gelişmiş bir teknolojidir. Bu sayede sebzeler taze haldeki besin değerlerini korur.</p>
    
    <h3>Dondurulmuş Sebzelerin Avantajları</h3>
    <ul>
        <li>Vitamin ve mineral kaybı minimum</li>
        <li>Uzun süre taze kalır</li>
        <li>Mevsim dışı erişim</li>
        <li>Hazır kullanım</li>
    </ul>
    
    <h3>Hangi Sebzeler Dondurulabilir?</h3>
    <p>Brokoli, ıspanak, bezelye, mısır, havuç gibi sebzeler flash freezing teknolojisi ile başarıyla dondurulabilir. Bu sebzeler, taze alternatiflerine göre daha uzun süre saklanabilir.</p>',
    
    'Flash freezing teknolojisi ile dondurulan sebzelerin besin değerini koruma özellikleri ve sağlık faydaları hakkında detaylı bilgi.',
    
    'Sebze & Meyve',
    
    'dondurulmuş sebze, vitamin, mineral, flash freezing, besin değeri, sağlık',
    
    'Admin',
    
    GETDATE(),
    
    1,
    
    0,
    
    'Dondurulmuş Sebzelerin Faydaları - Flash Freezing Teknolojisi',
    
    'Flash freezing teknolojisi ile dondurulan sebzelerin besin değerini koruma özellikleri. Dondurulmuş sebzelerin sağlık faydaları ve kullanım alanları.',
    
    'dondurulmuş sebze, flash freezing, vitamin, mineral, besin değeri, sağlık, sebze dondurma',
    
    'dondurulmus-sebzelerin-faydalari'
);

-- Blog 3: Donuk Gıda Depolama Teknikleri
INSERT INTO [Blogs] ([Title], [Content], [Summary], [Category], [Tags], [Author], [CreatedDate], [IsPublished], [ViewCount], [SeoTitle], [SeoDescription], [SeoKeywords], [SeoUrl])
VALUES (
    'Donuk Gıda Depolama Teknikleri: Uzun Süre Taze Tutma Yöntemleri',
    '<h2>Donuk Gıda Depolama Kuralları</h2>
    <p>Donuk gıdaları uzun süre taze tutmak için belirli kurallara uymak gerekir. Sıcaklık kontrolü, paketleme ve saklama koşulları önemlidir.</p>
    
    <h3>Optimal Depolama Sıcaklığı</h3>
    <ul>
        <li>Sebzeler: -18°C</li>
        <li>Meyveler: -18°C</li>
        <li>Et ürünleri: -18°C</li>
        <li>Deniz ürünleri: -18°C</li>
    </ul>
    
    <h3>Paketleme Önerileri</h3>
    <p>Donuk gıdaları hava almayacak şekilde paketlemek önemlidir. Vakum paketleme veya sıkı kapatılmış kaplar kullanılmalıdır. Bu sayede freezer burn önlenir.</p>',
    
    'Donuk gıdaları uzun süre taze tutma yöntemleri, optimal depolama sıcaklıkları ve paketleme önerileri hakkında detaylı bilgi.',
    
    'Teknik & Bilgi',
    
    'depolama, donuk gıda, muhafaza, sıcaklık, paketleme, freezer burn',
    
    'Admin',
    
    GETDATE(),
    
    1,
    
    0,
    
    'Donuk Gıda Depolama Teknikleri - Uzun Süre Taze Tutma',
    
    'Donuk gıdaları uzun süre taze tutma yöntemleri. Optimal depolama sıcaklıkları, paketleme önerileri ve muhafaza teknikleri.',
    
    'donuk gıda depolama, muhafaza teknikleri, sıcaklık kontrolü, paketleme, freezer burn önleme',
    
    'donuk-gida-depolama-teknikleri'
);

-- Blog 4: Dondurulmuş Meyvelerle Tarifler
INSERT INTO [Blogs] ([Title], [Content], [Summary], [Category], [Tags], [Author], [CreatedDate], [IsPublished], [ViewCount], [SeoTitle], [SeoDescription], [SeoKeywords], [SeoUrl])
VALUES (
    'Dondurulmuş Meyvelerle Lezzetli Tarifler: Smoothie ve Tatlılar',
    '<h2>Dondurulmuş Meyve Smoothie Tarifi</h2>
    <p>Dondurulmuş meyvelerle yapılan smoothie''ler hem lezzetli hem de besleyicidir. İşte basit bir smoothie tarifi:</p>
    
    <h3>Malzemeler:</h3>
    <ul>
        <li>1 su bardağı dondurulmuş çilek</li>
        <li>1 adet muz</li>
        <li>1 su bardağı süt</li>
        <li>1 yemek kaşığı bal</li>
    </ul>
    
    <h3>Hazırlanışı:</h3>
    <p>Tüm malzemeleri blender''a koyun ve pürüzsüz olana kadar karıştırın. Dondurulmuş meyveler sayesinde soğuk ve ferah bir smoothie elde edersiniz.</p>
    
    <h2>Dondurulmuş Meyve Tatlısı</h2>
    <p>Dondurulmuş meyvelerle yapılan meyve salatası da harika bir tatlı alternatifidir.</p>',
    
    'Dondurulmuş meyvelerle yapılan lezzetli smoothie ve tatlı tarifleri. Besleyici ve ferah smoothie hazırlama yöntemleri.',
    
    'Tarifler',
    
    'dondurulmuş meyve, tarif, smoothie, tatlı, meyve salatası, blender',
    
    'Admin',
    
    GETDATE(),
    
    1,
    
    0,
    
    'Dondurulmuş Meyvelerle Lezzetli Tarifler - Smoothie ve Tatlılar',
    
    'Dondurulmuş meyvelerle yapılan lezzetli smoothie ve tatlı tarifleri. Besleyici smoothie hazırlama yöntemleri ve meyve tatlıları.',
    
    'dondurulmuş meyve tarifleri, smoothie, meyve tatlısı, blender tarifleri, besleyici içecekler',
    
    'dondurulmus-meyvelerle-tarifler'
);

-- Blog 5: Donuk Gıda Endüstrisinde Teknolojik Gelişmeler
INSERT INTO [Blogs] ([Title], [Content], [Summary], [Category], [Tags], [Author], [CreatedDate], [IsPublished], [ViewCount], [SeoTitle], [SeoDescription], [SeoKeywords], [SeoUrl])
VALUES (
    'Donuk Gıda Endüstrisinde Teknolojik Gelişmeler: Flash Freezing Yenilikleri',
    '<h2>Modern Flash Freezing Teknolojileri</h2>
    <p>Donuk gıda endüstrisinde son yıllarda büyük teknolojik gelişmeler yaşanmaktadır. Flash freezing teknolojisi sürekli olarak geliştirilmekte ve yeni uygulama alanları bulmaktadır.</p>
    
    <h3>Yeni Teknolojik Gelişmeler</h3>
    <ul>
        <li>Kriyojenik dondurma sistemleri</li>
        <li>Akıllı sıcaklık kontrolü</li>
        <li>Otomatik paketleme sistemleri</li>
        <li>Kalite kontrol robotları</li>
    </ul>
    
    <h3>Endüstriyel Uygulamalar</h3>
    <p>Flash freezing teknolojisi artık sadece gıda sektöründe değil, ilaç ve kimyasal madde endüstrisinde de kullanılmaktadır. Bu teknoloji sayesinde ürün kalitesi artmakta ve maliyetler düşmektedir.</p>',
    
    'Donuk gıda sektöründeki son teknolojik gelişmeler, flash freezing yenilikleri ve endüstriyel uygulamalar hakkında detaylı bilgi.',
    
    'Teknoloji & Endüstri',
    
    'teknoloji, flash freezing, endüstri, yenilik, kriyojenik, otomasyon',
    
    'Admin',
    
    GETDATE(),
    
    1,
    
    0,
    
    'Donuk Gıda Endüstrisinde Teknolojik Gelişmeler - Flash Freezing',
    
    'Donuk gıda sektöründeki son teknolojik gelişmeler. Flash freezing yenilikleri, kriyojenik sistemler ve endüstriyel otomasyon.',
    
    'donuk gıda teknolojisi, flash freezing gelişmeleri, kriyojenik dondurma, endüstriyel otomasyon, gıda teknolojisi',
    
    'donuk-gida-endustrisinde-teknolojik-gelismeler'
);

-- Başarı mesajı
PRINT '5 adet örnek blog yazısı başarıyla eklendi!';
PRINT 'Blog yazıları şu kategorilerde eklendi:';
PRINT '1. Sağlık & Beslenme';
PRINT '2. Sebze & Meyve';
PRINT '3. Teknik & Bilgi';
PRINT '4. Tarifler';
PRINT '5. Teknoloji & Endüstri';
