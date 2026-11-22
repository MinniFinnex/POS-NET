using PuntoDeVenta.Comun.Excepciones;
using PuntoDeVenta.Datos.Repositorios;
using PuntoDeVenta.DTOs;
using PuntoDeVenta.Modelos;
using PuntoDeVenta.Modelos;
using PuntoDeVenta.Validaciones;
using System;
using System.Collections.Generic;

namespace PuntoDeVenta.Servicios
{
    public class VentaServicio
    {
        private readonly VentaRepositorio _repo = new();

        public (bool Exito, string Mensaje, int IdVenta) ProcesarVenta(Venta venta, IList<DetalleVenta> detalles)
        {
            try
            {
                VentaValidador.Validar(venta, detalles);

                // Calcular totales centralizadamente (regla de negocio vigente)
                decimal total = 0;
                foreach (var d in detalles)
                {
                    d.Subtotal = d.Cantidad * d.PrecioUnitario;
                    total += d.Subtotal;
                }
                venta.Subtotal = total;
                venta.Impuesto = 0m; // Precios netos con impuesto incluido
                venta.Total = total;

                int idVenta = _repo.RegistrarVenta(venta, detalles);
                return (true, "Venta registrada con éxito.", idVenta);
            }
            catch (NegocioException ex)
            {
                return (false, ex.Message, 0);
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

        public IList<VentaResumenDto> HistorialResumen(DateTime desde, DateTime hasta)
        {
            return _repo.ObtenerHistorialResumen(desde, hasta);
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
