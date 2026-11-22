using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PuntoDeVenta.Modelos
{
    [Table("Productos")]
    public class Producto
    {
        [Key]
        public int IdProducto { get; set; }

        [Required]
        [StringLength(50)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Descripcion { get; set; }

        public int IdCategoria { get; set; }

        [ForeignKey(nameof(IdCategoria))]
        [Browsable(false)]
        public virtual Categoria? Categoria { get; set; }

        [NotMapped]
        public string? CategoriaNombre
        {
            get => Categoria?.Nombre ?? _categoriaNombre;
            set => _categoriaNombre = value;
        }
        private string? _categoriaNombre;

        [Column(TypeName = "decimal(10, 2)")]
        public decimal PrecioCompra { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal PrecioVenta { get; set; }

        public int Stock { get; set; }
        public int StockMinimo { get; set; } = 5;
        public bool Activo { get; set; } = true;

        [Browsable(false)]
        public virtual ICollection<DetalleVenta> DetalleVentas { get; set; } = new List<DetalleVenta>();
    }
}
