using ProjectR.Attributes;
using ProjectR.Sample.Domain;
using System;
using System.Collections.Generic;

namespace ProjectR.Sample.Application.DTOs
{
    // A simple DTO for the Money value object.
    [Dto<Money>]
    public class MoneyDto
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } 
    }

    // A DTO for the Review entity.
    [Dto<Review>]
    public class ReviewDto
    {
        public int Stars { get; set; }
        public string Comment { get; set; }
    }

    // A DTO representing the full Product entity for read operations.
    [Dto<Product>]
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public MoneyDto Price { get; set; }
        public List<ReviewDto> Reviews { get; set; }
    }

    // A DTO used for creating a new product.
    public class CreateProductDto
    {
        public string Name { get; set; }
        public decimal PriceAmount { get; set; }
        public string PriceCurrency { get; set; }
    }

    // A DTO used for updating an existing product.
    [Dto<Product>]
    public class UpdateProductDto
    {
        public string Name { get; set; }
    }

    public abstract class BaseEntityDto
    {
        public Guid Id { get; set; }
    }

    [Dto<Category>]
    public class CategoryDto : BaseEntityDto
    {
        public string Name { get; set; }
    }
}
