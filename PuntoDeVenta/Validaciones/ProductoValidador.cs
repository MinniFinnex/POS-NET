using PuntoDeVenta.Comun.Excepciones;
using PuntoDeVenta.Modelos;

namespace PuntoDeVenta.Validaciones
{
    public static class ProductoValidador
    {
        public static void Validar(Producto producto)
        {
            if (producto == null)
                throw new NegocioException("Los datos del producto son nulos.");

            if (string.IsNullOrWhiteSpace(producto.Codigo))
                throw new NegocioException("El código del producto es obligatorio.");

            if (string.IsNullOrWhiteSpace(producto.Nombre))
                throw new NegocioException("El nombre del producto es obligatorio.");

            if (producto.IdCategoria <= 0)
                throw new NegocioException("Debe seleccionar una categoría válida.");

            if (producto.PrecioCompra < 0)
                throw new NegocioException("El precio de compra no puede ser negativo.");

            if (producto.PrecioVenta < 0)
                throw new NegocioException("El precio de venta no puede ser negativo.");

            if (producto.Stock < 0)
                throw new NegocioException("El stock no puede ser negativo.");

            if (producto.StockMinimo < 0)
                throw new NegocioException("El stock mínimo no puede ser negativo.");
        }
    }
}
