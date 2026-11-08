using System;
using System.Windows.Forms;
using PuntoDeVenta.Presentacion;

namespace PuntoDeVenta
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new frmLogin());
        }
    }
}
