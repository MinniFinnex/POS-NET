using PuntoDeVenta.Entidades;
using PuntoDeVenta.Negocio;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PuntoDeVenta.Presentacion
{
    public class frmProductos : Form
    {
        private readonly Usuario? _usuario;
        private DataGridView dgvProductos = null!;
        private TextBox txtCodigo = null!;
        private TextBox txtNombre = null!;
        private TextBox txtPrecioVenta = null!;
        private TextBox txtStock = null!;
        private ComboBox cboCategoria = null!;
        private Button btnGuardar = null!;
        private Button btnEliminar = null!;
        private Button btnNuevo = null!;
        private int _idSeleccionado = 0;

        private readonly ProductoService _service = new();

        public frmProductos(Usuario? usuario = null)
        {
            _usuario = usuario;
            InicializarComponentes();
            AplicarPermisosPorRol();
            CargarCategorias();
            CargarProductos();
        }

        private void AplicarPermisosPorRol()
        {
            // Si el rol es Cajero (IdRol == 2), el módulo de productos opera en modo consulta
            if (_usuario != null && _usuario.IdRol == 2)
            {
                btnGuardar.Enabled = false;
                btnEliminar.Enabled = false;
                btnNuevo.Enabled = false;
                txtCodigo.ReadOnly = true;
                txtNombre.ReadOnly = true;
                txtPrecioVenta.ReadOnly = true;
                txtStock.ReadOnly = true;
                cboCategoria.Enabled = false;

                btnGuardar.Text = "🔒 Solo Admin";
                btnEliminar.Text = "🔒 Solo Admin";
            }
        }

        private void InicializarComponentes()
        {
            this.Text = "Gestión de Productos";
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 9.5f);

            var lblTitulo = new Label
            {
                Text = _usuario?.IdRol == 2 
                    ? "📦 Consulta de Catálogo e Inventario (Modo Cajero)" 
                    : "📦 Gestión de Productos e Inventario (Administrador)",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 15),
                AutoSize = true
            };

            // Panel de Edición
            var grpDatos = new GroupBox
            {
                Text = "Detalle del Producto",
                Location = new Point(20, 50),
                Size = new Size(720, 140),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };

            var lblCod = new Label { Text = "Código:", Location = new Point(15, 30), AutoSize = true, Font = new Font("Segoe UI", 9) };
            txtCodigo = new TextBox { Location = new Point(15, 52), Size = new Size(120, 25), Font = new Font("Segoe UI", 9) };

            var lblNom = new Label { Text = "Nombre:", Location = new Point(145, 30), AutoSize = true, Font = new Font("Segoe UI", 9) };
            txtNombre = new TextBox { Location = new Point(145, 52), Size = new Size(200, 25), Font = new Font("Segoe UI", 9) };

            var lblCat = new Label { Text = "Categoría:", Location = new Point(355, 30), AutoSize = true, Font = new Font("Segoe UI", 9) };
            cboCategoria = new ComboBox { Location = new Point(355, 52), Size = new Size(130, 25), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };

            var lblPrecio = new Label { Text = "Precio ($):", Location = new Point(495, 30), AutoSize = true, Font = new Font("Segoe UI", 9) };
            txtPrecioVenta = new TextBox { Location = new Point(495, 52), Size = new Size(95, 25), Font = new Font("Segoe UI", 9), Text = "0.00" };

            var lblStock = new Label { Text = "Stock:", Location = new Point(600, 30), AutoSize = true, Font = new Font("Segoe UI", 9) };
            txtStock = new TextBox { Location = new Point(600, 52), Size = new Size(95, 25), Font = new Font("Segoe UI", 9), Text = "0" };

            btnNuevo = new Button
            {
                Text = "Nuevo",
                Location = new Point(355, 92),
                Size = new Size(100, 32),
                BackColor = Color.FromArgb(100, 116, 139),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnNuevo.Click += (s, e) => LimpiarFormulario();

            btnGuardar = new Button
            {
                Text = "💾 Guardar",
                Location = new Point(470, 92),
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
                Text = "🗑️ Eliminar",
                Location = new Point(590, 92),
                Size = new Size(105, 32),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnEliminar.Click += BtnEliminar_Click;

            grpDatos.Controls.Add(lblCod);
            grpDatos.Controls.Add(txtCodigo);
            grpDatos.Controls.Add(lblNom);
            grpDatos.Controls.Add(txtNombre);
            grpDatos.Controls.Add(lblCat);
            grpDatos.Controls.Add(cboCategoria);
            grpDatos.Controls.Add(lblPrecio);
            grpDatos.Controls.Add(txtPrecioVenta);
            grpDatos.Controls.Add(lblStock);
            grpDatos.Controls.Add(txtStock);
            grpDatos.Controls.Add(btnNuevo);
            grpDatos.Controls.Add(btnGuardar);
            grpDatos.Controls.Add(btnEliminar);

            // Tabla de productos
            dgvProductos = new DataGridView
            {
                Location = new Point(20, 205),
                Size = new Size(720, 320),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D
            };
            dgvProductos.CellClick += DgvProductos_CellClick;

            this.Controls.Add(lblTitulo);
            this.Controls.Add(grpDatos);
            this.Controls.Add(dgvProductos);
        }

        private void CargarCategorias()
        {
            try
            {
                var cats = _service.ListarCategorias();
                if (cats.Count > 0)
                {
                    cboCategoria.DataSource = cats;
                    cboCategoria.DisplayMember = "Nombre";
                    cboCategoria.ValueMember = "IdCategoria";
                }
                else
                {
                    cboCategoria.Items.Add(new Categoria { IdCategoria = 1, Nombre = "General" });
                    cboCategoria.SelectedIndex = 0;
                }
            }
            catch
            {
                cboCategoria.Items.Add("General");
                cboCategoria.SelectedIndex = 0;
            }
        }

        private void CargarProductos()
        {
            try
            {
                var lista = _service.ListarProductos();
                dgvProductos.DataSource = lista;
                if (dgvProductos.Columns["IdProducto"] != null)
                    dgvProductos.Columns["IdProducto"]!.Visible = false;
                if (dgvProductos.Columns["Activo"] != null)
                    dgvProductos.Columns["Activo"]!.Visible = false;
            }
            catch
            {
                dgvProductos.Columns.Clear();
                dgvProductos.Columns.Add("Codigo", "Código");
                dgvProductos.Columns.Add("Nombre", "Nombre");
                dgvProductos.Columns.Add("PrecioVenta", "Precio Venta");
                dgvProductos.Columns.Add("Stock", "Stock");

                dgvProductos.Rows.Add("PROD001", "Coca Cola 600ml", "18.00", "50");
                dgvProductos.Rows.Add("PROD002", "Papas Fritas 45g", "16.50", "40");
                dgvProductos.Rows.Add("PROD003", "Agua Mineral 500ml", "12.00", "60");
            }
        }

        private void DgvProductos_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvProductos.Rows.Count) return;
            var fila = dgvProductos.Rows[e.RowIndex];

            if (fila.DataBoundItem is Producto p)
            {
                _idSeleccionado = p.IdProducto;
                txtCodigo.Text = p.Codigo;
                txtNombre.Text = p.Nombre;
                txtPrecioVenta.Text = p.PrecioVenta.ToString("F2");
                txtStock.Text = p.Stock.ToString();
                if (cboCategoria.DataSource != null)
                    cboCategoria.SelectedValue = p.IdCategoria;
            }
            else
            {
                txtCodigo.Text = fila.Cells["Codigo"].Value?.ToString() ?? "";
                txtNombre.Text = fila.Cells["Nombre"].Value?.ToString() ?? "";
                txtPrecioVenta.Text = fila.Cells["PrecioVenta"].Value?.ToString() ?? "";
                txtStock.Text = fila.Cells["Stock"].Value?.ToString() ?? "";
            }
        }

        private void BtnGuardar_Click(object? sender, EventArgs e)
        {
            if (_usuario != null && _usuario.IdRol == 2)
            {
                MessageBox.Show("Solo los Administradores tienen permisos para modificar productos o precios.", "Permiso Denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrecioVenta.Text, out decimal precio))
            {
                MessageBox.Show("Ingresa un precio válido.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtStock.Text, out int stock))
            {
                MessageBox.Show("Ingresa una cantidad de stock válida.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int idCat = 1;
            if (cboCategoria.SelectedValue is int cId)
                idCat = cId;

            var producto = new Producto
            {
                IdProducto = _idSeleccionado,
                Codigo = txtCodigo.Text,
                Nombre = txtNombre.Text,
                PrecioVenta = precio,
                Stock = stock,
                IdCategoria = idCat
            };

            var res = _service.Guardar(producto);
            MessageBox.Show(res.Mensaje, res.Exito ? "Éxito" : "Aviso", MessageBoxButtons.OK, res.Exito ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

            if (res.Exito)
            {
                LimpiarFormulario();
                CargarProductos();
            }
        }

        private void BtnEliminar_Click(object? sender, EventArgs e)
        {
            if (_usuario != null && _usuario.IdRol == 2)
            {
                MessageBox.Show("Solo los Administradores tienen permisos para eliminar productos.", "Permiso Denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_idSeleccionado <= 0)
            {
                MessageBox.Show("Selecciona un producto para eliminar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("¿Seguro que deseas dar de baja este producto?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                var res = _service.Eliminar(_idSeleccionado);
                MessageBox.Show(res.Mensaje);
                LimpiarFormulario();
                CargarProductos();
            }
        }

        private void LimpiarFormulario()
        {
            _idSeleccionado = 0;
            txtCodigo.Clear();
            txtNombre.Clear();
            txtPrecioVenta.Text = "0.00";
            txtStock.Text = "0";
            txtCodigo.Focus();
        }
    }
}
