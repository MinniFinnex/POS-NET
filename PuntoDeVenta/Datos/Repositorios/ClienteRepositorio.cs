using Microsoft.EntityFrameworkCore;
using PuntoDeVenta.Modelos;
using System.Collections.Generic;
using System.Linq;

namespace PuntoDeVenta.Datos.Repositorios
{
    public class ClienteRepositorio : IRepositorio<Cliente>
    {
        public List<Cliente> ObtenerTodos()
        {
            using var ctx = new PosContext();
            return ctx.Clientes
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .ToList();
        }

        public Cliente? BuscarPorDocumento(string documento)
        {
            using var ctx = new PosContext();
            return ctx.Clientes
                .FirstOrDefault(c => c.Documento == documento && c.Activo);
        }

        public int Insertar(Cliente c)
        {
            using var ctx = new PosContext();
            c.Activo = true;
            ctx.Clientes.Add(c);
            ctx.SaveChanges();
            return c.IdCliente;
        }

        public bool Actualizar(Cliente c)
        {
            using var ctx = new PosContext();
            var existing = ctx.Clientes.Find(c.IdCliente);
            if (existing == null) return false;

            existing.Documento = c.Documento;
            existing.Nombre = c.Nombre;
            existing.Telefono = c.Telefono;
            existing.Email = c.Email;
            existing.Direccion = c.Direccion;

            return ctx.SaveChanges() > 0;
        }

        public bool Eliminar(int idCliente)
        {
            using var ctx = new PosContext();
            var existing = ctx.Clientes.Find(idCliente);
            if (existing == null) return false;

            existing.Activo = false;
            return ctx.SaveChanges() > 0;
        }

        // IRepositorio
        public Cliente? Obtener(int id)
        {
            using var ctx = new PosContext();
            return ctx.Clientes.FirstOrDefault(c => c.IdCliente == id);
        }

        IEnumerable<Cliente> IRepositorio<Cliente>.ObtenerTodos() => ObtenerTodos();
        public void Agregar(Cliente entidad) => Insertar(entidad);
        void IRepositorio<Cliente>.Actualizar(Cliente entidad) => Actualizar(entidad);
        void IRepositorio<Cliente>.Eliminar(int id) => Eliminar(id);
    }
}
