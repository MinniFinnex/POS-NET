using System;

namespace PuntoDeVenta.Entidades
{
    public class Caja
    {
        public int IdCaja { get; set; }
        public int IdUsuario { get; set; }
        public string? UsuarioNombre { get; set; }
        public DateTime FechaApertura { get; set; } = DateTime.Now;
        public decimal MontoInicial { get; set; }
        public DateTime? FechaCierre { get; set; }
        public decimal MontoVentas { get; set; }
        public decimal? MontoCierre { get; set; }
        public decimal? Diferencia { get; set; }
        public string? Observaciones { get; set; }
        public string Estado { get; set; } = "ABIERTA"; // ABIERTA, CERRADA

        public decimal TotalEsperado => MontoInicial + MontoVentas;
    }
}
