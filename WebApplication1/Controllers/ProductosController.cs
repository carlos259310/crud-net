using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/productos
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var productos = await _context.Productos
                .AsNoTracking()
                .ToListAsync();

            return Ok(productos);
        }

        // GET: api/productos/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest("Id inválido");

            var producto = await _context.Productos
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (producto == null)
                return NotFound();

            return Ok(producto);
        }

        // POST: api/productos
        [HttpPost]
        public async Task<IActionResult> Create(Producto producto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var categoriaExiste = await _context.Categorias
                    .AnyAsync(x => x.Id == producto.CategoriaId);

                if (!categoriaExiste)
                    return BadRequest("La categoría no existe");

                _context.Productos.Add(producto);

                await _context.SaveChangesAsync();

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = producto.Id },
                    producto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // PUT: api/productos/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Producto model)
        {
            if (id <= 0)
                return BadRequest("Id inválido");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
                return NotFound();

            var categoriaExiste = await _context.Categorias
                .AnyAsync(x => x.Id == model.CategoriaId);

            if (!categoriaExiste)
                return BadRequest("La categoría no existe");

            producto.Nombre = model.Nombre;
            producto.Descripcion = model.Descripcion;
            producto.Precio = model.Precio;
            producto.Stock = model.Stock;
            producto.CategoriaId = model.CategoriaId;
            producto.Activo = model.Activo;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/productos/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest("Id inválido");

            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
                return NotFound();

            _context.Productos.Remove(producto);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}