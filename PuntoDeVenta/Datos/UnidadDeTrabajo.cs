using Microsoft.EntityFrameworkCore.Storage;
using PuntoDeVenta.Datos.Repositorios;
using PuntoDeVenta.Modelos;
using System;

namespace PuntoDeVenta.Datos
{
    public class UnidadDeTrabajo : IDisposable
    {
        private readonly PosContext _context;
        private IDbContextTransaction? _transaccion;

        public ProductoRepositorio Productos { get; }
        public ClienteRepositorio Clientes { get; }
        public VentaRepositorio Ventas { get; }
        public CajaRepositorio Cajas { get; }
        public UsuarioRepositorio Usuarios { get; }

        public UnidadDeTrabajo()
        {
            _context = new PosContext();
            Productos = new ProductoRepositorio();
            Clientes = new ClienteRepositorio();
            Ventas = new VentaRepositorio();
            Cajas = new CajaRepositorio();
            Usuarios = new UsuarioRepositorio();
        }

        public PosContext Contexto => _context;

        public void IniciarTransaccion()
        {
            _transaccion = _context.Database.BeginTransaction();
        }

        public void ConfirmarTransaccion()
        {
            try
            {
                _context.SaveChanges();
                _transaccion?.Commit();
            }
            finally
            {
                _transaccion?.Dispose();
                _transaccion = null;
            }
        }

        public void RevertirTransaccion()
        {
            try
            {
                _transaccion?.Rollback();
            }
            finally
            {
                _transaccion?.Dispose();
                _transaccion = null;
            }
        }

        public int GuardarCambios() => _context.SaveChanges();

        public void Dispose()
        {
            _transaccion?.Dispose();
            _context.Dispose();
        }
    }
}
