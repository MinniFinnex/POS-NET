using Microsoft.EntityFrameworkCore;
using PuntoDeVenta.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PuntoDeVenta.Datos.Repositorios
{
    public class CajaRepositorio : IRepositorio<Caja>
    {
        public Caja? ObtenerCajaAbierta(int? idUsuario = null)
        {
            using var ctx = new PosContext();
            var query = ctx.Cajas
                .Include(c => c.Usuario)
                .Where(c => c.Estado == "ABIERTA");

            if (idUsuario.HasValue && idUsuario.Value > 0)
            {
                query = query.Where(c => c.IdUsuario == idUsuario.Value);
            }

            var caja = query.OrderByDescending(c => c.FechaApertura).FirstOrDefault();
            if (caja != null)
            {
                caja.MontoVentas = ctx.Ventas
                    .Where(v => v.IdCaja == caja.IdCaja && v.Estado == "COMPLETADA")
                    .Sum(v => (decimal?)v.Total) ?? 0m;
            }
            return caja;
        }

        public int AbrirCaja(Caja caja)
        {
            using var ctx = new PosContext();
            caja.Estado = "ABIERTA";
            caja.MontoVentas = 0m;
            if (caja.FechaApertura == default)
                caja.FechaApertura = DateTime.Now;

            ctx.Cajas.Add(caja);
            ctx.SaveChanges();
            return caja.IdCaja;
        }

        public decimal ObtenerTotalVentasCaja(int idCaja)
        {
            using var ctx = new PosContext();
            return ctx.Ventas
                .Where(v => v.IdCaja == idCaja && v.Estado == "COMPLETADA")
                .Sum(v => (decimal?)v.Total) ?? 0m;
        }

        public bool CerrarCaja(int idCaja, decimal montoCierre, string observaciones)
        {
            using var ctx = new PosContext();
            var caja = ctx.Cajas.Find(idCaja);
            if (caja == null) return false;

            caja.FechaCierre = DateTime.Now;
            caja.MontoCierre = montoCierre;
            caja.MontoVentas = ctx.Ventas
                .Where(v => v.IdCaja == idCaja && v.Estado == "COMPLETADA")
                .Sum(v => (decimal?)v.Total) ?? 0m;

            caja.Diferencia = montoCierre - (caja.MontoInicial + caja.MontoVentas);
            caja.Observaciones = observaciones;
            caja.Estado = "CERRADA";

            return ctx.SaveChanges() > 0;
        }

        public List<Caja> ObtenerHistorialCajas()
        {
            using var ctx = new PosContext();
            return ctx.Cajas
                .Include(c => c.Usuario)
                .OrderByDescending(c => c.FechaApertura)
                .ToList();
        }

        // IRepositorio
        public Caja? Obtener(int id)
        {
            using var ctx = new PosContext();
            return ctx.Cajas.Include(c => c.Usuario).FirstOrDefault(c => c.IdCaja == id);
        }

        public IEnumerable<Caja> ObtenerTodos() => ObtenerHistorialCajas();
        public void Agregar(Caja entidad) => AbrirCaja(entidad);
        public void Actualizar(Caja entidad)
        {
            using var ctx = new PosContext();
            ctx.Cajas.Update(entidad);
            ctx.SaveChanges();
        }

        public void Eliminar(int id)
        {
            using var ctx = new PosContext();
            var existing = ctx.Cajas.Find(id);
            if (existing != null)
            {
                ctx.Cajas.Remove(existing);
                ctx.SaveChanges();
            }
        }
    }
}
