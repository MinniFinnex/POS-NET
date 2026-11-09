using System;

namespace PuntoDeVenta.Entidades
{
    public class ProductoVendidoDTO
    {
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal TotalRecaudado { get; set; }
    }

    public class ResumenVentasDTO
    {
        public decimal TotalVendido { get; set; }
        public int TotalTransacciones { get; set; }
        public int TotalArticulos { get; set; }
        public decimal TicketPromedio => TotalTransacciones > 0 ? TotalVendido / TotalTransacciones : 0;
    }
}
