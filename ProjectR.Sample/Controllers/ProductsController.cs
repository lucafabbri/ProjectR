using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using ProjectR.Sample.Domain;
using ProjectR.Sample.Application.DTOs;
using ProjectR.Sample.Application.Mappers;
using ProjectR.Services;

namespace ProjectR.Sample.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {

        // In a real app, this would be a repository connected to a database.
        private static readonly List<Product> _products = new();
        private readonly IMapperResolver _mapperResolver;

        public ProductsController(IMapperResolver mapperResolver)
        {
            _mapperResolver = mapperResolver;
        }

        [HttpGet("{id}")]
        public ActionResult<ProductDto> GetById(Guid id)
        {
            var product = _products.Find(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            var mapper = _mapperResolver.GetMapper<Product, ProductDto>();
            var productDto = product.Project<ProductDto, Product, ProductDtoMapper>();
            return Ok(productDto);
        }

        [HttpPost]
        public IActionResult CreateProduct([FromBody] CreateProductDto createDto)
        {
            var newProduct = _mapperResolver
                .GetMapper<Product, CreateProductDto>()
                .Build(createDto);

            _products.Add(newProduct);

            return CreatedAtAction(nameof(GetById), new { id = newProduct.Id }, _mapperResolver.GetMapper<Product, ProductDto>().Project(newProduct));
        }

        [HttpPut("{id}")]
        public IActionResult UpdateProduct(Guid id, [FromBody] UpdateProductDto updateDto)
        {
            var productToUpdate = _products.Find(p => p.Id == id);
            if (productToUpdate == null)
            {
                return NotFound();
            }

            _mapperResolver.GetMapper<Product, UpdateProductDto>().Apply(updateDto, productToUpdate);

            _products.RemoveAll(p => p.Id == id);
            _products.Add(productToUpdate);

            return NoContent();
        }
    }
}
