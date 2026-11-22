using System;

namespace PuntoDeVenta.Comun.Excepciones
{
    public class DatosException : Exception
    {
        public DatosException() { }

        public DatosException(string message) : base(message) { }

        public DatosException(string message, Exception innerException) : base(message, innerException) { }
    }
}
