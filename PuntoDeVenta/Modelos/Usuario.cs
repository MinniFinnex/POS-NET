using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PuntoDeVenta.Modelos
{
    [Table("Usuarios")]
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }

        [Required]
        [StringLength(100)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("Usuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;

        public int IdRol { get; set; }

        [ForeignKey(nameof(IdRol))]
        [Browsable(false)]
        public virtual Rol? Rol { get; set; }

        [NotMapped]
        public string? RolNombre
        {
            get => Rol?.Nombre ?? _rolNombre;
            set => _rolNombre = value;
        }
        private string? _rolNombre;

        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Browsable(false)]
        public virtual ICollection<Caja> Cajas { get; set; } = new List<Caja>();

        [Browsable(false)]
        public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    }
}
