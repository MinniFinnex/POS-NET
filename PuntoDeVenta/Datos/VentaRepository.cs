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
                    @"INSERT INTO Ventas (Fecha, IdCliente, IdUsuario, IdCaja, Subtotal, Impuesto, Total, Estado)
                      VALUES (@Fecha, @IdCliente, @IdUsuario, @IdCaja, @Subtotal, @Impuesto, @Total, @Estado);
                      SELECT SCOPE_IDENTITY();", cn, tx);

                cmdVenta.Parameters.AddWithValue("@Fecha", venta.Fecha);
                cmdVenta.Parameters.AddWithValue("@IdCliente", (object?)venta.IdCliente ?? DBNull.Value);
                cmdVenta.Parameters.AddWithValue("@IdUsuario", venta.IdUsuario);
                cmdVenta.Parameters.AddWithValue("@IdCaja", (object?)venta.IdCaja ?? DBNull.Value);
                cmdVenta.Parameters.AddWithValue("@Subtotal", venta.Subtotal);
                cmdVenta.Parameters.AddWithValue("@Impuesto", venta.Impuesto);
                cmdVenta.Parameters.AddWithValue("@Total", venta.Total);
                cmdVenta.Parameters.AddWithValue("@Estado", venta.Estado ?? "COMPLETADA");

                int idVenta = Convert.ToInt32(cmdVenta.ExecuteScalar());

                foreach (var d in detalles)
                {
                    // Validar primero si el producto existe y tiene stock suficiente
                    var cmdCheck = new SqlCommand("SELECT Stock, Nombre FROM Productos WHERE IdProducto = @IdProducto", cn, tx);
                    cmdCheck.Parameters.AddWithValue("@IdProducto", d.IdProducto);
                    using (var drCheck = cmdCheck.ExecuteReader())
                    {
                        if (drCheck.Read())
                        {
                            int stockActual = drCheck.GetInt32(0);
                            string nombreProd = drCheck.GetString(1);
                            if (stockActual < d.Cantidad)
                            {
                                throw new InvalidOperationException($"Stock insuficiente para '{nombreProd}'. Disponible: {stockActual}, Solicitado: {d.Cantidad}.");
                            }
                        }
                    }

                    var cmdDet = new SqlCommand(
                        @"INSERT INTO DetalleVentas (IdVenta, IdProducto, Cantidad, PrecioUnitario, Subtotal)
                          VALUES (@IdVenta, @IdProducto, @Cantidad, @PrecioUnitario, @Subtotal);

                          UPDATE Productos 
                          SET Stock = Stock - @Cantidad
                          WHERE IdProducto = @IdProducto;", cn, tx);

                    cmdDet.Parameters.AddWithValue("@IdVenta", idVenta);
                    cmdDet.Parameters.AddWithValue("@IdProducto", d.IdProducto);
                    cmdDet.Parameters.AddWithValue("@Cantidad", d.Cantidad);
                    cmdDet.Parameters.AddWithValue("@PrecioUnitario", d.PrecioUnitario);
                    cmdDet.Parameters.AddWithValue("@Subtotal", d.Subtotal);

                    cmdDet.ExecuteNonQuery();
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
            return ObtenerHistorialVentasFiltrado(DateTime.MinValue, DateTime.MaxValue, null);
        }

        public List<Venta> ObtenerHistorialVentasFiltrado(DateTime fechaInicio, DateTime fechaFin, int? idUsuario = null)
        {
            var ventas = new List<Venta>();
            using var cn = Conexion.ObtenerConexion();
            string sql = @"SELECT v.IdVenta, v.Fecha, v.IdCliente, c.Nombre AS ClienteNombre,
                                  v.IdUsuario, u.NombreCompleto AS UsuarioNombre, v.IdCaja,
                                  v.Subtotal, v.Impuesto, v.Total, v.Estado
                           FROM Ventas v
                           LEFT JOIN Clientes c ON v.IdCliente = c.IdCliente
                           INNER JOIN Usuarios u ON v.IdUsuario = u.IdUsuario
                           WHERE 1=1";

            if (fechaInicio > DateTime.MinValue)
                sql += " AND v.Fecha >= @FechaInicio";

            if (fechaFin < DateTime.MaxValue)
                sql += " AND v.Fecha <= @FechaFin";

            if (idUsuario.HasValue && idUsuario.Value > 0)
                sql += " AND v.IdUsuario = @IdUsuario";

            sql += " ORDER BY v.Fecha DESC";

            using var cmd = new SqlCommand(sql, cn);
            if (fechaInicio > DateTime.MinValue)
                cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
            if (fechaFin < DateTime.MaxValue)
                cmd.Parameters.AddWithValue("@FechaFin", fechaFin);
            if (idUsuario.HasValue && idUsuario.Value > 0)
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario.Value);

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
                    IdCaja        = dr.IsDBNull(dr.GetOrdinal("IdCaja")) ? null : dr.GetInt32(dr.GetOrdinal("IdCaja")),
                    Subtotal      = dr.GetDecimal(dr.GetOrdinal("Subtotal")),
                    Impuesto      = dr.GetDecimal(dr.GetOrdinal("Impuesto")),
                    Total         = dr.GetDecimal(dr.GetOrdinal("Total")),
                    Estado        = dr.GetString(dr.GetOrdinal("Estado"))
                });
            }
            return ventas;
        }

        public List<DetalleVenta> ObtenerDetallesPorVenta(int idVenta)
        {
            var detalles = new List<DetalleVenta>();
            using var cn = Conexion.ObtenerConexion();
            using var cmd = new SqlCommand(
                @"SELECT d.IdDetalle, d.IdVenta, d.IdProducto, p.Nombre AS ProductoNombre,
                         d.Cantidad, d.PrecioUnitario, d.Subtotal
                  FROM DetalleVentas d
                  INNER JOIN Productos p ON d.IdProducto = p.IdProducto
                  WHERE d.IdVenta = @IdVenta", cn);

            cmd.Parameters.AddWithValue("@IdVenta", idVenta);
            cn.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                detalles.Add(new DetalleVenta
                {
                    IdDetalle      = dr.GetInt32(dr.GetOrdinal("IdDetalle")),
                    IdVenta        = dr.GetInt32(dr.GetOrdinal("IdVenta")),
                    IdProducto     = dr.GetInt32(dr.GetOrdinal("IdProducto")),
                    ProductoNombre = dr.GetString(dr.GetOrdinal("ProductoNombre")),
                    Cantidad       = dr.GetInt32(dr.GetOrdinal("Cantidad")),
                    PrecioUnitario = dr.GetDecimal(dr.GetOrdinal("PrecioUnitario")),
                    Subtotal       = dr.GetDecimal(dr.GetOrdinal("Subtotal"))
                });
            }
            return detalles;
        }

        public List<ProductoVendidoDTO> ObtenerTopProductos(DateTime fechaInicio, DateTime fechaFin, int top = 5)
        {
            var topList = new List<ProductoVendidoDTO>();
            using var cn = Conexion.ObtenerConexion();
            using var cmd = new SqlCommand(
                $@"SELECT TOP ({top}) p.Codigo, p.Nombre,
                          SUM(d.Cantidad) AS CantidadVendida,
                          SUM(d.Subtotal) AS TotalRecaudado
                   FROM DetalleVentas d
                   INNER JOIN Ventas v ON d.IdVenta = v.IdVenta
                   INNER JOIN Productos p ON d.IdProducto = p.IdProducto
                   WHERE v.Estado = 'COMPLETADA'
                     AND v.Fecha >= @FechaInicio AND v.Fecha <= @FechaFin
                   GROUP BY p.Codigo, p.Nombre
                   ORDER BY CantidadVendida DESC", cn);

            cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
            cmd.Parameters.AddWithValue("@FechaFin", fechaFin);

            cn.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                topList.Add(new ProductoVendidoDTO
                {
                    Codigo = dr.GetString(0),
                    Nombre = dr.GetString(1),
                    CantidadVendida = dr.GetInt32(2),
                    TotalRecaudado = dr.GetDecimal(3)
                });
            }
            return topList;
        }

        public ResumenVentasDTO ObtenerResumen(DateTime fechaInicio, DateTime fechaFin, int? idUsuario = null)
        {
            var resumen = new ResumenVentasDTO();
            using var cn = Conexion.ObtenerConexion();
            string sql = @"SELECT 
                             ISNULL(SUM(v.Total), 0) AS TotalVendido,
                             COUNT(DISTINCT v.IdVenta) AS TotalTransacciones,
                             ISNULL(SUM(d.Cantidad), 0) AS TotalArticulos
                           FROM Ventas v
                           LEFT JOIN DetalleVentas d ON v.IdVenta = d.IdVenta
                           WHERE v.Estado = 'COMPLETADA'
                             AND v.Fecha >= @FechaInicio AND v.Fecha <= @FechaFin";

            if (idUsuario.HasValue && idUsuario.Value > 0)
                sql += " AND v.IdUsuario = @IdUsuario";

            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
            cmd.Parameters.AddWithValue("@FechaFin", fechaFin);
            if (idUsuario.HasValue && idUsuario.Value > 0)
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario.Value);

            cn.Open();
            using var dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                resumen.TotalVendido = dr.IsDBNull(0) ? 0 : dr.GetDecimal(0);
                resumen.TotalTransacciones = dr.IsDBNull(1) ? 0 : dr.GetInt32(1);
                resumen.TotalArticulos = dr.IsDBNull(2) ? 0 : dr.GetInt32(2);
            }
            return resumen;
        }
    }
}
