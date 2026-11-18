using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestNexoCorp.Domain
{
    public class Categorias
    {
        [Key]
        public Guid IdCategoria { get; set; }
        public String Descripcion { get; set; }
        public bool Activo { get; set; } = true;
        public ProductosCategorias ProductosCategoria { get; set; }
        
        public Guid? IdCategoriaPadre { get; set; }
        public Categorias CategoriaPadre { get; set; }
    }
}