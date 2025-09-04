using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantApi.Infrastructure;
using PlantApi.Infrastructure.Entities;
using PlantApi.Models;

namespace PlantApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlantsController : ControllerBase
    {
        private readonly RelationalDbContext _db;

        public PlantsController(RelationalDbContext db)
        {
            _db = db;
        }

        // /api/plants
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entities = await _db.Plants.AsNoTracking().ToListAsync();
            var result = entities.Select(e => new Plant
            {
                Id = e.Id,
                Name = e.Name,
                ScientificName = e.ScientificName,
                Family = e.Family,
                Data = null // si tu modelo Data existe, mapearlo aquí; por ahora devuelvo null
            }).ToList();
            return Ok(result);
        }

        // /api/plants/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var e = await _db.Plants.FindAsync(id);
            if (e == null) return NotFound();
            var model = new Plant
            {
                Id = e.Id,
                Name = e.Name,
                ScientificName = e.ScientificName,
                Family = e.Family,
                Data = null
            };
            return Ok(model);
        }

        //  /api/plants
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Plant input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.Name))
                return BadRequest("Name is required.");

            var entity = new PlantEntity
            {
                Id = Guid.NewGuid(),
                Name = input.Name,
                ScientificName = input.ScientificName ?? string.Empty,
                Family = input.Family ?? string.Empty,
                MaxHeight = 0,
                MaxAge = 0,
                ConservationLevel = 0
            };

            _db.Plants.Add(entity);
            await _db.SaveChangesAsync();

            input.Id = entity.Id;
            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, input);
        }

        //  /api/plants/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var e = await _db.Plants.FindAsync(id);
            if (e == null) return NotFound();

            _db.Plants.Remove(e);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
