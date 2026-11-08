using System;

namespace PuntoDeVenta.Entidades
{
    public class Venta
    {
        public int IdVenta { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public int? IdCliente { get; set; }
        public string? ClienteNombre { get; set; }
        public int IdUsuario { get; set; }
        public string? UsuarioNombre { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = "COMPLETADA";
    }
}
