using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PuntoDeVenta.Modelos
{
    [Table("Cajas")]
    public class Caja
    {
        [Key]
        public int IdCaja { get; set; }

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

        public DateTime FechaApertura { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(12, 2)")]
        public decimal MontoInicial { get; set; }

        public DateTime? FechaCierre { get; set; }

        [Column(TypeName = "decimal(12, 2)")]
        public decimal MontoVentas { get; set; }

        [Column(TypeName = "decimal(12, 2)")]
        public decimal? MontoCierre { get; set; }

        [Column(TypeName = "decimal(12, 2)")]
        public decimal? Diferencia { get; set; }

        [StringLength(300)]
        public string? Observaciones { get; set; }

        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "ABIERTA";

        [NotMapped]
        public decimal TotalEsperado => MontoInicial + MontoVentas;

        [Browsable(false)]
        public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    }
}
