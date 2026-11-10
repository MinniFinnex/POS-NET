using PuntoDeVenta.Datos;
using PuntoDeVenta.Entidades;
using PuntoDeVenta.Utilidades;
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
            this.Text = "Clientes";
            this.BackColor = TemaUI.FondoClaro;
            this.Font = new Font("Segoe UI", 9.5f);

            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = TemaUI.Blanco,
                Padding = new Padding(20, 10, 20, 10)
            };

            var pnlHeaderBorder = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = TemaUI.Borde
            };
            pnlHeader.Controls.Add(pnlHeaderBorder);

            var lblTitulo = new Label
            {
                Text = "Directorio de Clientes",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = TemaUI.TextoOscuro,
                Location = new Point(20, 14),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblTitulo);

            var pnlForm = new Panel
            {
                Location = new Point(20, 65),
                Size = new Size(760, 145),
                BackColor = TemaUI.Blanco,
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var lblDoc = new Label { Text = "Documento:", Location = new Point(15, 12), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = TemaUI.TextoOscuro };
            txtDocumento = new TextBox { Location = new Point(15, 32), Size = new Size(130, 25) };

            var lblNom = new Label { Text = "Nombre Completo:", Location = new Point(160, 12), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = TemaUI.TextoOscuro };
            txtNombre = new TextBox { Location = new Point(160, 32), Size = new Size(260, 25) };

            var lblTel = new Label { Text = "Teléfono:", Location = new Point(435, 12), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = TemaUI.TextoOscuro };
            txtTelefono = new TextBox { Location = new Point(435, 32), Size = new Size(140, 25) };

            var lblEmail = new Label { Text = "Email:", Location = new Point(590, 12), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = TemaUI.TextoOscuro };
            txtEmail = new TextBox { Location = new Point(590, 32), Size = new Size(150, 25) };

            var lblDir = new Label { Text = "Dirección:", Location = new Point(15, 68), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = TemaUI.TextoOscuro };
            txtDireccion = new TextBox { Location = new Point(15, 88), Size = new Size(405, 25) };

            btnNuevo = new Button
            {
                Text = "Limpiar",
                Location = new Point(435, 85),
                Size = new Size(95, 30)
            };
            TemaUI.AplicarBotonSecundario(btnNuevo, new Font("Segoe UI", 9, FontStyle.Bold));
            btnNuevo.Click += (s, e) => LimpiarFormulario();

            btnGuardar = new Button
            {
                Text = "Guardar",
                Location = new Point(540, 85),
                Size = new Size(100, 30)
            };
            TemaUI.AplicarBotonAzul(btnGuardar, new Font("Segoe UI", 9, FontStyle.Bold));
            btnGuardar.Click += BtnGuardar_Click;

            btnEliminar = new Button
            {
                Text = "Eliminar",
                Location = new Point(650, 85),
                Size = new Size(90, 30)
            };
            TemaUI.AplicarBotonSecundario(btnEliminar, new Font("Segoe UI", 9, FontStyle.Bold));
            btnEliminar.Click += BtnEliminar_Click;

            pnlForm.Controls.Add(lblDoc);
            pnlForm.Controls.Add(txtDocumento);
            pnlForm.Controls.Add(lblNom);
            pnlForm.Controls.Add(txtNombre);
            pnlForm.Controls.Add(lblTel);
            pnlForm.Controls.Add(txtTelefono);
            pnlForm.Controls.Add(lblEmail);
            pnlForm.Controls.Add(txtEmail);
            pnlForm.Controls.Add(lblDir);
            pnlForm.Controls.Add(txtDireccion);
            pnlForm.Controls.Add(btnNuevo);
            pnlForm.Controls.Add(btnGuardar);
            pnlForm.Controls.Add(btnEliminar);

            dgvClientes = new DataGridView
            {
                Location = new Point(20, 225),
                Size = new Size(760, 320),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            TemaUI.AplicarEstiloGrid(dgvClientes);
            dgvClientes.CellClick += DgvClientes_CellClick;

            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlForm);
            this.Controls.Add(dgvClientes);
        }

        private void CargarClientes()
        {
            try
            {
                var lista = _repo.ObtenerTodos();
                dgvClientes.DataSource = null;
                dgvClientes.DataSource = lista;
                if (dgvClientes.Columns["IdCliente"] != null)
                    dgvClientes.Columns["IdCliente"]!.Visible = false;
                if (dgvClientes.Columns["Activo"] != null)
                    dgvClientes.Columns["Activo"]!.Visible = false;

                if (dgvClientes.Columns["Documento"] != null) dgvClientes.Columns["Documento"]!.HeaderText = "Documento";
                if (dgvClientes.Columns["Nombre"] != null) dgvClientes.Columns["Nombre"]!.HeaderText = "Nombre";
                if (dgvClientes.Columns["Telefono"] != null) dgvClientes.Columns["Telefono"]!.HeaderText = "Teléfono";
                if (dgvClientes.Columns["Email"] != null) dgvClientes.Columns["Email"]!.HeaderText = "Email";
                if (dgvClientes.Columns["Direccion"] != null) dgvClientes.Columns["Direccion"]!.HeaderText = "Dirección";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los clientes:\n{ex.Message}", "Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                txtNombre.Focus();
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
                MessageBox.Show($"Error al guardar cliente:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    MessageBox.Show("Cliente dado de baja.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LimpiarFormulario();
                    CargarClientes();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al dar de baja:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
