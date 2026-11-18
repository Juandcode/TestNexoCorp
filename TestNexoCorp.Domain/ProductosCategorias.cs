using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestNexoCorp.Domain
{
    public class ProductosCategorias
    {
        [Key]
        public Guid IdDetalle { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public Guid? IdProducto { get; set; }
        public ExpProductos ExpProductos { get; set; }

        public Guid? IdCategoria { get; set; }
        public Categorias Categorias { get; set; }
    }
}