using PuntoDeVenta.Datos.Repositorios;
using PuntoDeVenta.Modelos;
using System.Collections.Generic;

namespace PuntoDeVenta.Servicios
{
    public class UsuarioServicio
    {
        private readonly UsuarioRepositorio _repo = new();

        public Usuario? IniciarSesion(string usuario, string passwordEnTextoPlano)
        {
            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(passwordEnTextoPlano))
                return null;

            return _repo.ValidarLogin(usuario.Trim(), passwordEnTextoPlano.Trim());
        }

        public List<Usuario> ListarUsuarios() => _repo.ObtenerTodos();
    }
}
