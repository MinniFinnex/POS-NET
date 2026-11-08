using System;
using System.Security.Cryptography;
using System.Text;

namespace PuntoDeVenta.Utilidades
{
    public static class Seguridad
    {
        public static string CalcularHashSha256(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return string.Empty;

            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(texto));
            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        public static bool VerificarHash(string texto, string hashEsperado)
        {
            string hashCalculado = CalcularHashSha256(texto);
            return string.Equals(hashCalculado, hashEsperado, StringComparison.OrdinalIgnoreCase);
        }
    }
}
