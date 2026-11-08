using Microsoft.Data.SqlClient;
using System;
using System.Configuration;

namespace PuntoDeVenta.Datos
{
    public static class Conexion
    {
        private static readonly string _cadena =
            ConfigurationManager.ConnectionStrings["PuntoDeVentaDB"]?.ConnectionString
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=PuntoDeVentaDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public static SqlConnection ObtenerConexion()
        {
            return new SqlConnection(_cadena);
        }

        public static bool ProbarConexion(out string mensajeError)
        {
            mensajeError = string.Empty;
            try
            {
                using var cn = ObtenerConexion();
                cn.Open();
                return true;
            }
            catch (Exception ex)
            {
                mensajeError = ex.Message;
                return false;
            }
        }
    }
}
