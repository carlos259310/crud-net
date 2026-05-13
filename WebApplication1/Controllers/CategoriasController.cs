using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/categorias
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categorias = await _context.Categorias
                .AsNoTracking()
                .ToListAsync();

            return Ok(categorias);
        }

        // GET: api/categorias/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest("Id inválido");

            var categoria = await _context.Categorias
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (categoria == null)
                return NotFound();

            return Ok(categoria);
        }

        // POST: api/categorias
        [HttpPost]
        public async Task<IActionResult> Create(Categoria categoria)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _context.Categorias.Add(categoria);

                await _context.SaveChangesAsync();

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = categoria.Id },
                    categoria);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // PUT: api/categorias/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Categoria model)
        {
            if (id <= 0)
                return BadRequest("Id inválido");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria == null)
                return NotFound();

            categoria.Nombre = model.Nombre;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/categorias/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest("Id inválido");

            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria == null)
                return NotFound();

            _context.Categorias.Remove(categoria);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}