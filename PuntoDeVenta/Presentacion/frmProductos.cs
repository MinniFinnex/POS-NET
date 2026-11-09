using PuntoDeVenta.Entidades;
using PuntoDeVenta.Negocio;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PuntoDeVenta.Presentacion
{
    public class frmProductos : Form
    {
        private readonly Usuario? _usuario;
        private readonly ProductoService _service = new();

        private DataGridView dgvProductos = null!;
        private TextBox txtBuscar = null!;
        private TextBox txtCodigo = null!;
        private TextBox txtNombre = null!;
        private TextBox txtDescripcion = null!;
        private TextBox txtPrecioVenta = null!;
        private TextBox txtStock = null!;
        private ComboBox cboCategoria = null!;
        private Button btnGuardar = null!;
        private Button btnEliminar = null!;
        private Button btnNuevo = null!;

        private int _idSeleccionado = 0;
        private List<Producto> _todosLosProductos = new();

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
            if (_usuario != null && _usuario.IdRol == 2)
            {
                btnGuardar.Enabled = false;
                btnEliminar.Enabled = false;
                btnNuevo.Enabled = false;
                txtCodigo.ReadOnly = true;
                txtNombre.ReadOnly = true;
                txtDescripcion.ReadOnly = true;
                txtPrecioVenta.ReadOnly = true;
                txtStock.ReadOnly = true;
                cboCategoria.Enabled = false;

                btnGuardar.Text = "🔒 Solo Admin";
                btnEliminar.Text = "🔒 Solo Admin";
            }
        }

        private void InicializarComponentes()
        {
            this.Text = "Catálogo y Gestión de Productos";
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 9.5f);

            // Encabezado
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.White,
                Padding = new Padding(20, 10, 20, 10)
            };

            var lblTitulo = new Label
            {
                Text = _usuario?.IdRol == 2 ? " Consulta de Productos e Inventario" : " Catálogo y Gestión de Productos",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(20, 15),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblTitulo);

            // Panel de Datos (Formulario Simple)
            var pnlForm = new Panel
            {
                Location = new Point(20, 70),
                Size = new Size(760, 180),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // Fila 1: Código, Nombre, Categoría
            var lblCod = new Label { Text = "Código de Barras:", Location = new Point(15, 12), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            txtCodigo = new TextBox { Location = new Point(15, 32), Size = new Size(130, 25) };

            var lblNom = new Label { Text = "Nombre del Producto:", Location = new Point(160, 12), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            txtNombre = new TextBox { Location = new Point(160, 32), Size = new Size(260, 25) };

            var lblCat = new Label { Text = "Categoría:", Location = new Point(435, 12), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            cboCategoria = new ComboBox { Location = new Point(435, 32), Size = new Size(150, 25), DropDownStyle = ComboBoxStyle.DropDownList };

            var lblPrecio = new Label { Text = "Precio ($):", Location = new Point(600, 12), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            txtPrecioVenta = new TextBox { Location = new Point(600, 32), Size = new Size(70, 25), Text = "0.00" };

            var lblStock = new Label { Text = "Stock:", Location = new Point(685, 12), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            txtStock = new TextBox { Location = new Point(685, 32), Size = new Size(60, 25), Text = "0" };

            // Fila 2: Descripción (Detalle 1)
            var lblDesc = new Label { Text = "Descripción / Detalles:", Location = new Point(15, 65), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            txtDescripcion = new TextBox { Location = new Point(15, 85), Size = new Size(405, 25) };

            // Botones de acción
            btnNuevo = new Button
            {
                Text = " Limpiar",
                Location = new Point(435, 82),
                Size = new Size(95, 32),
                BackColor = Color.FromArgb(100, 116, 139),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnNuevo.FlatAppearance.BorderSize = 0;
            btnNuevo.Click += (s, e) => LimpiarFormulario();

            btnGuardar = new Button
            {
                Text = " Guardar",
                Location = new Point(540, 82),
                Size = new Size(100, 32),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;

            btnEliminar = new Button
            {
                Text = " Eliminar",
                Location = new Point(650, 82),
                Size = new Size(95, 32),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnEliminar.FlatAppearance.BorderSize = 0;
            btnEliminar.Click += BtnEliminar_Click;

            // Fila 3: Barra de búsqueda rápida
            var lblBuscar = new Label { Text = " Buscar en catálogo:", Location = new Point(15, 130), AutoSize = true, Font = new Font("Segoe UI", 9) };
            txtBuscar = new TextBox { Location = new Point(160, 127), Size = new Size(260, 25), PlaceholderText = "Escribe código, nombre o descripción..." };
            txtBuscar.TextChanged += (s, e) => FiltrarProductos();

            pnlForm.Controls.Add(lblCod);
            pnlForm.Controls.Add(txtCodigo);
            pnlForm.Controls.Add(lblNom);
            pnlForm.Controls.Add(txtNombre);
            pnlForm.Controls.Add(lblCat);
            pnlForm.Controls.Add(cboCategoria);
            pnlForm.Controls.Add(lblPrecio);
            pnlForm.Controls.Add(txtPrecioVenta);
            pnlForm.Controls.Add(lblStock);
            pnlForm.Controls.Add(txtStock);
            pnlForm.Controls.Add(lblDesc);
            pnlForm.Controls.Add(txtDescripcion);
            pnlForm.Controls.Add(btnNuevo);
            pnlForm.Controls.Add(btnGuardar);
            pnlForm.Controls.Add(btnEliminar);
            pnlForm.Controls.Add(lblBuscar);
            pnlForm.Controls.Add(txtBuscar);

            // Tabla DataGridView
            dgvProductos = new DataGridView
            {
                Location = new Point(20, 260),
                Size = new Size(760, 260),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                RowHeadersVisible = false,
                AllowUserToAddRows = false
            };
            dgvProductos.CellClick += DgvProductos_CellClick;

            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlForm);
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
                _todosLosProductos = _service.ListarProductos();
                MostrarProductosEnGrid(_todosLosProductos);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los productos desde la base de datos:\n{ex.Message}", "Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _todosLosProductos = new List<Producto>();
                MostrarProductosEnGrid(_todosLosProductos);
            }
        }

        private void MostrarProductosEnGrid(List<Producto> lista)
        {
            dgvProductos.DataSource = null;
            dgvProductos.DataSource = lista;

            if (dgvProductos.Columns["IdProducto"] != null) dgvProductos.Columns["IdProducto"]!.Visible = false;
            if (dgvProductos.Columns["IdCategoria"] != null) dgvProductos.Columns["IdCategoria"]!.Visible = false;
            if (dgvProductos.Columns["PrecioCompra"] != null) dgvProductos.Columns["PrecioCompra"]!.Visible = false;
            if (dgvProductos.Columns["StockMinimo"] != null) dgvProductos.Columns["StockMinimo"]!.Visible = false;
            if (dgvProductos.Columns["Activo"] != null) dgvProductos.Columns["Activo"]!.Visible = false;

            if (dgvProductos.Columns["Codigo"] != null) dgvProductos.Columns["Codigo"]!.HeaderText = "Código";
            if (dgvProductos.Columns["Nombre"] != null) dgvProductos.Columns["Nombre"]!.HeaderText = "Nombre";
            if (dgvProductos.Columns["Descripcion"] != null) dgvProductos.Columns["Descripcion"]!.HeaderText = "Descripción";
            if (dgvProductos.Columns["CategoriaNombre"] != null) dgvProductos.Columns["CategoriaNombre"]!.HeaderText = "Categoría";
            if (dgvProductos.Columns["PrecioVenta"] != null)
            {
                dgvProductos.Columns["PrecioVenta"]!.HeaderText = "Precio";
                dgvProductos.Columns["PrecioVenta"]!.DefaultCellStyle.Format = "C2";
            }
            if (dgvProductos.Columns["Stock"] != null) dgvProductos.Columns["Stock"]!.HeaderText = "Stock Disp.";
        }

        private void FiltrarProductos()
        {
            string filtro = txtBuscar.Text.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(filtro))
            {
                MostrarProductosEnGrid(_todosLosProductos);
                return;
            }

            var filtrados = _todosLosProductos.Where(p =>
                (p.Codigo != null && p.Codigo.ToLowerInvariant().Contains(filtro)) ||
                (p.Nombre != null && p.Nombre.ToLowerInvariant().Contains(filtro)) ||
                (p.Descripcion != null && p.Descripcion.ToLowerInvariant().Contains(filtro))
            ).ToList();

            MostrarProductosEnGrid(filtrados);
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
                txtDescripcion.Text = p.Descripcion ?? string.Empty;
                txtPrecioVenta.Text = p.PrecioVenta.ToString("F2");
                txtStock.Text = p.Stock.ToString();
                if (cboCategoria.DataSource != null)
                    cboCategoria.SelectedValue = p.IdCategoria;
            }
            else
            {
                txtCodigo.Text = fila.Cells["Codigo"].Value?.ToString() ?? "";
                txtNombre.Text = fila.Cells["Nombre"].Value?.ToString() ?? "";
                txtDescripcion.Text = fila.Cells["Descripcion"]?.Value?.ToString() ?? "";
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

            if (string.IsNullOrWhiteSpace(txtCodigo.Text))
            {
                MessageBox.Show("Ingresa el código del producto.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCodigo.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Ingresa el nombre del producto.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombre.Focus();
                return;
            }

            if (!decimal.TryParse(txtPrecioVenta.Text, out decimal precio) || precio < 0)
            {
                MessageBox.Show("Ingresa un precio válido mayor o igual a 0.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrecioVenta.Focus();
                return;
            }

            if (!int.TryParse(txtStock.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("Ingresa una cantidad de stock válida mayor o igual a 0.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStock.Focus();
                return;
            }

            int idCat = 1;
            if (cboCategoria.SelectedValue is int cId)
                idCat = cId;

            var producto = new Producto
            {
                IdProducto = _idSeleccionado,
                Codigo = txtCodigo.Text.Trim(),
                Nombre = txtNombre.Text.Trim(),
                Descripcion = string.IsNullOrWhiteSpace(txtDescripcion.Text) ? null : txtDescripcion.Text.Trim(),
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
                MessageBox.Show("Selecciona un producto de la tabla para eliminar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("¿Seguro que deseas dar de baja este producto?", "Confirmar Eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                var res = _service.Eliminar(_idSeleccionado);
                MessageBox.Show(res.Mensaje, "Resultado", MessageBoxButtons.OK, res.Exito ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                LimpiarFormulario();
                CargarProductos();
            }
        }

        private void LimpiarFormulario()
        {
            _idSeleccionado = 0;
            txtCodigo.Clear();
            txtNombre.Clear();
            txtDescripcion.Clear();
            txtPrecioVenta.Text = "0.00";
            txtStock.Text = "0";
            txtBuscar.Clear();
            txtCodigo.Focus();
        }
    }
}
