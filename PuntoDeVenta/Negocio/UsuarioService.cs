

using PuntoDeVenta.Datos;
using PuntoDeVenta.Entidades;
using PuntoDeVenta.Utilidades;
using System.Collections.Generic;

namespace PuntoDeVenta.Negocio
{
    public class UsuarioService
    {
        private readonly UsuarioRepository _repo = new();

        public Usuario? IniciarSesion(string usuario, string passwordEnTextoPlano)
        {
            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(passwordEnTextoPlano))
                return null;

            string passwordHash = Seguridad.CalcularHashSha256(passwordEnTextoPlano);
            return _repo.ValidarLogin(usuario.Trim(), passwordHash);
        }

        public List<Usuario> ListarUsuarios() => _repo.ObtenerTodos();
    }
}
