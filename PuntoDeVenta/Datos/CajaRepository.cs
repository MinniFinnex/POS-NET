using Microsoft.Data.SqlClient;
using PuntoDeVenta.Entidades;
using System;
using System.Collections.Generic;

namespace PuntoDeVenta.Datos
{
    public class CajaRepository
    {
        public Caja? ObtenerCajaAbierta(int? idUsuario = null)
        {
            using var cn = Conexion.ObtenerConexion();
            string query = @"SELECT TOP 1 c.IdCaja, c.IdUsuario, u.NombreCompleto AS UsuarioNombre, 
                                    c.FechaApertura, c.MontoInicial, c.FechaCierre, 
                                    ISNULL((SELECT SUM(v.Total) FROM Ventas v WHERE v.IdCaja = c.IdCaja AND v.Estado = 'COMPLETADA'), 0) AS MontoVentas,
                                    c.MontoCierre, c.Diferencia, c.Observaciones, c.Estado
                             FROM Cajas c
                             INNER JOIN Usuarios u ON c.IdUsuario = u.IdUsuario
                             WHERE c.Estado = 'ABIERTA'";

            if (idUsuario.HasValue && idUsuario.Value > 0)
            {
                query += " AND c.IdUsuario = @IdUsuario";
            }

            query += " ORDER BY c.FechaApertura DESC";

            using var cmd = new SqlCommand(query, cn);
            if (idUsuario.HasValue && idUsuario.Value > 0)
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario.Value);

            cn.Open();
            using var dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                return MapearCaja(dr);
            }
            return null;
        }

        public int AbrirCaja(Caja caja)
        {
            using var cn = Conexion.ObtenerConexion();
            using var cmd = new SqlCommand(
                @"INSERT INTO Cajas (IdUsuario, FechaApertura, MontoInicial, MontoVentas, Estado, Observaciones)
                  VALUES (@IdUsuario, @FechaApertura, @MontoInicial, 0, 'ABIERTA', @Observaciones);
                  SELECT SCOPE_IDENTITY();", cn);

            cmd.Parameters.AddWithValue("@IdUsuario", caja.IdUsuario);
            cmd.Parameters.AddWithValue("@FechaApertura", caja.FechaApertura);
            cmd.Parameters.AddWithValue("@MontoInicial", caja.MontoInicial);
            cmd.Parameters.AddWithValue("@Observaciones", (object?)caja.Observaciones ?? DBNull.Value);

            cn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public decimal ObtenerTotalVentasCaja(int idCaja)
        {
            using var cn = Conexion.ObtenerConexion();
            using var cmd = new SqlCommand(
                "SELECT ISNULL(SUM(Total), 0) FROM Ventas WHERE IdCaja = @IdCaja AND Estado = 'COMPLETADA'", cn);
            cmd.Parameters.AddWithValue("@IdCaja", idCaja);

            cn.Open();
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }

        public bool CerrarCaja(int idCaja, decimal montoCierre, string observaciones)
        {
            decimal montoVentas = ObtenerTotalVentasCaja(idCaja);

            using var cn = Conexion.ObtenerConexion();
            using var cmd = new SqlCommand(
                @"UPDATE Cajas 
                  SET FechaCierre = GETDATE(),
                      MontoVentas = @MontoVentas,
                      MontoCierre = @MontoCierre,
                      Diferencia = @MontoCierre - (MontoInicial + @MontoVentas),
                      Observaciones = @Observaciones,
                      Estado = 'CERRADA'
                  WHERE IdCaja = @IdCaja", cn);

            cmd.Parameters.AddWithValue("@IdCaja", idCaja);
            cmd.Parameters.AddWithValue("@MontoVentas", montoVentas);
            cmd.Parameters.AddWithValue("@MontoCierre", montoCierre);
            cmd.Parameters.AddWithValue("@Observaciones", string.IsNullOrWhiteSpace(observaciones) ? (object)DBNull.Value : observaciones);

            cn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public List<Caja> ObtenerHistorialCajas()
        {
            var lista = new List<Caja>();
            using var cn = Conexion.ObtenerConexion();
            using var cmd = new SqlCommand(
                @"SELECT c.IdCaja, c.IdUsuario, u.NombreCompleto AS UsuarioNombre, 
                         c.FechaApertura, c.MontoInicial, c.FechaCierre, 
                         ISNULL(c.MontoVentas, 0) AS MontoVentas,
                         c.MontoCierre, c.Diferencia, c.Observaciones, c.Estado
                  FROM Cajas c
                  INNER JOIN Usuarios u ON c.IdUsuario = u.IdUsuario
                  ORDER BY c.FechaApertura DESC", cn);

            cn.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                lista.Add(MapearCaja(dr));
            }
            return lista;
        }

        private static Caja MapearCaja(SqlDataReader dr)
        {
            return new Caja
            {
                IdCaja = dr.GetInt32(dr.GetOrdinal("IdCaja")),
                IdUsuario = dr.GetInt32(dr.GetOrdinal("IdUsuario")),
                UsuarioNombre = dr.GetString(dr.GetOrdinal("UsuarioNombre")),
                FechaApertura = dr.GetDateTime(dr.GetOrdinal("FechaApertura")),
                MontoInicial = dr.GetDecimal(dr.GetOrdinal("MontoInicial")),
                FechaCierre = dr.IsDBNull(dr.GetOrdinal("FechaCierre")) ? null : dr.GetDateTime(dr.GetOrdinal("FechaCierre")),
                MontoVentas = dr.GetDecimal(dr.GetOrdinal("MontoVentas")),
                MontoCierre = dr.IsDBNull(dr.GetOrdinal("MontoCierre")) ? null : dr.GetDecimal(dr.GetOrdinal("MontoCierre")),
                Diferencia = dr.IsDBNull(dr.GetOrdinal("Diferencia")) ? null : dr.GetDecimal(dr.GetOrdinal("Diferencia")),
                Observaciones = dr.IsDBNull(dr.GetOrdinal("Observaciones")) ? null : dr.GetString(dr.GetOrdinal("Observaciones")),
                Estado = dr.GetString(dr.GetOrdinal("Estado"))
            };
        }
    }
}
