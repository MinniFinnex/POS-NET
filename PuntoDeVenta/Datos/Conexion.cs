using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;

namespace PuntoDeVenta.Datos
{
    public static class Conexion
    {
        public static string CadenaConexion => PosContext.CadenaConexion;

        public static PosContext CrearContexto() => new PosContext();

        public static SqlConnection ObtenerConexion()
        {
            return new SqlConnection(CadenaConexion);
        }

        public static bool ProbarConexion(out string mensajeError)
        {
            mensajeError = string.Empty;
            try
            {
                using var ctx = CrearContexto();
                return ctx.Database.CanConnect();
            }
            catch (Exception ex)
            {
                mensajeError = ex.Message;
                return false;
            }
        }

        public static void AsegurarEsquema()
        {
            try
            {
                using var cn = ObtenerConexion();
                cn.Open();
                using var cmd = cn.CreateCommand();
                cmd.CommandText = @"
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Usuarios') AND name = 'PasswordHash')
BEGIN
    EXEC sp_rename 'Usuarios.PasswordHash', 'Password', 'COLUMN';
END
ELSE IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Usuarios') AND name = 'Password')
BEGIN
    ALTER TABLE Usuarios ADD Password NVARCHAR(100) NOT NULL DEFAULT 'admin123';
END";
                cmd.ExecuteNonQuery();
            }
            catch
            {
                // Silencioso: si no se puede conectar aún, el flujo principal manejará el error
            }
        }
    }
}
