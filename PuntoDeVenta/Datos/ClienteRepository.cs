using Microsoft.Data.SqlClient;
using PuntoDeVenta.Entidades;
using System;
using System.Collections.Generic;

namespace PuntoDeVenta.Datos
{
    public class ClienteRepository
    {
        public List<Cliente> ObtenerTodos()
        {
            var lista = new List<Cliente>();
            using var cn = Conexion.ObtenerConexion();
            using var cmd = new SqlCommand(
                "SELECT IdCliente, Documento, Nombre, Telefono, Email, Direccion, Activo FROM Clientes WHERE Activo = 1", cn);

            cn.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                lista.Add(new Cliente
                {
                    IdCliente = dr.GetInt32(0),
                    Documento = dr.IsDBNull(1) ? null : dr.GetString(1),
                    Nombre    = dr.GetString(2),
                    Telefono  = dr.IsDBNull(3) ? null : dr.GetString(3),
                    Email     = dr.IsDBNull(4) ? null : dr.GetString(4),
                    Direccion = dr.IsDBNull(5) ? null : dr.GetString(5),
                    Activo    = dr.GetBoolean(6)
                });
            }
            return lista;
        }

        public int Insertar(Cliente c)
        {
            using var cn = Conexion.ObtenerConexion();
            using var cmd = new SqlCommand(
                @"INSERT INTO Clientes (Documento, Nombre, Telefono, Email, Direccion, Activo)
                  VALUES (@Documento, @Nombre, @Telefono, @Email, @Direccion, 1);
                  SELECT SCOPE_IDENTITY();", cn);

            cmd.Parameters.AddWithValue("@Documento", (object?)c.Documento ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Nombre", c.Nombre);
            cmd.Parameters.AddWithValue("@Telefono", (object?)c.Telefono ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", (object?)c.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Direccion", (object?)c.Direccion ?? DBNull.Value);

            cn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public bool Actualizar(Cliente c)
        {
            using var cn = Conexion.ObtenerConexion();
            using var cmd = new SqlCommand(
                @"UPDATE Clientes 
                  SET Documento = @Documento, Nombre = @Nombre, Telefono = @Telefono,
                      Email = @Email, Direccion = @Direccion
                  WHERE IdCliente = @IdCliente", cn);

            cmd.Parameters.AddWithValue("@IdCliente", c.IdCliente);
            cmd.Parameters.AddWithValue("@Documento", (object?)c.Documento ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Nombre", c.Nombre);
            cmd.Parameters.AddWithValue("@Telefono", (object?)c.Telefono ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", (object?)c.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Direccion", (object?)c.Direccion ?? DBNull.Value);

            cn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Eliminar(int idCliente)
        {
            using var cn = Conexion.ObtenerConexion();
            using var cmd = new SqlCommand("UPDATE Clientes SET Activo = 0 WHERE IdCliente = @IdCliente", cn);
            cmd.Parameters.AddWithValue("@IdCliente", idCliente);
            cn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
