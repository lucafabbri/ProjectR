using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using ProjectR.Sample.Domain;
using ProjectR.Sample.Application.DTOs;
using ProjectR.Sample.Application.Mappers;

namespace ProjectR.Sample.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {

        // In a real app, this would be a repository connected to a database.
        private static readonly List<Product> _products = new();

        public ProductsController()
        {
        }

        [HttpGet("{id}")]
        public ActionResult<ProductDto> GetById(Guid id)
        {
            var product = _products.Find(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            // --- USAGE OF ProjectAs ---
            // The mapper handles the complex projection, including the nested
            // Money object and the collection of Reviews.
            var productDto = product.ProjectAs<ProductDto, Product, ProductDtoMapper>();
            return Ok(productDto);
        }

        [HttpPost]
        public IActionResult CreateProduct([FromBody] CreateProductDto createDto)
        {
            // --- USAGE OF Build ---
            // The mapper uses its "zero-config" policy to find the `Product.Create`
            // factory method and constructs a valid domain entity.
            // Note: A custom policy would be needed if the DTO shape was very different.
            var newProduct = createDto.Build<CreateProductDto, Product, CreateProductMapper>();

            _products.Add(newProduct);

            return CreatedAtAction(nameof(GetById), new { id = newProduct.Id }, new ProductDtoMapper().ProjectAs(newProduct));
        }

        [HttpPut("{id}")]
        public IActionResult UpdateProduct(Guid id, [FromBody] UpdateProductDto updateDto)
        {
            var productToUpdate = _products.Find(p => p.Id == id);
            if (productToUpdate == null)
            {
                return NotFound();
            }

            // --- USAGE OF ApplyTo ---
            // The mapper applies the changes from the DTO to the existing entity.
            // The default policy will map matching properties (`Name`) and ignore others (`Id`).
            new ProductDtoMapper().ApplyTo(new ProductDto { Name = updateDto.Name }, productToUpdate);
            // In a real app, you would call _repository.SaveChanges() here.

            return NoContent();
        }
    }
}
