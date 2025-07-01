## MappingValidator

MappingValidator is a lightweight and powerful validation tool that detects missing fields in mappings between source and destination objects without relying on an external mapping library.

**Dependency:** .NET 9

### ✨ Features

- Works independently of mapping libraries (AutoMapper, Mapster, manual mapping, etc.)
- Detects fields that are not automatically or manually set in the target object
- Excludes specific fields with a customizable Ignore list
- Configurations are stored and reused by type pair (TSource, TDestination)

### 🚀 Installation

Reference the project as a DLL or add it via NuGet:

    MappingValidator

### 🛠️ Usage

#### 1. Example Model Definitions

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

#### 2. Configuration Class
    @using MappingValidator

    public static class MappingValidatorConfigurations
    {
        public static void RegisterAll()
        {
            MapValidator.Configure<Source, Destination>(builder =>
            {
                builder.Ignore(dest => dest.CreatedBy);
            });
        }
    }

#### 3. Add Configuration to Program.cs

    MapValidator.Configure<Source, Destination>();

#### 4. Application Usage
    @using MappingValidator

    try
    {
        var source = new Source { Name = "Murat", Email = "murat@example.com" };
        var entity = Mapper.Map<Destination>(source);
    
        entity.Role = "Admin";
        //entity.CreatedBy = "murat.koc";
    
        MapValidator.Validate<Source, Destination>(entity);
    
        Console.WriteLine("All manual fields set successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }

If the CreatedBy field is not set and is not added to the Ignore list:

> ** ValidationException:** Required fields not set on Destination: CreatedBy

##### 🎯 When to Use?

- If there are fields that need to be filled in the target object after mapping
- If it is desired to detect missing assignments in mapping operations such as AutoMapper, Mapster, Mapperly
- If it is aimed to prevent missing data from going to the database

📄 License

MIT
