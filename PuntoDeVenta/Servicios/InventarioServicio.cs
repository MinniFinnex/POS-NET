using PuntoDeVenta.Datos.Repositorios;
using PuntoDeVenta.Modelos;
using System.Collections.Generic;
using System.Linq;

namespace PuntoDeVenta.Servicios
{
    public class InventarioServicio
    {
        private readonly ProductoRepositorio _repo = new();

        public bool VerificarDisponibilidad(int idProducto, int cantidadSolicitada)
        {
            var prod = _repo.Obtener(idProducto);
            return prod != null && prod.Stock >= cantidadSolicitada;
        }

        public List<Producto> ObtenerProductosBajoStockMinimo()
        {
            return _repo.ObtenerTodos()
                .Where(p => p.Stock <= p.StockMinimo)
                .ToList();
        }
    }
}
