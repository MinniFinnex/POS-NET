using Microsoft.EntityFrameworkCore;
using PuntoDeVenta.Modelos;
using System.Collections.Generic;
using System.Linq;

namespace PuntoDeVenta.Datos.Repositorios
{
    public class UsuarioRepositorio : IRepositorio<Usuario>
    {
        public Usuario? ValidarLogin(string usuario, string password)
        {
            using var ctx = new PosContext();
            return ctx.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefault(u => u.NombreUsuario == usuario && u.Password == password && u.Activo);
        }

        public List<Usuario> ObtenerTodos()
        {
            using var ctx = new PosContext();
            return ctx.Usuarios
                .Include(u => u.Rol)
                .Where(u => u.Activo)
                .OrderBy(u => u.NombreCompleto)
                .ToList();
        }

        // IRepositorio
        public Usuario? Obtener(int id)
        {
            using var ctx = new PosContext();
            return ctx.Usuarios.Include(u => u.Rol).FirstOrDefault(u => u.IdUsuario == id);
        }

        IEnumerable<Usuario> IRepositorio<Usuario>.ObtenerTodos() => ObtenerTodos();
        public void Agregar(Usuario entidad)
        {
            using var ctx = new PosContext();
            ctx.Usuarios.Add(entidad);
            ctx.SaveChanges();
        }

        public void Actualizar(Usuario entidad)
        {
            using var ctx = new PosContext();
            ctx.Usuarios.Update(entidad);
            ctx.SaveChanges();
        }

        public void Eliminar(int id)
        {
            using var ctx = new PosContext();
            var existing = ctx.Usuarios.Find(id);
            if (existing != null)
            {
                existing.Activo = false;
                ctx.SaveChanges();
            }
        }
    }
}
