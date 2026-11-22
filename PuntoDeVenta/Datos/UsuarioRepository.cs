using Microsoft.Data.SqlClient;
using PuntoDeVenta.Entidades;
using System;
using System.Collections.Generic;

namespace PuntoDeVenta.Datos
{
    public class UsuarioRepository
    {
        public Usuario? ValidarLogin(string usuario, string password)
        {
            using var cn = Conexion.ObtenerConexion();
            using var cmd = new SqlCommand(
                @"SELECT u.IdUsuario, u.NombreCompleto, u.Usuario, u.Password, 
                         u.IdRol, r.Nombre AS RolNombre, u.Activo, u.FechaCreacion
                  FROM Usuarios u
                  INNER JOIN Roles r ON u.IdRol = r.IdRol
                  WHERE u.Usuario = @Usuario AND u.Password = @Password AND u.Activo = 1", cn);

            cmd.Parameters.AddWithValue("@Usuario", usuario);
            cmd.Parameters.AddWithValue("@Password", password);

            cn.Open();
            using var dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                return new Usuario
                {
                    IdUsuario      = dr.GetInt32(dr.GetOrdinal("IdUsuario")),
                    NombreCompleto = dr.GetString(dr.GetOrdinal("NombreCompleto")),
                    NombreUsuario  = dr.GetString(dr.GetOrdinal("Usuario")),
                    Password       = dr.GetString(dr.GetOrdinal("Password")),
                    IdRol          = dr.GetInt32(dr.GetOrdinal("IdRol")),
                    RolNombre      = dr.GetString(dr.GetOrdinal("RolNombre")),
                    Activo         = dr.GetBoolean(dr.GetOrdinal("Activo")),
                    FechaCreacion  = dr.GetDateTime(dr.GetOrdinal("FechaCreacion"))
                };
            }
            return null;
        }

        public List<Usuario> ObtenerTodos()
        {
            var lista = new List<Usuario>();
            using var cn = Conexion.ObtenerConexion();
            using var cmd = new SqlCommand(
                @"SELECT u.IdUsuario, u.NombreCompleto, u.Usuario, u.Password, 
                         u.IdRol, r.Nombre AS RolNombre, u.Activo, u.FechaCreacion
                  FROM Usuarios u
                  INNER JOIN Roles r ON u.IdRol = r.IdRol
                  WHERE u.Activo = 1", cn);

            cn.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                lista.Add(new Usuario
                {
                    IdUsuario      = dr.GetInt32(dr.GetOrdinal("IdUsuario")),
                    NombreCompleto = dr.GetString(dr.GetOrdinal("NombreCompleto")),
                    NombreUsuario  = dr.GetString(dr.GetOrdinal("Usuario")),
                    Password       = dr.GetString(dr.GetOrdinal("Password")),
                    IdRol          = dr.GetInt32(dr.GetOrdinal("IdRol")),
                    RolNombre      = dr.GetString(dr.GetOrdinal("RolNombre")),
                    Activo         = dr.GetBoolean(dr.GetOrdinal("Activo")),
                    FechaCreacion  = dr.GetDateTime(dr.GetOrdinal("FechaCreacion"))
                });
            }
            return lista;
        }
    }
}
