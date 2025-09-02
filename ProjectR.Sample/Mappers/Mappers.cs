using ProjectR;
using ProjectR.Sample.Domain;
using ProjectR.Sample.Application.DTOs;

namespace ProjectR.Sample.Application.Mappers
{
    /// <summary>
    /// A simple mapper for the Money value object.
    /// ProjectR handles this with its "zero-config" default policy.
    /// </summary>
    public partial class MoneyMapper : Mapper<Money, MoneyDto> { } 

    /// <summary>
    /// A simple mapper for the Review entity.
    /// Also handled by the "zero-config" default policy.
    /// </summary>
    public partial class ReviewMapper : Mapper<Review, ReviewDto> { }

    /// <summary>
    /// This mapper handles the standard projection from the Product entity to the ProductDto.
    /// It's "zero-config" because it relies on the MoneyMapper and ReviewMapper for its complex properties.
    /// </summary>
    public partial class ProductMapper : Mapper<Product, ProductDto> { }

    /// <summary>
    /// A specialized mapper dedicated to creating a Product entity from a CreateProductDto.
    /// This separation of concerns keeps the mapping logic clean and focused.
    /// </summary>
    public partial class CreateProductMapper : Mapper<Product, CreateProductDto> 
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
                .MapParameter("price").FromSource(dto => new Money(dto.PriceAmount, dto.PriceCurrency));
        }
    }
}

