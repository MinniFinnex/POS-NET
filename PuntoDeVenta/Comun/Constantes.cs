namespace PuntoDeVenta.Comun
{
    public static class Constantes
    {
        public static class Roles
        {
            public const string Administrador = "Administrador";
            public const string Cajero = "Cajero";
        }

        public static class EstadosCaja
        {
            public const string Abierta = "ABIERTA";
            public const string Cerrada = "CERRADA";
        }

        public static class EstadosVenta
        {
            public const string Completada = "COMPLETADA";
            public const string Cancelada = "CANCELADA";
        }

        public const decimal TasaIvaReferencial = 0.13m;
    }
}
