using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestNexoCorp.Domain
{
    public class CodigosBarras
    {
        [Key]
        public Guid IdCodigoBarra { get; set; }
        
        public Guid UniqueCodigo { get; set; } = Guid.NewGuid();

        public bool Activo { get; set; } = true;
        
        public Guid? IdProducto { get; set; }
        public ExpProductos ExpProductos { get; set; }
    }
}