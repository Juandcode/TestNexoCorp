using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestNexoCorp.Domain
{
    public class ExpProductos
    {
        [Key]
        public Guid IdProducto { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaVencimiento { get; set; }
        public string Observaciones { get; set; } = string.Empty;
        public Guid? IdTipoProducto { get; set; }
        public TiposProductos TiposProductos { get; set; }
        public ErpProductos? ErpProductos { get; set; }
        public ProductosCategorias ProductosCategoria { get; set; }
        public CodigosBarras CodigosBarras { get; set; }

        public VentasExpress VentasExpress { get; set; }
    }
}