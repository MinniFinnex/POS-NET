using PuntoDeVenta.Entidades;
using PuntoDeVenta.Negocio;
using PuntoDeVenta.Utilidades;
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
            this.Text = "Iniciar Sesión";
            this.Size = new Size(380, 290);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = TemaUI.FondoClaro;
            this.Font = new Font("Segoe UI", 9.5f);

            var lblTitulo = new Label
            {
                Text = "SISTEMA POS",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = TemaUI.AzulMarino,
                Location = new Point(30, 22),
                AutoSize = true
            };

            var lblUsuario = new Label
            {
                Text = "Usuario:",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = TemaUI.TextoOscuro,
                Location = new Point(30, 68),
                AutoSize = true
            };

            txtUsuario = new TextBox
            {
                Location = new Point(30, 90),
                Size = new Size(305, 27),
                Font = new Font("Segoe UI", 10),
                Text = "admin"
            };

            var lblPassword = new Label
            {
                Text = "Contraseña:",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = TemaUI.TextoOscuro,
                Location = new Point(30, 126),
                AutoSize = true
            };

            txtPassword = new TextBox
            {
                Location = new Point(30, 148),
                Size = new Size(305, 27),
                Font = new Font("Segoe UI", 10),
                PasswordChar = '●',
                Text = "admin123"
            };
            txtPassword.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    BtnIngresar_Click(btnIngresar, EventArgs.Empty);
                }
            };

            btnIngresar = new Button
            {
                Text = "Ingresar",
                Location = new Point(30, 192),
                Size = new Size(145, 36)
            };
            TemaUI.AplicarBotonAzul(btnIngresar, new Font("Segoe UI", 9.5f, FontStyle.Bold));
            btnIngresar.Click += BtnIngresar_Click;

            btnSalir = new Button
            {
                Text = "Salir",
                Location = new Point(190, 192),
                Size = new Size(145, 36)
            };
            TemaUI.AplicarBotonSecundario(btnSalir, new Font("Segoe UI", 9.5f, FontStyle.Bold));
            btnSalir.Click += (s, e) => Application.Exit();

            this.Controls.Add(lblTitulo);
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
                        "Usuario o contraseña incorrectos.",
                        "Error de Acceso",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al conectar con la base de datos:\n\n{ex.Message}",
                    "Error de Conexión",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
