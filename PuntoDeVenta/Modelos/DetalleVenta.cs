using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PuntoDeVenta.Modelos
{
    [Table("DetalleVentas")]
    public class DetalleVenta
    {
        [Key]
        public int IdDetalle { get; set; }

        public int IdVenta { get; set; }

        [ForeignKey(nameof(IdVenta))]
        [Browsable(false)]
        public virtual Venta? Venta { get; set; }

        public int IdProducto { get; set; }

        [ForeignKey(nameof(IdProducto))]
        [Browsable(false)]
        public virtual Producto? Producto { get; set; }

        [NotMapped]
        public string? ProductoNombre
        {
            get => Producto?.Nombre ?? _productoNombre;
            set => _productoNombre = value;
        }
        private string? _productoNombre;

        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal PrecioUnitario { get; set; }

        [Column(TypeName = "decimal(12, 2)")]
        public decimal Subtotal { get; set; }
    }
}
