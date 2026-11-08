using PuntoDeVenta.Datos;
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
        private Button btnProbarDb = null!;
        private Label lblEstado = null!;

        public Usuario? UsuarioAutenticado { get; private set; }

        public frmLogin()
        {
            InicializarComponentes();
        }

        private void InicializarComponentes()
        {
            this.Text = "Punto de Venta - Iniciar Sesión";
            this.Size = new Size(420, 360);
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
                Location = new Point(30, 100),
                AutoSize = true
            };

            txtUsuario = new TextBox
            {
                Location = new Point(30, 125),
                Size = new Size(345, 28),
                Font = new Font("Segoe UI", 10),
                Text = "admin"
            };

            var lblPassword = new Label
            {
                Text = "Contraseña:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(30, 165),
                AutoSize = true
            };

            txtPassword = new TextBox
            {
                Location = new Point(30, 190),
                Size = new Size(345, 28),
                Font = new Font("Segoe UI", 10),
                PasswordChar = '●',
                Text = "admin123"
            };

            btnIngresar = new Button
            {
                Text = "Iniciar Sesión",
                Location = new Point(30, 235),
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
                Location = new Point(210, 235),
                Size = new Size(165, 38),
                BackColor = Color.FromArgb(226, 232, 240),
                ForeColor = Color.FromArgb(30, 41, 59),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnSalir.FlatAppearance.BorderSize = 0;
            btnSalir.Click += (s, e) => Application.Exit();

            btnProbarDb = new Button
            {
                Text = "🔌 Probar Conexión SQL",
                Location = new Point(30, 285),
                Size = new Size(345, 28),
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(71, 85, 105),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f),
                Cursor = Cursors.Hand
            };
            btnProbarDb.FlatAppearance.BorderSize = 1;
            btnProbarDb.Click += BtnProbarDb_Click;

            lblEstado = new Label
            {
                Location = new Point(30, 318),
                Size = new Size(345, 20),
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.DarkGray,
                TextAlign = ContentAlignment.MiddleCenter
            };

            this.Controls.Add(lblTitulo);
            this.Controls.Add(lblSubtitulo);
            this.Controls.Add(lblUsuario);
            this.Controls.Add(txtUsuario);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnIngresar);
            this.Controls.Add(btnSalir);
            this.Controls.Add(btnProbarDb);
            this.Controls.Add(lblEstado);

            this.AcceptButton = btnIngresar;
        }

        private void BtnProbarDb_Click(object? sender, EventArgs e)
        {
            if (Conexion.ProbarConexion(out string err))
            {
                MessageBox.Show("¡Conexión a SQL Server exitosa!", "Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"No se pudo conectar a la base de datos:\n{err}\n\nAsegúrate de ejecutar los scripts de Database/ en SQL Server.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnIngresar_Click(object? sender, EventArgs e)
        {
            var service = new UsuarioService();
            try
            {
                var usuario = service.IniciarSesion(txtUsuario.Text, txtPassword.Text);
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
                    // Si las credenciales no coinciden en BD pero es el admin por defecto y no hay conexión a BD, ofrecer entrar en modo demo
                    var respuesta = MessageBox.Show(
                        "Usuario o contraseña incorrectos o la base de datos aún no contiene el usuario.\n\n¿Deseas iniciar en MODO DEMO?",
                        "Autenticación",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (respuesta == DialogResult.Yes)
                    {
                        UsuarioAutenticado = new Usuario
                        {
                            IdUsuario = 1,
                            NombreCompleto = "Administrador (Demo)",
                            NombreUsuario = "admin",
                            IdRol = 1,
                            RolNombre = "Administrador"
                        };
                        this.Hide();
                        var frm = new frmPrincipal(UsuarioAutenticado);
                        frm.FormClosed += (s, ev) => this.Close();
                        frm.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                var resp = MessageBox.Show(
                    $"Error al consultar la base de datos:\n{ex.Message}\n\n¿Deseas iniciar la aplicación en MODO DEMO?",
                    "Aviso de Base de Datos",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (resp == DialogResult.Yes)
                {
                    UsuarioAutenticado = new Usuario
                    {
                        IdUsuario = 1,
                        NombreCompleto = "Administrador (Demo)",
                        NombreUsuario = "admin",
                        IdRol = 1,
                        RolNombre = "Administrador"
                    };
                    this.Hide();
                    var frm = new frmPrincipal(UsuarioAutenticado);
                    frm.FormClosed += (s, ev) => this.Close();
                    frm.Show();
                }
            }
        }
    }
}
