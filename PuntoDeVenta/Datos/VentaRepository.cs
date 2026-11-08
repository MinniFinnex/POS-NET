using Microsoft.Data.SqlClient;
using PuntoDeVenta.Entidades;
using System;
using System.Collections.Generic;

namespace PuntoDeVenta.Datos
{
    public class VentaRepository
    {
        public int RegistrarVenta(Venta venta, List<DetalleVenta> detalles)
        {
            using var cn = Conexion.ObtenerConexion();
            cn.Open();
            using var tx = cn.BeginTransaction();

            try
            {
         
                var cmdVenta = new SqlCommand(
                    @"INSERT INTO Ventas (Fecha, IdCliente, IdUsuario, Subtotal, Impuesto, Total, Estado)
                      VALUES (@Fecha, @IdCliente, @IdUsuario, @Subtotal, @Impuesto, @Total, @Estado);
                      SELECT SCOPE_IDENTITY();", cn, tx);

                cmdVenta.Parameters.AddWithValue("@Fecha", venta.Fecha);
                cmdVenta.Parameters.AddWithValue("@IdCliente", (object?)venta.IdCliente ?? DBNull.Value);
                cmdVenta.Parameters.AddWithValue("@IdUsuario", venta.IdUsuario);
                cmdVenta.Parameters.AddWithValue("@Subtotal", venta.Subtotal);
                cmdVenta.Parameters.AddWithValue("@Impuesto", venta.Impuesto);
                cmdVenta.Parameters.AddWithValue("@Total", venta.Total);
                cmdVenta.Parameters.AddWithValue("@Estado", venta.Estado ?? "COMPLETADA");

                int idVenta = Convert.ToInt32(cmdVenta.ExecuteScalar());

          
                foreach (var d in detalles)
                {
                    var cmdDet = new SqlCommand(
                        @"INSERT INTO DetalleVentas (IdVenta, IdProducto, Cantidad, PrecioUnitario, Subtotal)
                          VALUES (@IdVenta, @IdProducto, @Cantidad, @PrecioUnitario, @Subtotal);

                          UPDATE Productos 
                          SET Stock = Stock - @Cantidad
                          WHERE IdProducto = @IdProducto AND Stock >= @Cantidad;", cn, tx);

                    cmdDet.Parameters.AddWithValue("@IdVenta", idVenta);
                    cmdDet.Parameters.AddWithValue("@IdProducto", d.IdProducto);
                    cmdDet.Parameters.AddWithValue("@Cantidad", d.Cantidad);
                    cmdDet.Parameters.AddWithValue("@PrecioUnitario", d.PrecioUnitario);
                    cmdDet.Parameters.AddWithValue("@Subtotal", d.Subtotal);

                    int filasAfectadas = cmdDet.ExecuteNonQuery();
                    if (filasAfectadas < 2)
                    {
                        throw new InvalidOperationException($"Stock insuficiente para el producto ID: {d.IdProducto}");
                    }
                }

                tx.Commit();
                return idVenta;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public List<Venta> ObtenerHistorialVentas()
        {
            var ventas = new List<Venta>();
            using var cn = Conexion.ObtenerConexion();
            using var cmd = new SqlCommand(
                @"SELECT v.IdVenta, v.Fecha, v.IdCliente, c.Nombre AS ClienteNombre,
                         v.IdUsuario, u.NombreCompleto AS UsuarioNombre,
                         v.Subtotal, v.Impuesto, v.Total, v.Estado
                  FROM Ventas v
                  LEFT JOIN Clientes c ON v.IdCliente = c.IdCliente
                  INNER JOIN Usuarios u ON v.IdUsuario = u.IdUsuario
                  ORDER BY v.Fecha DESC", cn);

            cn.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                ventas.Add(new Venta
                {
                    IdVenta       = dr.GetInt32(dr.GetOrdinal("IdVenta")),
                    Fecha         = dr.GetDateTime(dr.GetOrdinal("Fecha")),
                    IdCliente     = dr.IsDBNull(dr.GetOrdinal("IdCliente")) ? null : dr.GetInt32(dr.GetOrdinal("IdCliente")),
                    ClienteNombre = dr.IsDBNull(dr.GetOrdinal("ClienteNombre")) ? "Consumidor Final" : dr.GetString(dr.GetOrdinal("ClienteNombre")),
                    IdUsuario     = dr.GetInt32(dr.GetOrdinal("IdUsuario")),
                    UsuarioNombre = dr.GetString(dr.GetOrdinal("UsuarioNombre")),
                    Subtotal      = dr.GetDecimal(dr.GetOrdinal("Subtotal")),
                    Impuesto      = dr.GetDecimal(dr.GetOrdinal("Impuesto")),
                    Total         = dr.GetDecimal(dr.GetOrdinal("Total")),
                    Estado        = dr.GetString(dr.GetOrdinal("Estado"))
                });
            }
            return ventas;
        }
    }
}
