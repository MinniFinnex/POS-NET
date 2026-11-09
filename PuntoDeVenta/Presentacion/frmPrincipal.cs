using PuntoDeVenta.Entidades;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PuntoDeVenta.Presentacion
{
    public class frmPrincipal : Form
    {
        private readonly Usuario _usuario;
        private Panel panelContenedor = null!;
        private readonly List<Button> _menuButtons = new();

        public frmPrincipal(Usuario usuario)
        {
            _usuario = usuario;
            InicializarComponentes();
        }

        private void InicializarComponentes()
        {
            string rolEtiqueta = _usuario.IdRol == 1 ? "Administrador" : (_usuario.RolNombre ?? "Cajero");
            this.Text = $"Punto de Venta POS - {_usuario.NombreCompleto} [{rolEtiqueta}]";
            this.Size = new Size(1180, 740);
            this.MinimumSize = new Size(1024, 680);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 9.5f);

            // 1. Barra Superior
            var panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(15, 23, 42),
                Padding = new Padding(20, 0, 20, 0)
            };

            var lblLogo = new Label
            {
                Text = "🛒 SISTEMA POS",
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                AutoSize = true
            };

            var lblUserInfo = new Label
            {
                Text = $"👤 {_usuario.NombreCompleto}   |   Rol: {rolEtiqueta}",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(203, 213, 225),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(800, 20),
                AutoSize = true
            };

            panelTop.Controls.Add(lblLogo);
            panelTop.Controls.Add(lblUserInfo);

            // 2. Barra Lateral (Menú de Navegación)
            var panelMenu = new Panel
            {
                Dock = DockStyle.Left,
                Width = 230,
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(10, 20, 10, 15)
            };

            int topPos = 15;

            Button CrearBotonMenu(string texto, Action accion)
            {
                var btn = new Button
                {
                    Text = texto,
                    Location = new Point(10, topPos),
                    Size = new Size(210, 48),
                    BackColor = Color.FromArgb(30, 41, 59),
                    ForeColor = Color.FromArgb(241, 245, 249),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Cursor = Cursors.Hand,
                    Padding = new Padding(12, 0, 0, 0)
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += (s, e) =>
                {
                    ResaltarBotonActivo(btn);
                    accion();
                };
                _menuButtons.Add(btn);
                topPos += 54;
                return btn;
            }

            var btnVentas = CrearBotonMenu("🧾  Nueva Venta", () => AbrirFormularioEnPanel(new frmVentas(_usuario)));
            var btnProductos = CrearBotonMenu(_usuario.IdRol == 1 ? "📦  Productos" : "📦  Catálogo", () => AbrirFormularioEnPanel(new frmProductos(_usuario)));
            var btnClientes = CrearBotonMenu("👥  Clientes", () => AbrirFormularioEnPanel(new frmClientes()));
            var btnCaja = CrearBotonMenu("💵  Control de Caja", () => AbrirFormularioEnPanel(new frmCaja(_usuario)));
            var btnReportes = CrearBotonMenu("📊  Reportes y Ventas", () => AbrirFormularioEnPanel(new frmReportes(_usuario)));

            // Espacio y botón Salir al fondo
            var btnSalir = new Button
            {
                Text = " Cerrar Sesión",
                Dock = DockStyle.Bottom,
                Height = 44,
                BackColor = Color.FromArgb(51, 65, 85),
                ForeColor = Color.FromArgb(248, 113, 113),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            btnSalir.FlatAppearance.BorderSize = 0;
            btnSalir.Click += (s, e) => this.Close();

            panelMenu.Controls.Add(btnVentas);
            panelMenu.Controls.Add(btnProductos);
            panelMenu.Controls.Add(btnClientes);
            panelMenu.Controls.Add(btnCaja);
            panelMenu.Controls.Add(btnReportes);
            panelMenu.Controls.Add(btnSalir);

            // 3. Contenedor de Formularios Hijos
            panelContenedor = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 250, 252)
            };

            this.Controls.Add(panelContenedor);
            this.Controls.Add(panelMenu);
            this.Controls.Add(panelTop);

            // Abrir módulo de ventas por defecto y resaltar botón
            this.Load += (s, e) =>
            {
                ResaltarBotonActivo(btnVentas);
                AbrirFormularioEnPanel(new frmVentas(_usuario));
            };
        }

        private void ResaltarBotonActivo(Button botonActivo)
        {
            foreach (var b in _menuButtons)
            {
                b.BackColor = Color.FromArgb(30, 41, 59);
                b.ForeColor = Color.FromArgb(241, 245, 249);
            }
            botonActivo.BackColor = Color.FromArgb(37, 99, 235);
            botonActivo.ForeColor = Color.White;
        }

        private void AbrirFormularioEnPanel(Form formHijo)
        {
            if (panelContenedor.Controls.Count > 0)
            {
                var previo = panelContenedor.Controls[0] as Form;
                previo?.Dispose();
                panelContenedor.Controls.Clear();
            }

            formHijo.TopLevel = false;
            formHijo.FormBorderStyle = FormBorderStyle.None;
            formHijo.Dock = DockStyle.Fill;
            panelContenedor.Controls.Add(formHijo);
            panelContenedor.Tag = formHijo;
            formHijo.Show();
        }
    }
}
