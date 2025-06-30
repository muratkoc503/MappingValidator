## MappingValidator

MappingValidator, harici bir eşleme (mapping) kütüphanesine bağımlı olmadan kaynak (source) ve hedef (destination) objeler arasında yapılan eşlemelerde eksik set edilen alanları tespit eden hafif ve güçlü bir doğrulama aracıdır.

### ✨ Özellikler

- Eşleme kütüphanelerinden bağımsız çalışır (AutoMapper, Mapster, manuel mapping vs.)
- Hedef nesnede otomatik veya manuel olarak set edilmemiş alanları tespit eder
- Özelleştirilebilir Ignore listesi ile belirli alanlar hariç tutulabilir
- Konfigürasyonlar tip çiftine göre (TSource, TDestination) saklanır ve yeniden kullanılabilir

### 🚀 Kurulum

DLL olarak projeye referans verin ya da NuGet üzerinden ekleyin:

    dotnet add package MappingValidator

### 🛠️ Kullanım

#### 1. Örnek Model Tanımları

    public class Source
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }
    
    public class Destination
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string CreatedBy { get; set; }
    }

#### 2. Konfigürasyonu Sınıfı

    public static class MappingValidatorConfigurations
    {
        public static void RegisterAll()
        {
            MappingValidator.Configure<Source, Destination>(builder =>
            {
                builder.Ignore(dest => dest.CreatedBy);
            });
        }
    }

#### 3. Konfigürasyonu Program.cs dosyasına Ekle

    MappingValidator.Configure<Source, Destination>();

#### 4. Uygulama Kullanımı

    try
    {
        var source = new Source { Name = "Murat", Email = "murat@example.com" };
        var entity = Mapper.Map<Destination>(source);
    
        entity.Role = "Admin";
        //entity.CreatedBy = "murat.koc";
    
        MappingValidator.Validate<Source, Destination>(entity);
    
        Console.WriteLine("All manual fields set successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }

Eğer CreatedBy alanı set edilmez ve Ignore listesine de eklenmemişse:

> ** ValidationException:** Required fields not set on Destination: CreatedBy



##### 🎯 Ne Zaman Kullanılır?

- Mapping sonrası hedef nesnede doldurulması gereken alanlar varsa
- AutoMapper, Mapster, Mapperly gibi mapping işlemlerinde eksik atamaları tespit etmek isteniyorsa
- Veritabanına eksik veri gitmesini önlemek amaçlanıyorsa

📄 Lisans

MIT
