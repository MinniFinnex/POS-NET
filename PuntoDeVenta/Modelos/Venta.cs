using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PuntoDeVenta.Modelos
{
    [Table("Ventas")]
    public class Venta
    {
        [Key]
        public int IdVenta { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public int? IdCliente { get; set; }

        [ForeignKey(nameof(IdCliente))]
        [Browsable(false)]
        public virtual Cliente? Cliente { get; set; }

        [NotMapped]
        public string? ClienteNombre
        {
            get => Cliente?.Nombre ?? _clienteNombre;
            set => _clienteNombre = value;
        }
        private string? _clienteNombre;

        public int IdUsuario { get; set; }

        [ForeignKey(nameof(IdUsuario))]
        [Browsable(false)]
        public virtual Usuario? Usuario { get; set; }

        [NotMapped]
        public string? UsuarioNombre
        {
            get => Usuario?.NombreCompleto ?? _usuarioNombre;
            set => _usuarioNombre = value;
        }
        private string? _usuarioNombre;

        public int? IdCaja { get; set; }

        [ForeignKey(nameof(IdCaja))]
        [Browsable(false)]
        public virtual Caja? Caja { get; set; }

        [Column(TypeName = "decimal(12, 2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(12, 2)")]
        public decimal Impuesto { get; set; }

        [Column(TypeName = "decimal(12, 2)")]
        public decimal Total { get; set; }

        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "COMPLETADA";

        [Browsable(false)]
        public virtual ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    }
}
