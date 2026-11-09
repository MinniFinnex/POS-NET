using PuntoDeVenta.Datos;
using PuntoDeVenta.Entidades;
using System;
using System.Collections.Generic;

namespace PuntoDeVenta.Negocio
{
    public class CajaService
    {
        private readonly CajaRepository _repo = new();

        public Caja? ObtenerCajaAbierta(int? idUsuario = null)
        {
            return _repo.ObtenerCajaAbierta(idUsuario);
        }

        public (bool Exito, string Mensaje, int IdCaja) AbrirCaja(int idUsuario, decimal montoInicial, string? observaciones = null)
        {
            if (montoInicial < 0)
                return (false, "El monto inicial no puede ser negativo.", 0);

            var cajaActual = _repo.ObtenerCajaAbierta(idUsuario);
            if (cajaActual != null)
                return (false, $"Ya existe una caja abierta iniciada el {cajaActual.FechaApertura:dd/MM/yyyy HH:mm}.", cajaActual.IdCaja);

            try
            {
                var nuevaCaja = new Caja
                {
                    IdUsuario = idUsuario,
                    FechaApertura = DateTime.Now,
                    MontoInicial = montoInicial,
                    Observaciones = observaciones,
                    Estado = "ABIERTA"
                };

                int id = _repo.AbrirCaja(nuevaCaja);
                return (true, "Apertura de caja realizada con éxito.", id);
            }
            catch (Exception ex)
            {
                return (false, $"Error al abrir caja: {ex.Message}", 0);
            }
        }

        public (bool Exito, string Mensaje) CerrarCaja(int idCaja, decimal montoCierre, string observaciones)
        {
            if (idCaja <= 0)
                return (false, "Identificador de caja inválido.");

            if (montoCierre < 0)
                return (false, "El monto de cierre en efectivo no puede ser negativo.");

            try
            {
                bool ok = _repo.CerrarCaja(idCaja, montoCierre, observaciones);
                return ok ? (true, "Cierre de caja completado con éxito.") : (false, "No se pudo cerrar la caja seleccionada.");
            }
            catch (Exception ex)
            {
                return (false, $"Error al cerrar caja: {ex.Message}");
            }
        }

        public decimal ObtenerTotalVentasCaja(int idCaja)
        {
            return _repo.ObtenerTotalVentasCaja(idCaja);
        }

        public List<Caja> ListarHistorialCajas()
        {
            return _repo.ObtenerHistorialCajas();
        }
    }
}
