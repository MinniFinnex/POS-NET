using System.Collections.Generic;

namespace PuntoDeVenta.Datos.Repositorios
{
    public interface IRepositorio<T> where T : class
    {
        T? Obtener(int id);
        IEnumerable<T> ObtenerTodos();
        void Agregar(T entidad);
        void Actualizar(T entidad);
        void Eliminar(int id);
    }
}
