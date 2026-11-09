using PuntoDeVenta.Entidades;
using PuntoDeVenta.Negocio;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PuntoDeVenta.Presentacion
{
    public class frmLogin : Form
    {
        private TextBox txtUsuario = null!;
        private TextBox txtPassword = null!;
        private Button btnIngresar = null!;
        private Button btnSalir = null!;

        public Usuario? UsuarioAutenticado { get; private set; }

        public frmLogin()
        {
            InicializarComponentes();
        }

        private void InicializarComponentes()
        {
            this.Text = "Punto de Venta - Iniciar Sesión";
            this.Size = new Size(420, 330);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(245, 247, 250);

            var lblTitulo = new Label
            {
                Text = "🛒 Sistema POS",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(30, 20),
                AutoSize = true
            };

            var lblSubtitulo = new Label
            {
                Text = "Ingresa tus credenciales para acceder",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(32, 58),
                AutoSize = true
            };

            var lblUsuario = new Label
            {
                Text = "Usuario:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(30, 95),
                AutoSize = true
            };

            txtUsuario = new TextBox
            {
                Location = new Point(30, 120),
                Size = new Size(345, 28),
                Font = new Font("Segoe UI", 10),
                Text = "admin"
            };

            var lblPassword = new Label
            {
                Text = "Contraseña:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(30, 160),
                AutoSize = true
            };

            txtPassword = new TextBox
            {
                Location = new Point(30, 185),
                Size = new Size(345, 28),
                Font = new Font("Segoe UI", 10),
                PasswordChar = '●',
                Text = "admin123"
            };

            btnIngresar = new Button
            {
                Text = "Iniciar Sesión",
                Location = new Point(30, 230),
                Size = new Size(165, 38),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnIngresar.FlatAppearance.BorderSize = 0;
            btnIngresar.Click += BtnIngresar_Click;

            btnSalir = new Button
            {
                Text = "Salir",
                Location = new Point(210, 230),
                Size = new Size(165, 38),
                BackColor = Color.FromArgb(226, 232, 240),
                ForeColor = Color.FromArgb(30, 41, 59),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnSalir.FlatAppearance.BorderSize = 0;
            btnSalir.Click += (s, e) => Application.Exit();

            this.Controls.Add(lblTitulo);
            this.Controls.Add(lblSubtitulo);
            this.Controls.Add(lblUsuario);
            this.Controls.Add(txtUsuario);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnIngresar);
            this.Controls.Add(btnSalir);
        }

        private void BtnIngresar_Click(object? sender, EventArgs e)
        {
            var service = new UsuarioService();
            try
            {
                var usuario = service.IniciarSesion(txtUsuario.Text.Trim(), txtPassword.Text);
                if (usuario != null)
                {
                    UsuarioAutenticado = usuario;
                    this.Hide();
                    var frm = new frmPrincipal(usuario);
                    frm.FormClosed += (s, ev) => this.Close();
                    frm.Show();
                }
                else
                {
                    MessageBox.Show(
                        "Usuario o contraseña incorrectos. Verifica tus credenciales.",
                        "Error de Autenticación",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al conectar con la base de datos SQL Server:\n\n{ex.Message}\n\nAsegúrate de que el servicio de SQL Server esté en ejecución.",
                    "Error de Conexión a Base de Datos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
