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
            var productDto = mapper.Project(product);
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
    }
}
