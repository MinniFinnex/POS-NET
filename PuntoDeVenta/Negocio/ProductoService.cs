using PuntoDeVenta.Datos;
using PuntoDeVenta.Entidades;
using System;
using System.Collections.Generic;

namespace PuntoDeVenta.Negocio
{
    public class ProductoService
    {
        private readonly ProductoRepository _repo = new();

        public List<Producto> ListarProductos() => _repo.ObtenerTodos();

        public List<Categoria> ListarCategorias() => _repo.ObtenerCategorias();

        public Producto? BuscarPorCodigo(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return null;
            return _repo.BuscarPorCodigo(codigo.Trim());
        }

        public (bool Exito, string Mensaje) Guardar(Producto producto)
        {
            if (string.IsNullOrWhiteSpace(producto.Codigo))
                return (false, "El código del producto es obligatorio.");

            if (string.IsNullOrWhiteSpace(producto.Nombre))
                return (false, "El nombre del producto es obligatorio.");

            if (producto.PrecioVenta < 0)
                return (false, "El precio de venta no puede ser negativo.");

            if (producto.Stock < 0)
                return (false, "El stock no puede ser negativo.");

            try
            {
                if (producto.IdProducto == 0)
                {
                    _repo.Insertar(producto);
                    return (true, "Producto registrado exitosamente.");
                }
                else
                {
                    _repo.Actualizar(producto);
                    return (true, "Producto actualizado exitosamente.");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error al guardar producto: {ex.Message}");
            }
        }

        public (bool Exito, string Mensaje) Eliminar(int idProducto)
        {
            try
            {
                bool eliminado = _repo.Eliminar(idProducto);
                return eliminado 
                    ? (true, "Producto eliminado correctamente.") 
                    : (false, "No se encontró el producto a eliminar.");
            }
            catch (Exception ex)
            {
                return (false, $"Error al eliminar producto: {ex.Message}");
            }
        }
    }
}
