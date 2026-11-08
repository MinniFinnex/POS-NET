using Microsoft.Data.SqlClient;
using PuntoDeVenta.Entidades;
using System;
using System.Collections.Generic;

namespace PuntoDeVenta.Datos
{
    public class ProductoRepository
    {
        public List<Producto> ObtenerTodos()
        {
            var lista = new List<Producto>();

            using var cn = Conexion.ObtenerConexion();
            using var cmd = new SqlCommand(
                @"SELECT p.IdProducto, p.Codigo, p.Nombre, p.Descripcion, 
                         p.IdCategoria, c.Nombre AS CategoriaNombre, 
                         p.PrecioCompra, p.PrecioVenta, p.Stock, p.StockMinimo, p.Activo
                  FROM Productos p
                  INNER JOIN Categorias c ON p.IdCategoria = c.IdCategoria
                  WHERE p.Activo = 1
                  ORDER BY p.Nombre", cn);

            cn.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                lista.Add(new Producto
                {
                    IdProducto      = dr.GetInt32(dr.GetOrdinal("IdProducto")),
                    Codigo          = dr.GetString(dr.GetOrdinal("Codigo")),
                    Nombre          = dr.GetString(dr.GetOrdinal("Nombre")),
                    Descripcion     = dr.IsDBNull(dr.GetOrdinal("Descripcion")) ? null : dr.GetString(dr.GetOrdinal("Descripcion")),
                    IdCategoria     = dr.GetInt32(dr.GetOrdinal("IdCategoria")),
                    CategoriaNombre = dr.GetString(dr.GetOrdinal("CategoriaNombre")),
                    PrecioCompra    = dr.GetDecimal(dr.GetOrdinal("PrecioCompra")),
                    PrecioVenta     = dr.GetDecimal(dr.GetOrdinal("PrecioVenta")),
                    Stock           = dr.GetInt32(dr.GetOrdinal("Stock")),
                    StockMinimo     = dr.GetInt32(dr.GetOrdinal("StockMinimo")),
                    Activo          = dr.GetBoolean(dr.GetOrdinal("Activo"))
                });
            }
            return lista;
        }

        public Producto? BuscarPorCodigo(string codigo)
        {
            using var cn = Conexion.ObtenerConexion();
            using var cmd = new SqlCommand(
                @"SELECT p.IdProducto, p.Codigo, p.Nombre, p.Descripcion, 
                         p.IdCategoria, c.Nombre AS CategoriaNombre, 
                         p.PrecioCompra, p.PrecioVenta, p.Stock, p.StockMinimo, p.Activo
                  FROM Productos p
                  INNER JOIN Categorias c ON p.IdCategoria = c.IdCategoria
                  WHERE p.Codigo = @Codigo AND p.Activo = 1", cn);

            cmd.Parameters.AddWithValue("@Codigo", codigo);
            cn.Open();
            using var dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                return new Producto
                {
                    IdProducto      = dr.GetInt32(dr.GetOrdinal("IdProducto")),
                    Codigo          = dr.GetString(dr.GetOrdinal("Codigo")),
                    Nombre          = dr.GetString(dr.GetOrdinal("Nombre")),
                    Descripcion     = dr.IsDBNull(dr.GetOrdinal("Descripcion")) ? null : dr.GetString(dr.GetOrdinal("Descripcion")),
                    IdCategoria     = dr.GetInt32(dr.GetOrdinal("IdCategoria")),
                    CategoriaNombre = dr.GetString(dr.GetOrdinal("CategoriaNombre")),
                    PrecioCompra    = dr.GetDecimal(dr.GetOrdinal("PrecioCompra")),
                    PrecioVenta     = dr.GetDecimal(dr.GetOrdinal("PrecioVenta")),
                    Stock           = dr.GetInt32(dr.GetOrdinal("Stock")),
                    StockMinimo     = dr.GetInt32(dr.GetOrdinal("StockMinimo")),
                    Activo          = dr.GetBoolean(dr.GetOrdinal("Activo"))
                };
            }
            return null;
        }

        public int Insertar(Producto p)
        {
            using var cn = Conexion.ObtenerConexion();
            using var cmd = new SqlCommand(
                @"INSERT INTO Productos (Codigo, Nombre, Descripcion, IdCategoria, PrecioCompra, PrecioVenta, Stock, StockMinimo, Activo)
                  VALUES (@Codigo, @Nombre, @Descripcion, @IdCategoria, @PrecioCompra, @PrecioVenta, @Stock, @StockMinimo, 1);
                  SELECT SCOPE_IDENTITY();", cn);

            cmd.Parameters.AddWithValue("@Codigo", p.Codigo);
            cmd.Parameters.AddWithValue("@Nombre", p.Nombre);
            cmd.Parameters.AddWithValue("@Descripcion", (object?)p.Descripcion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IdCategoria", p.IdCategoria);
            cmd.Parameters.AddWithValue("@PrecioCompra", p.PrecioCompra);
            cmd.Parameters.AddWithValue("@PrecioVenta", p.PrecioVenta);
            cmd.Parameters.AddWithValue("@Stock", p.Stock);
            cmd.Parameters.AddWithValue("@StockMinimo", p.StockMinimo);

            cn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public bool Actualizar(Producto p)
        {
            using var cn = Conexion.ObtenerConexion();
            using var cmd = new SqlCommand(
                @"UPDATE Productos 
                  SET Codigo = @Codigo, Nombre = @Nombre, Descripcion = @Descripcion,
                      IdCategoria = @IdCategoria, PrecioCompra = @PrecioCompra,
                      PrecioVenta = @PrecioVenta, Stock = @Stock, StockMinimo = @StockMinimo
                  WHERE IdProducto = @IdProducto", cn);

            cmd.Parameters.AddWithValue("@IdProducto", p.IdProducto);
            cmd.Parameters.AddWithValue("@Codigo", p.Codigo);
            cmd.Parameters.AddWithValue("@Nombre", p.Nombre);
            cmd.Parameters.AddWithValue("@Descripcion", (object?)p.Descripcion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IdCategoria", p.IdCategoria);
            cmd.Parameters.AddWithValue("@PrecioCompra", p.PrecioCompra);
            cmd.Parameters.AddWithValue("@PrecioVenta", p.PrecioVenta);
            cmd.Parameters.AddWithValue("@Stock", p.Stock);
            cmd.Parameters.AddWithValue("@StockMinimo", p.StockMinimo);

            cn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Eliminar(int idProducto)
        {
            using var cn = Conexion.ObtenerConexion();
            using var cmd = new SqlCommand(
                "UPDATE Productos SET Activo = 0 WHERE IdProducto = @IdProducto", cn);

            cmd.Parameters.AddWithValue("@IdProducto", idProducto);
            cn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public List<Categoria> ObtenerCategorias()
        {
            var categorias = new List<Categoria>();
            using var cn = Conexion.ObtenerConexion();
            using var cmd = new SqlCommand("SELECT IdCategoria, Nombre FROM Categorias WHERE Activo = 1", cn);

            cn.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                categorias.Add(new Categoria
                {
                    IdCategoria = dr.GetInt32(0),
                    Nombre      = dr.GetString(1)
                });
            }
            return categorias;
        }
    }
}
