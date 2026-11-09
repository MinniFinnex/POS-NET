using PuntoDeVenta.Datos;
using PuntoDeVenta.Entidades;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PuntoDeVenta.Presentacion
{
    public class frmClientes : Form
    {
        private DataGridView dgvClientes = null!;
        private TextBox txtDocumento = null!;
        private TextBox txtNombre = null!;
        private TextBox txtTelefono = null!;
        private TextBox txtEmail = null!;
        private TextBox txtDireccion = null!;
        private Button btnGuardar = null!;
        private Button btnEliminar = null!;
        private Button btnNuevo = null!;
        private int _idSeleccionado = 0;

        private readonly ClienteRepository _repo = new();

        public frmClientes()
        {
            InicializarComponentes();
            CargarClientes();
        }

        private void InicializarComponentes()
        {
            this.Text = "Gestión de Clientes";
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 9.5f);

            var lblTitulo = new Label
            {
                Text = " Directorio de Clientes",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 15),
                AutoSize = true
            };

            var grpDatos = new GroupBox
            {
                Text = "Datos del Cliente",
                Location = new Point(20, 50),
                Size = new Size(720, 150),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };

            var lblDoc = new Label { Text = "DUI:", Location = new Point(15, 28), AutoSize = true, Font = new Font("Segoe UI", 9) };
            txtDocumento = new TextBox { Location = new Point(15, 50), Size = new Size(130, 25), Font = new Font("Segoe UI", 9) };

            var lblNom = new Label { Text = "Nombre Completo:", Location = new Point(155, 28), AutoSize = true, Font = new Font("Segoe UI", 9) };
            txtNombre = new TextBox { Location = new Point(155, 50), Size = new Size(240, 25), Font = new Font("Segoe UI", 9) };

            var lblTel = new Label { Text = "Teléfono:", Location = new Point(405, 28), AutoSize = true, Font = new Font("Segoe UI", 9) };
            txtTelefono = new TextBox { Location = new Point(405, 50), Size = new Size(130, 25), Font = new Font("Segoe UI", 9) };

            var lblEmail = new Label { Text = "Email:", Location = new Point(545, 28), AutoSize = true, Font = new Font("Segoe UI", 9) };
            txtEmail = new TextBox { Location = new Point(545, 50), Size = new Size(160, 25), Font = new Font("Segoe UI", 9) };

            var lblDir = new Label { Text = "Dirección:", Location = new Point(15, 83), AutoSize = true, Font = new Font("Segoe UI", 9) };
            txtDireccion = new TextBox { Location = new Point(15, 105), Size = new Size(320, 25), Font = new Font("Segoe UI", 9) };

            btnNuevo = new Button
            {
                Text = "Nuevo",
                Location = new Point(360, 102),
                Size = new Size(95, 32),
                BackColor = Color.FromArgb(100, 116, 139),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnNuevo.Click += (s, e) => LimpiarFormulario();

            btnGuardar = new Button
            {
                Text = " Guardar",
                Location = new Point(470, 102),
                Size = new Size(110, 32),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnGuardar.Click += BtnGuardar_Click;

            btnEliminar = new Button
            {
                Text = " Eliminar",
                Location = new Point(595, 102),
                Size = new Size(110, 32),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnEliminar.Click += BtnEliminar_Click;

            grpDatos.Controls.Add(lblDoc);
            grpDatos.Controls.Add(txtDocumento);
            grpDatos.Controls.Add(lblNom);
            grpDatos.Controls.Add(txtNombre);
            grpDatos.Controls.Add(lblTel);
            grpDatos.Controls.Add(txtTelefono);
            grpDatos.Controls.Add(lblEmail);
            grpDatos.Controls.Add(txtEmail);
            grpDatos.Controls.Add(lblDir);
            grpDatos.Controls.Add(txtDireccion);
            grpDatos.Controls.Add(btnNuevo);
            grpDatos.Controls.Add(btnGuardar);
            grpDatos.Controls.Add(btnEliminar);

            dgvClientes = new DataGridView
            {
                Location = new Point(20, 215),
                Size = new Size(720, 310),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D
            };
            dgvClientes.CellClick += DgvClientes_CellClick;

            this.Controls.Add(lblTitulo);
            this.Controls.Add(grpDatos);
            this.Controls.Add(dgvClientes);
        }

        private void CargarClientes()
        {
            try
            {
                var lista = _repo.ObtenerTodos();
                dgvClientes.DataSource = lista;
                if (dgvClientes.Columns["IdCliente"] != null)
                    dgvClientes.Columns["IdCliente"]!.Visible = false;
                if (dgvClientes.Columns["Activo"] != null)
                    dgvClientes.Columns["Activo"]!.Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los clientes desde la base de datos:\n{ex.Message}", "Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvClientes_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvClientes.Rows.Count) return;
            var fila = dgvClientes.Rows[e.RowIndex];

            if (fila.DataBoundItem is Cliente c)
            {
                _idSeleccionado = c.IdCliente;
                txtDocumento.Text = c.Documento ?? "";
                txtNombre.Text = c.Nombre;
                txtTelefono.Text = c.Telefono ?? "";
                txtEmail.Text = c.Email ?? "";
                txtDireccion.Text = c.Direccion ?? "";
            }
        }

        private void BtnGuardar_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre del cliente es obligatorio.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var cliente = new Cliente
            {
                IdCliente = _idSeleccionado,
                Documento = txtDocumento.Text.Trim(),
                Nombre = txtNombre.Text.Trim(),
                Telefono = txtTelefono.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Direccion = txtDireccion.Text.Trim()
            };

            try
            {
                if (_idSeleccionado == 0)
                {
                    _repo.Insertar(cliente);
                    MessageBox.Show("Cliente registrado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    _repo.Actualizar(cliente);
                    MessageBox.Show("Cliente actualizado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                LimpiarFormulario();
                CargarClientes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar cliente: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnEliminar_Click(object? sender, EventArgs e)
        {
            if (_idSeleccionado <= 0)
            {
                MessageBox.Show("Selecciona un cliente para dar de baja.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("¿Seguro que deseas dar de baja este cliente?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                try
                {
                    _repo.Eliminar(_idSeleccionado);
                    MessageBox.Show("Cliente dado de baja.");
                    LimpiarFormulario();
                    CargarClientes();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al dar de baja: {ex.Message}");
                }
            }
        }

        private void LimpiarFormulario()
        {
            _idSeleccionado = 0;
            txtDocumento.Clear();
            txtNombre.Clear();
            txtTelefono.Clear();
            txtEmail.Clear();
            txtDireccion.Clear();
            txtDocumento.Focus();
        }
    }
}
