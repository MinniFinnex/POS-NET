using Microsoft.EntityFrameworkCore;
using PuntoDeVenta.DTOs;
using PuntoDeVenta.Modelos;
using PuntoDeVenta.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PuntoDeVenta.Datos.Repositorios
{
    public class VentaRepositorio : IRepositorio<Venta>
    {
        public int RegistrarVenta(Venta venta, IList<DetalleVenta> detalles)
        {
            using var ctx = new PosContext();
            using var tx = ctx.Database.BeginTransaction();

            try
            {
                // Validar existencias y descontar inventario de cada producto
                foreach (var d in detalles)
                {
                    var prod = ctx.Productos.Find(d.IdProducto)
                        ?? throw new InvalidOperationException($"El producto con ID {d.IdProducto} no existe.");

                    if (prod.Stock < d.Cantidad)
                    {
                        throw new InvalidOperationException(
                            $"Stock insuficiente para '{prod.Nombre}'. Disponible: {prod.Stock}, Solicitado: {d.Cantidad}.");
                    }

                    prod.Stock -= d.Cantidad;
                }

                //Insertar cabecera de la venta
                if (string.IsNullOrWhiteSpace(venta.Estado))
                    venta.Estado = "COMPLETADA";

                if (venta.Fecha == default)
                    venta.Fecha = DateTime.Now;

                ctx.Ventas.Add(venta);
                ctx.SaveChanges();

                //  Insertar DetalleVentas vinculados a la venta generada
                foreach (var d in detalles)
                {
                    d.IdVenta = venta.IdVenta;
                    ctx.DetalleVentas.Add(d);
                }
                ctx.SaveChanges();

                tx.Commit();
                return venta.IdVenta;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public List<Venta> ObtenerHistorialVentas()
        {
            return ObtenerHistorialVentasFiltrado(DateTime.MinValue, DateTime.MaxValue, null);
        }

        public List<Venta> ObtenerHistorialVentasFiltrado(DateTime fechaInicio, DateTime fechaFin, int? idUsuario = null)
        {
            using var ctx = new PosContext();
            var query = ctx.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Usuario)
                .AsQueryable();

            if (fechaInicio > DateTime.MinValue)
                query = query.Where(v => v.Fecha >= fechaInicio);

            if (fechaFin < DateTime.MaxValue)
                query = query.Where(v => v.Fecha <= fechaFin);

            if (idUsuario.HasValue && idUsuario.Value > 0)
                query = query.Where(v => v.IdUsuario == idUsuario.Value);

            return query.OrderByDescending(v => v.Fecha).ToList();
        }

        public IList<VentaResumenDto> ObtenerHistorialResumen(DateTime desde, DateTime hasta)
        {
            using var ctx = new PosContext();
            return ctx.Ventas
                .Where(v => v.Fecha >= desde && v.Fecha <= hasta)
                .Select(v => new VentaResumenDto
                {
                    IdVenta = v.IdVenta,
                    Fecha = v.Fecha,
                    Cliente = v.Cliente != null ? v.Cliente.Nombre : "Consumidor Final",
                    Cajero = v.Usuario != null ? v.Usuario.NombreCompleto : "General",
                    Total = v.Total,
                    Lineas = v.Detalles.Count(),
                    Estado = v.Estado
                })
                .OrderByDescending(v => v.Fecha)
                .ToList();
        }

        public List<DetalleVenta> ObtenerDetallesPorVenta(int idVenta)
        {
            using var ctx = new PosContext();
            return ctx.DetalleVentas
                .Include(d => d.Producto)
                .Where(d => d.IdVenta == idVenta)
                .ToList();
        }

        public List<ProductoVendidoDTO> ObtenerTopProductos(DateTime fechaInicio, DateTime fechaFin, int top = 5)
        {
            using var ctx = new PosContext();
            return ctx.DetalleVentas
                .Include(d => d.Producto)
                .Include(d => d.Venta)
                .Where(d => d.Venta != null && d.Venta.Fecha >= fechaInicio && d.Venta.Fecha <= fechaFin && d.Venta.Estado == "COMPLETADA")
                .GroupBy(d => new { d.IdProducto, d.Producto!.Nombre })
                .Select(g => new ProductoVendidoDTO
                {
                    IdProducto = g.Key.IdProducto,
                    Nombre = g.Key.Nombre,
                    CantidadVendida = g.Sum(x => x.Cantidad),
                    TotalRecaudado = g.Sum(x => x.Subtotal)
                })
                .OrderByDescending(x => x.CantidadVendida)
                .Take(top)
                .ToList();
        }

        public ResumenVentasDTO ObtenerResumen(DateTime fechaInicio, DateTime fechaFin, int? idUsuario = null)
        {
            using var ctx = new PosContext();
            var queryVentas = ctx.Ventas
                .Where(v => v.Fecha >= fechaInicio && v.Fecha <= fechaFin && v.Estado == "COMPLETADA");

            if (idUsuario.HasValue && idUsuario.Value > 0)
                queryVentas = queryVentas.Where(v => v.IdUsuario == idUsuario.Value);

            var ventas = queryVentas.Select(v => new { v.IdVenta, v.Total }).ToList();
            var idsVentas = ventas.Select(v => v.IdVenta).ToList();

            int totalArticulos = 0;
            if (idsVentas.Count > 0)
            {
                totalArticulos = ctx.DetalleVentas
                    .Where(d => idsVentas.Contains(d.IdVenta))
                    .Sum(d => (int?)d.Cantidad) ?? 0;
            }

            return new ResumenVentasDTO
            {
                TotalVendido = ventas.Sum(v => v.Total),
                TotalTransacciones = ventas.Count,
                TotalArticulos = totalArticulos
            };
        }

        // IRepositorio
        public Venta? Obtener(int id)
        {
            using var ctx = new PosContext();
            return ctx.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Usuario)
                .Include(v => v.Detalles)
                .FirstOrDefault(v => v.IdVenta == id);
        }

        IEnumerable<Venta> IRepositorio<Venta>.ObtenerTodos() => ObtenerHistorialVentas();
        public void Agregar(Venta entidad)
        {
            using var ctx = new PosContext();
            ctx.Ventas.Add(entidad);
            ctx.SaveChanges();
        }

        void IRepositorio<Venta>.Actualizar(Venta entidad)
        {
            using var ctx = new PosContext();
            ctx.Ventas.Update(entidad);
            ctx.SaveChanges();
        }

        void IRepositorio<Venta>.Eliminar(int id)
        {
            using var ctx = new PosContext();
            var existing = ctx.Ventas.Find(id);
            if (existing != null)
            {
                existing.Estado = "ANULADA";
                ctx.SaveChanges();
            }
        }
    }
}
