using Microsoft.EntityFrameworkCore;
using PuntoDeVenta.Modelos;
using System.Collections.Generic;
using System.Linq;

namespace PuntoDeVenta.Datos.Repositorios
{
    public class ProductoRepositorio : IRepositorio<Producto>
    {
        public List<Producto> ObtenerTodos()
        {
            using var ctx = new PosContext();
            return ctx.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .ToList();
        }

        public Producto? BuscarPorCodigo(string codigo)
        {
            using var ctx = new PosContext();
            return ctx.Productos
                .Include(p => p.Categoria)
                .FirstOrDefault(p => p.Codigo == codigo && p.Activo);
        }

        public int Insertar(Producto p)
        {
            using var ctx = new PosContext();
            p.Activo = true;
            ctx.Productos.Add(p);
            ctx.SaveChanges();
            return p.IdProducto;
        }

        public bool Actualizar(Producto p)
        {
            using var ctx = new PosContext();
            var existing = ctx.Productos.Find(p.IdProducto);
            if (existing == null) return false;

            existing.Codigo = p.Codigo;
            existing.Nombre = p.Nombre;
            existing.Descripcion = p.Descripcion;
            existing.IdCategoria = p.IdCategoria;
            existing.PrecioCompra = p.PrecioCompra;
            existing.PrecioVenta = p.PrecioVenta;
            existing.Stock = p.Stock;
            existing.StockMinimo = p.StockMinimo;

            return ctx.SaveChanges() > 0;
        }

        public bool Eliminar(int idProducto)
        {
            using var ctx = new PosContext();
            var existing = ctx.Productos.Find(idProducto);
            if (existing == null) return false;

            existing.Activo = false;
            return ctx.SaveChanges() > 0;
        }

        public List<Categoria> ObtenerCategorias()
        {
            using var ctx = new PosContext();
            return ctx.Categorias
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .ToList();
        }

        // IRepositorio
        public Producto? Obtener(int id)
        {
            using var ctx = new PosContext();
            return ctx.Productos
                .Include(p => p.Categoria)
                .FirstOrDefault(p => p.IdProducto == id);
        }

        IEnumerable<Producto> IRepositorio<Producto>.ObtenerTodos() => ObtenerTodos();
        public void Agregar(Producto entidad) => Insertar(entidad);
        void IRepositorio<Producto>.Actualizar(Producto entidad) => Actualizar(entidad);
        void IRepositorio<Producto>.Eliminar(int id) => Eliminar(id);
    }
}
