using PuntoDeVenta.Entidades;
using PuntoDeVenta.Utilidades;
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
            string rolEtiqueta = _usuario.IdRol == 1 ? "Admin" : (_usuario.RolNombre ?? "Cajero");
            this.Text = $"Punto de Venta - {_usuario.NombreCompleto} [{rolEtiqueta}]";
            this.Size = new Size(1180, 740);
            this.MinimumSize = new Size(1024, 680);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = TemaUI.FondoClaro;
            this.Font = new Font("Segoe UI", 9.5f);

            var panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = TemaUI.AzulMarino,
                Padding = new Padding(20, 0, 20, 0)
            };

            var lblLogo = new Label
            {
                Text = "SISTEMA POS",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 16),
                AutoSize = true
            };

            var lblUserInfo = new Label
            {
                Text = $"{_usuario.NombreCompleto} | {rolEtiqueta}",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(203, 213, 225),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(820, 18),
                AutoSize = true
            };

            panelTop.Controls.Add(lblLogo);
            panelTop.Controls.Add(lblUserInfo);

            var panelMenu = new Panel
            {
                Dock = DockStyle.Left,
                Width = 200,
                BackColor = TemaUI.AzulMarinoHover,
                Padding = new Padding(8, 15, 8, 15)
            };

            int topPos = 12;

            Button CrearBotonMenu(string texto, Action accion)
            {
                var btn = new Button
                {
                    Text = texto,
                    Location = new Point(8, topPos),
                    Size = new Size(184, 44),
                    BackColor = TemaUI.AzulMarinoHover,
                    ForeColor = Color.FromArgb(241, 245, 249),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Cursor = Cursors.Hand,
                    Padding = new Padding(12, 0, 0, 0)
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = TemaUI.AzulMarino;
                btn.FlatAppearance.MouseDownBackColor = TemaUI.AzulMarinoDown;
                btn.Click += (s, e) =>
                {
                    ResaltarBotonActivo(btn);
                    accion();
                };
                _menuButtons.Add(btn);
                topPos += 48;
                return btn;
            }

            var btnVentas = CrearBotonMenu("Ventas", () => AbrirFormularioEnPanel(new frmVentas(_usuario)));
            var btnProductos = CrearBotonMenu("Productos", () => AbrirFormularioEnPanel(new frmProductos(_usuario)));
            var btnClientes = CrearBotonMenu("Clientes", () => AbrirFormularioEnPanel(new frmClientes()));
            var btnCaja = CrearBotonMenu("Caja", () => AbrirFormularioEnPanel(new frmCaja(_usuario)));
            var btnReportes = CrearBotonMenu("Reportes", () => AbrirFormularioEnPanel(new frmReportes(_usuario)));

            var btnSalir = new Button
            {
                Text = "Cerrar Sesión",
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = TemaUI.AzulMarino,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            btnSalir.FlatAppearance.BorderSize = 0;
            btnSalir.FlatAppearance.MouseOverBackColor = TemaUI.AzulMarinoHover;
            btnSalir.FlatAppearance.MouseDownBackColor = TemaUI.AzulMarinoDown;
            btnSalir.Click += (s, e) => this.Close();

            panelMenu.Controls.Add(btnVentas);
            panelMenu.Controls.Add(btnProductos);
            panelMenu.Controls.Add(btnClientes);
            panelMenu.Controls.Add(btnCaja);
            panelMenu.Controls.Add(btnReportes);
            panelMenu.Controls.Add(btnSalir);

            panelContenedor = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = TemaUI.FondoClaro
            };

            this.Controls.Add(panelContenedor);
            this.Controls.Add(panelMenu);
            this.Controls.Add(panelTop);

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
                b.BackColor = TemaUI.AzulMarinoHover;
                b.ForeColor = Color.FromArgb(241, 245, 249);
            }
            botonActivo.BackColor = TemaUI.AzulMarino;
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
