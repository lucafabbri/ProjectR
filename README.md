# ProjectR

**ProjectR** is a lightweight, high-performance .NET library for object-to-object mapping, powered by Roslyn Source Generators. It's designed to automate the repetitive task of writing mapping logic (e.g., from Domain Entities to DTOs) by generating transparent, compile-time C# code based on your own rules and conventions.

## 🤔 Why ProjectR?

In modern applications following Clean or Onion Architecture, mapping objects between layers is a constant necessity. This often leads to boilerplate code that is tedious to write, hard to maintain, and prone to error. While many libraries solve this problem using reflection, they often introduce performance overhead and ""black magic"" that can be difficult to debug.

**ProjectR** offers a different approach:

* **No Runtime Reflection**: All mapping logic is generated at compile time. The result is pure, high-performance C# code, just as if you had written it yourself. This means zero runtime overhead and full AOT/trimming compatibility.

* **You're in Control**: The library is ""unopinionated."" It starts with sensible defaults (mapping properties with matching names) but gives you a powerful and declarative fluent API to define custom mapping rules for complex scenarios, such as constructing Value Objects or handling flattened properties.

* **Compile-Time Safety**: Since the mapping code is generated during compilation, any errors—such as a missing source property or an invalid cast—are caught immediately, not at runtime. This makes refactoring safer and your application more robust.

* **Total Transparency**: You can view the generated code directly in your IDE. There's no hidden magic—just a clean, partial class implementation that you can step through and debug like any other part of your codebase.

## ✨ Features

* **Source Generation**: Automatically generates the implementation for `Mapper<TSource, TDestination>` classes.

* **Convention-Based Mapping**: Out-of-the-box support for mapping properties with matching names (case-insensitive).

* **Attribute-Based Mapping**: Simply add a `[Dto<TEntity>]` attribute to your DTO class to generate a corresponding mapper without creating a separate class file.

* **Custom Mapping Policies**: A powerful fluent API (`ConfigureMappingPolicies`) lets you override the default behavior for creating objects and mapping individual properties and constructor parameters.

* **Dependency Injection Integration**: Provides `AddGeneratedMappers()` for high-performance, reflection-free registration of mappers.

* **Cross-Assembly Discovery**: Automatically registers mappers across multiple projects. The required `[assembly: DiscoverMappers]` attribute is injected automatically by the NuGet package.

* **Nested & Collection Mapping**: Automatically discovers and uses other mappers for complex, nested objects and collections.

## ⚙️ How It Works

The core of ProjectR is a Roslyn Source Generator that activates during compilation.

1. **Trigger**: The generator scans your codebase for two triggers:

   * A class that inherits from `ProjectR.Mapper<TSource, TDestination>`.

   * A class decorated with the `[Dto<TEntity>]` attribute.

2. **Analysis**: For each trigger, it analyzes the source and destination types. It identifies constructors, static factory methods, and public properties.

3. **Policy Application**:

   * It applies a default ""zero-config"" policy, mapping properties with matching names.

   * If it finds a static method named `ConfigureMappingPolicies(IPolicyConfiguration config)`, it parses the user-defined rules and applies them to override the default behavior.

4. **Code Generation**: It generates:
   * A `partial class` implementation in a `.g.cs` file containing the `ProjectGenerated` and `BuildGenerated` methods.
   * A `GeneratedMapperRegistrations` class containing an `AddGeneratedMappers()` extension method for `IServiceCollection`. This method registers all mappers in the assembly directly into the DI container without using reflection.

## 🚀 Getting Started: Example

Here is a complete example of how to map a `CreateProductDto` to a `Product` entity, where the entity uses a `Money` Value Object for its price.

### Installation

First, install the NuGet package into your project.

```bash
dotnet add package ProjectR
dotnet add package ProjectR.Generator
```

### 1. Define Your Models

First, define the domain entity, the value object, and the DTO.

```csharp
// Money.cs (Value Object)
public record Money(decimal Amount, string Currency);

// Product.cs (Domain Entity)
public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Money Price { get; private set; }

    // Private constructor for ORMs
    private Product() {} 

    // Static factory method for controlled creation
    public static Product Create(string name, Money price)
    {
        // Add validation logic here...
        return new Product { Id = Guid.NewGuid(), Name = name, Price = price };
    }
}

// CreateProductDto.cs
// The DtoAttribute triggers the generation of a 'CreateProductDtoMapper' class.
[Dto<Product>]
public class CreateProductDto
{
    public string Name { get; set; }
    public decimal PriceAmount { get; set; }
    public string PriceCurrency { get; set; }
}

```

### 2. Define a Custom Mapping Policy (Optional)

For simple one-to-one mappings, the `[Dto<TEntity>]` attribute is sufficient. However, to create the `Money` Value Object from two separate DTO properties (`PriceAmount` and `PriceCurrency`), we need a custom policy. To do this, we create a mapper class.

The source generator will automatically create a `CreateProductDtoMapper.ph.g.cs` file from the attribute. We just need to add our `partial` class with the policy configuration.

```csharp
// CreateProductDtoMapper.cs
public partial class CreateProductDtoMapper
{
    /// <summary>
    /// This method implements a custom policy to handle the creation of a Product,
    /// specifically for constructing the Money value object from two separate DTO properties.
    /// </summary>
    static void ConfigureMappingPolicies(IPolicyConfiguration config)
    {
        // Define a custom policy for the Build(CreateProductDto source) method.
        config.ForCreation<CreateProductDto, Product>()
              // Instruct the engine to prioritize using a static factory for creation.
              .Try(MappingStrategy.UseStaticFactories)
              // The "price" parameter of the Product.Create factory is complex.
              // We provide a custom expression to build the Money object from the DTO.
              .MapParameter("price")
              .FromSource(dto => new Money(dto.PriceAmount, dto.PriceCurrency));
    }
}

```

*Note: The generator will detect this method and use it to build the implementation.*

### 3. Configure Dependency Injection

In your `Program.cs`, call `AddGeneratedMappers()` to register all mappers. To discover mappers from referenced assemblies, add the `[assembly: DiscoverMappers]` attribute to your assembly.

```csharp
// Program.cs
using ProjectR.Generated; // The namespace where registrations are generated

// Note: [assembly: DiscoverMappers] is automatically injected by the NuGet package.
// You do not need to add it manually unless you have disabled auto-discovery.

var builder = WebApplication.CreateBuilder(args);

// ... other services

// High-performance registration generated at compile-time.
builder.Services.AddGeneratedMappers();

var app = builder.Build();

// ...
```

> [!NOTE]
> The old reflection-based `AddMappers()` is now deprecated and should be replaced with `AddGeneratedMappers()`.

### 4. Use the Mapper in Your Services

Inject `IMapperResolver` into your services to get mapper instances at runtime.

```csharp
// ProductService.cs
public class ProductService
{
    private readonly IMapperResolver _mapperResolver;
    private readonly IProductRepository _productRepository;

    public ProductService(IMapperResolver mapperResolver, IProductRepository productRepository)
    {
        _mapperResolver = mapperResolver;
        _productRepository = productRepository;
    }

    public async Task<Guid> CreateProductAsync(CreateProductDto dto)
    {
        // 1. Get the specific mapper for this operation.
        var mapper = _mapperResolver.GetMapper<CreateProductDto, Product>();

        // 2. Use the generated 'Build' method to create the entity.
        var product = mapper.Build(dto);

        // 3. Save the entity.
        await _productRepository.AddAsync(product);

        return product.Id;
    }
}

```

The `mapper.Build(dto)` call will now execute the generated code, which includes your custom logic for creating the `Money` object.

## 🤝 Contributing

Contributions, issues, and feature requests are welcome! Feel free to check the [issues page](https://www.google.com/search?q=https://github.com/lucafabbri/ProjectR/issues) to report a bug or suggest a new feature, or open a Pull Request directly.

## 💖 Show Your Support

Please give a ⭐️ if this project helped you! Your support is much appreciated.

## 📝 License

This project is licensed under the **MIT License**.
