using Microsoft.AspNetCore.Mvc;
using CobaWebAPI.Models;
using CobaWebAPI.Services;
using CobaWebAPI.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace CobaWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _service;

        public ProductsController(IProductService service)
        {
            _service = service;
        }

        
        [HttpGet]
        [Authorize]
        public IActionResult GetAll()
        {
            
            return Ok(_service.GetAll());
        }

        [HttpGet("{id}")]
        [Authorize]
        public IActionResult GetById(int id)
        {
            var product = _service.GetById(id);
            if (product == null)
                return NotFound();

            return Ok(product);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Create(ProductCreateDto dto)
        {
            // if (string.IsNullOrEmpty(product.Name))
            //   return BadRequest("Name is required");

            var product = new Product
            {
                Name = dto.Name,
                Price = dto.Price
            };
            var created = _service.Add(product);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Update(int id, ProductUpdateDto dto)
        {
            /*var success = _service.Update(id, product);
            if (!success)
                return NotFound();

            return NoContent();*/
            var existing = _service.GetById(id);
            if (existing == null)
                return NotFound();

            existing.Name = dto.Name;
            existing.Price = dto.Price;

            _service.Update(id, existing);

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            var success = _service.Delete(id);
            if (!success)
                return NotFound();

            return NoContent();
        }
        [HttpPatch("{id}")]
        [Authorize]
        public IActionResult Patch(int id, ProductPatchDto dto)
        {
            var product = _service.GetById(id);
            if (product == null)
                return NotFound();

            if (dto.Name != null)
                product.Name = dto.Name;

            if (dto.Price.HasValue)
                product.Price = dto.Price.Value;

            _service.Update(id, product);

            return NoContent();
        }

    }
}