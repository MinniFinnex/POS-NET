using System;

namespace PuntoDeVenta.DTOs
{
    public class VentaResumenDto
    {
        public int IdVenta { get; set; }
        public DateTime Fecha { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public string Cajero { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public int Lineas { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
