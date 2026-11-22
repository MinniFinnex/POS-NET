using PuntoDeVenta.Datos.Repositorios;

namespace PuntoDeVenta.Datos
{
    public interface IRepositorio<T> : Repositorios.IRepositorio<T> where T : class
    {
    }
}
