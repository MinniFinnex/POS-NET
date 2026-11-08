using PuntoDeVenta.Entidades;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PuntoDeVenta.Presentacion
{
    public class frmPrincipal : Form
    {
        private readonly Usuario _usuario;

        public frmPrincipal(Usuario usuario)
        {
            _usuario = usuario;
            InicializarComponentes();
        }

        private void InicializarComponentes()
        {
            this.Text = $"Punto de Venta POS - Usuario: {_usuario.NombreCompleto} ({_usuario.RolNombre ?? "Admin"})";
            this.Size = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(248, 250, 252);

            // Barra Superior
            var panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(30, 41, 59)
            };

            var lblLogo = new Label
            {
                Text = "🛒 PUNTO DE VENTA",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 18),
                AutoSize = true
            };

            var lblUserInfo = new Label
            {
                Text = $"👤 {_usuario.NombreCompleto} | Rol: {_usuario.RolNombre ?? "Usuario"}",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(203, 213, 225),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(650, 25),
                AutoSize = true
            };

            panelTop.Controls.Add(lblLogo);
            panelTop.Controls.Add(lblUserInfo);

            // Barra Lateral / Menú
            var panelMenu = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = Color.FromArgb(15, 23, 42)
            };

            int topPos = 20;

            Button CrearBotonMenu(string texto, EventHandler onClick)
            {
                var btn = new Button
                {
                    Text = texto,
                    Location = new Point(10, topPos),
                    Size = new Size(200, 48),
                    BackColor = Color.FromArgb(30, 41, 59),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += onClick;
                topPos += 58;
                return btn;
            }

            var btnVentas = CrearBotonMenu("🧾 Nueva Venta", (s, e) => AbrirFormularioEnPanel(new frmVentas(_usuario)));
            var btnProductos = CrearBotonMenu("📦 Productos", (s, e) => AbrirFormularioEnPanel(new frmProductos()));
            var btnClientes = CrearBotonMenu("👥 Clientes", (s, e) => AbrirFormularioEnPanel(new frmClientes()));
            var btnSalir = CrearBotonMenu("🚪 Cerrar Sesión", (s, e) => this.Close());

            panelMenu.Controls.Add(btnVentas);
            panelMenu.Controls.Add(btnProductos);
            panelMenu.Controls.Add(btnClientes);
            panelMenu.Controls.Add(btnSalir);

            // Contenedor principal
            panelContenedor = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(241, 245, 249)
            };

            this.Controls.Add(panelContenedor);
            this.Controls.Add(panelMenu);
            this.Controls.Add(panelTop);

            // Abrir módulo de ventas por defecto al iniciar
            this.Load += (s, e) => AbrirFormularioEnPanel(new frmVentas(_usuario));
        }

        private Panel panelContenedor = null!;

        private void AbrirFormularioEnPanel(Form formHijo)
        {
            if (panelContenedor.Controls.Count > 0)
                panelContenedor.Controls.RemoveAt(0);

            formHijo.TopLevel = false;
            formHijo.FormBorderStyle = FormBorderStyle.None;
            formHijo.Dock = DockStyle.Fill;
            panelContenedor.Controls.Add(formHijo);
            panelContenedor.Tag = formHijo;
            formHijo.Show();
        }
    }
}
