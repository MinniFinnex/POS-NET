using PuntoDeVenta.Comun.Excepciones;
using PuntoDeVenta.Modelos;
using System.Collections.Generic;

namespace PuntoDeVenta.Validaciones
{
    public static class VentaValidador
    {
        public static void Validar(Venta venta, IList<DetalleVenta> detalles)
        {
            if (venta == null)
                throw new NegocioException("La venta no puede ser nula.");

            if (detalles == null || detalles.Count == 0)
                throw new NegocioException("El carrito de compras está vacío.");

            if (venta.IdUsuario <= 0)
                throw new NegocioException("Se requiere un cajero/usuario válido para registrar la venta.");

            foreach (var d in detalles)
            {
                if (d.IdProducto <= 0)
                    throw new NegocioException("Identificador de producto inválido en el detalle.");

                if (d.Cantidad <= 0)
                    throw new NegocioException($"La cantidad para el producto ID {d.IdProducto} debe ser mayor a cero.");

                if (d.PrecioUnitario < 0)
                    throw new NegocioException($"El precio unitario no puede ser negativo.");
            }
        }
    }
}
