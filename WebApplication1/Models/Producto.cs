
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Producto
    {

        public int Id { get; set; }

        [Required]
        [MaxLength]
        public string Nombre { get; set; } = string.Empty;


        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        public int Stock { get; set; }

        public int CategoriaId { get; set; }

        public bool Activo { get; set; }

    }
}
