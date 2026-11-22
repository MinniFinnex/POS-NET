using PuntoDeVenta.Comun.Excepciones;
using PuntoDeVenta.Datos.Repositorios;
using PuntoDeVenta.Modelos;
using PuntoDeVenta.Validaciones;
using System;
using System.Collections.Generic;

namespace PuntoDeVenta.Servicios
{
    public class ProductoServicio
    {
        private readonly ProductoRepositorio _repo = new();

        public List<Producto> ListarProductos() => _repo.ObtenerTodos();

        public List<Categoria> ListarCategorias() => _repo.ObtenerCategorias();

        public Producto? BuscarPorCodigo(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return null;
            return _repo.BuscarPorCodigo(codigo.Trim());
        }

        public (bool Exito, string Mensaje) Guardar(Producto producto)
        {
            try
            {
                ProductoValidador.Validar(producto);

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
            catch (NegocioException ex)
            {
                return (false, ex.Message);
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
