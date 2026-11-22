using PuntoDeVenta.Comun.Excepciones;
using PuntoDeVenta.Datos.Repositorios;
using PuntoDeVenta.Modelos;
using System;
using System.Collections.Generic;

namespace PuntoDeVenta.Servicios
{
    public class ClienteServicio
    {
        private readonly ClienteRepositorio _repo = new();

        public List<Cliente> ListarClientes() => _repo.ObtenerTodos();

        public (bool Exito, string Mensaje, int IdCliente) Guardar(Cliente cliente)
        {
            if (string.IsNullOrWhiteSpace(cliente.Nombre))
                return (false, "El nombre del cliente es obligatorio.", 0);

            try
            {
                if (cliente.IdCliente == 0)
                {
                    int id = _repo.Insertar(cliente);
                    return (true, "Cliente registrado exitosamente.", id);
                }
                else
                {
                    bool ok = _repo.Actualizar(cliente);
                    return ok 
                        ? (true, "Cliente actualizado exitosamente.", cliente.IdCliente) 
                        : (false, "No se pudo actualizar el cliente.", 0);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error al guardar cliente: {ex.Message}", 0);
            }
        }

        public (bool Exito, string Mensaje) Eliminar(int idCliente)
        {
            try
            {
                bool ok = _repo.Eliminar(idCliente);
                return ok 
                    ? (true, "Cliente eliminado exitosamente.") 
                    : (false, "No se encontró el cliente a eliminar.");
            }
            catch (Exception ex)
            {
                return (false, $"Error al eliminar cliente: {ex.Message}");
            }
        }
    }
}
