


using PuntoDeVenta.Datos;
using PuntoDeVenta.Entidades;
using System;
using System.Collections.Generic;

namespace PuntoDeVenta.Negocio
{
    public class VentaService
    {
        private readonly VentaRepository _repo = new();

        public (bool Exito, string Mensaje, int IdVenta) ProcesarVenta(Venta venta, List<DetalleVenta> detalles)
        {
            if (detalles == null || detalles.Count == 0)
                return (false, "El carrito de ventas no contiene ningún producto.", 0);

            if (venta.IdUsuario <= 0)
                return (false, "Se requiere un usuario válido para registrar la venta.", 0);

            try
            {
                int idVenta = _repo.RegistrarVenta(venta, detalles);
                return (true, "Venta registrada con éxito.", idVenta);
            }
            catch (Exception ex)
            {
                return (false, $"Error al registrar la venta: {ex.Message}", 0);
            }
        }

        public List<Venta> HistorialVentas() => _repo.ObtenerHistorialVentas();

        public List<Venta> HistorialVentasFiltrado(DateTime inicio, DateTime fin, int? idUsuario = null)
        {
            return _repo.ObtenerHistorialVentasFiltrado(inicio, fin, idUsuario);
        }

        public List<DetalleVenta> ObtenerDetallesPorVenta(int idVenta)
        {
            return _repo.ObtenerDetallesPorVenta(idVenta);
        }

        public List<ProductoVendidoDTO> ObtenerTopProductos(DateTime inicio, DateTime fin, int top = 5)
        {
            return _repo.ObtenerTopProductos(inicio, fin, top);
        }

        public ResumenVentasDTO ObtenerResumen(DateTime inicio, DateTime fin, int? idUsuario = null)
        {
            return _repo.ObtenerResumen(inicio, fin, idUsuario);
        }
    }
}
