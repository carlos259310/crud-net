using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TiposDocumentosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TiposDocumentosController(AppDbContext context)
        {
            _context = context;
        }


        // GET: api/tiposdocumentos
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            //devuelve todo 
            var tiposdocumentos = await _context.TiposDocumentos
                .AsNoTracking()
                .ToListAsync();

            return Ok(tiposdocumentos);
        }


        // GET: api/tiposdocumentos/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest("Id inválido");

            var tipodocumento = await _context.TiposDocumentos
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (tipodocumento == null)
                return NotFound();

            return Ok(tipodocumento);
        }


        // POST: api/tiposdocumentos
        [HttpPost]
        public async Task<IActionResult> Create(TipoDocumento tipodocumento)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var existeCodigo = await _context.TiposDocumentos
                    .AnyAsync(x => x.Codigo == tipodocumento.Codigo);

                if (existeCodigo)
                    return BadRequest("El código ya existe");

                _context.TiposDocumentos.Add(tipodocumento);

                await _context.SaveChangesAsync();

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = tipodocumento.Id },
                    tipodocumento);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        // PUT: api/tiposdocumentos/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TipoDocumento model)
        {
            if (id <= 0)
                return BadRequest("Id inválido");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tipoDocumento = await _context.TiposDocumentos
                .FindAsync(id);

            if (tipoDocumento == null)
                return NotFound();

            var existeCodigo = await _context.TiposDocumentos
                .AnyAsync(x =>
                    x.Codigo == model.Codigo &&
                    x.Id != id);

            if (existeCodigo)
                return BadRequest("El código ya existe");

            tipoDocumento.Codigo = model.Codigo;
            tipoDocumento.Nombre = model.Nombre;

            await _context.SaveChangesAsync();

            return NoContent();
        }



        // DELETE: api/tiposdocumentos/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest("Id inválido");

            var tipoDocumento = await _context.TiposDocumentos
                .FindAsync(id);

            if (tipoDocumento == null)
                return NotFound();

            _context.TiposDocumentos.Remove(tipoDocumento);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    
}