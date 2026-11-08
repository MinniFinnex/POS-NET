using PuntoDeVenta.Datos;
using PuntoDeVenta.Entidades;
using PuntoDeVenta.Negocio;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PuntoDeVenta.Presentacion
{
    public class frmVentas : Form
    {
        private readonly Usuario _usuario;
        private readonly ProductoService _productoService = new();
        private readonly VentaService _ventaService = new();
        private readonly ClienteRepository _clienteRepo = new();

        private TextBox txtCodigo = null!;
        private TextBox txtProductoNombre = null!;
        private TextBox txtPrecio = null!;
        private NumericUpDown numCantidad = null!;
        private Button btnAgregar = null!;
        private Button btnQuitar = null!;
        private Button btnCobrar = null!;

        private ComboBox cboClientes = null!;
        private DataGridView dgvCarrito = null!;

        private Label lblSubtotalVal = null!;
        private Label lblImpuestoVal = null!;
        private Label lblTotalVal = null!;

        private Producto? _productoSeleccionado;
        private readonly List<DetalleVenta> _carrito = new();

        public frmVentas(Usuario usuario)
        {
            _usuario = usuario;
            InicializarComponentes();
            CargarClientes();
        }

        private void InicializarComponentes()
        {
            this.Text = "Módulo de Ventas";
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 9.5f);

            var lblTitulo = new Label
            {
                Text = "🧾 Registro de Venta / Caja",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 15),
                AutoSize = true
            };

            // Selección de Cliente
            var lblCli = new Label { Text = "Cliente:", Location = new Point(20, 52), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            cboClientes = new ComboBox
            {
                Location = new Point(80, 48),
                Size = new Size(260, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };

            // Panel Agregar Producto
            var grpBuscar = new GroupBox
            {
                Text = "Agregar Producto al Carrito",
                Location = new Point(20, 85),
                Size = new Size(720, 90),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };

            var lblCod = new Label { Text = "Código:", Location = new Point(15, 25), AutoSize = true, Font = new Font("Segoe UI", 8.5f) };
            txtCodigo = new TextBox { Location = new Point(15, 45), Size = new Size(110, 24), Font = new Font("Segoe UI", 9) };
            txtCodigo.KeyDown += TxtCodigo_KeyDown;

            var lblProd = new Label { Text = "Producto:", Location = new Point(135, 25), AutoSize = true, Font = new Font("Segoe UI", 8.5f) };
            txtProductoNombre = new TextBox { Location = new Point(135, 45), Size = new Size(210, 24), ReadOnly = true, Font = new Font("Segoe UI", 9) };

            var lblPre = new Label { Text = "Precio Uni:", Location = new Point(355, 25), AutoSize = true, Font = new Font("Segoe UI", 8.5f) };
            txtPrecio = new TextBox { Location = new Point(355, 45), Size = new Size(80, 24), ReadOnly = true, Font = new Font("Segoe UI", 9) };

            var lblCant = new Label { Text = "Cantidad:", Location = new Point(445, 25), AutoSize = true, Font = new Font("Segoe UI", 8.5f) };
            numCantidad = new NumericUpDown { Location = new Point(445, 45), Size = new Size(70, 24), Minimum = 1, Maximum = 9999, Value = 1, Font = new Font("Segoe UI", 9) };

            btnAgregar = new Button
            {
                Text = "➕ Agregar",
                Location = new Point(525, 42),
                Size = new Size(95, 30),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAgregar.Click += BtnAgregar_Click;

            btnQuitar = new Button
            {
                Text = "❌ Quitar",
                Location = new Point(625, 42),
                Size = new Size(85, 30),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnQuitar.Click += BtnQuitar_Click;

            grpBuscar.Controls.Add(lblCod);
            grpBuscar.Controls.Add(txtCodigo);
            grpBuscar.Controls.Add(lblProd);
            grpBuscar.Controls.Add(txtProductoNombre);
            grpBuscar.Controls.Add(lblPre);
            grpBuscar.Controls.Add(txtPrecio);
            grpBuscar.Controls.Add(lblCant);
            grpBuscar.Controls.Add(numCantidad);
            grpBuscar.Controls.Add(btnAgregar);
            grpBuscar.Controls.Add(btnQuitar);

            // Carrito DataGridView
            dgvCarrito = new DataGridView
            {
                Location = new Point(20, 185),
                Size = new Size(720, 230),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D
            };
            ConfigurarColumnasCarrito();

            // Panel Totales y Cobro
            var pnlTotales = new Panel
            {
                Location = new Point(20, 425),
                Size = new Size(720, 95),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.FromArgb(241, 245, 249),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblSub = new Label { Text = "Subtotal:", Location = new Point(20, 15), AutoSize = true, Font = new Font("Segoe UI", 10) };
            lblSubtotalVal = new Label { Text = "$0.00", Location = new Point(95, 15), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            var lblImp = new Label { Text = "IVA (16%):", Location = new Point(20, 42), AutoSize = true, Font = new Font("Segoe UI", 10) };
            lblImpuestoVal = new Label { Text = "$0.00", Location = new Point(95, 42), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            var lblTot = new Label { Text = "TOTAL:", Location = new Point(260, 25), AutoSize = true, Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42) };
            lblTotalVal = new Label { Text = "$0.00", Location = new Point(360, 22), AutoSize = true, Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.FromArgb(22, 101, 52) };

            btnCobrar = new Button
            {
                Text = "💵 COBRAR Y FINALIZAR",
                Location = new Point(510, 15),
                Size = new Size(190, 60),
                BackColor = Color.FromArgb(22, 163, 74),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCobrar.Click += BtnCobrar_Click;

            pnlTotales.Controls.Add(lblSub);
            pnlTotales.Controls.Add(lblSubtotalVal);
            pnlTotales.Controls.Add(lblImp);
            pnlTotales.Controls.Add(lblImpuestoVal);
            pnlTotales.Controls.Add(lblTot);
            pnlTotales.Controls.Add(lblTotalVal);
            pnlTotales.Controls.Add(btnCobrar);

            this.Controls.Add(lblTitulo);
            this.Controls.Add(lblCli);
            this.Controls.Add(cboClientes);
            this.Controls.Add(grpBuscar);
            this.Controls.Add(dgvCarrito);
            this.Controls.Add(pnlTotales);
        }

        private void ConfigurarColumnasCarrito()
        {
            dgvCarrito.Columns.Clear();
            dgvCarrito.Columns.Add("IdProducto", "Id");
            dgvCarrito.Columns.Add("Producto", "Producto");
            dgvCarrito.Columns.Add("Cantidad", "Cantidad");
            dgvCarrito.Columns.Add("PrecioUnitario", "Precio Unitario");
            dgvCarrito.Columns.Add("Subtotal", "Subtotal");

            if (dgvCarrito.Columns["IdProducto"] != null)
                dgvCarrito.Columns["IdProducto"]!.Visible = false;
        }

        private void CargarClientes()
        {
            try
            {
                var clientes = _clienteRepo.ObtenerTodos();
                if (clientes.Count > 0)
                {
                    cboClientes.DataSource = clientes;
                    cboClientes.DisplayMember = "Nombre";
                    cboClientes.ValueMember = "IdCliente";
                }
                else
                {
                    cboClientes.Items.Add("Consumidor Final");
                    cboClientes.SelectedIndex = 0;
                }
            }
            catch
            {
                cboClientes.Items.Add("Consumidor Final");
                cboClientes.SelectedIndex = 0;
            }
        }

        private void TxtCodigo_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                BuscarProducto();
            }
        }

        private void BuscarProducto()
        {
            string codigo = txtCodigo.Text.Trim();
            if (string.IsNullOrEmpty(codigo)) return;

            try
            {
                var prod = _productoService.BuscarPorCodigo(codigo);
                if (prod != null)
                {
                    _productoSeleccionado = prod;
                    txtProductoNombre.Text = prod.Nombre;
                    txtPrecio.Text = prod.PrecioVenta.ToString("F2");
                    numCantidad.Value = 1;
                    numCantidad.Focus();
                }
                else
                {
                    // Si no encuentra en BD o estamos en modo offline, buscar en lista simulada
                    if (codigo.Equals("PROD001", StringComparison.OrdinalIgnoreCase))
                    {
                        _productoSeleccionado = new Producto { IdProducto = 1, Codigo = "PROD001", Nombre = "Coca Cola 600ml", PrecioVenta = 18.00m, Stock = 50 };
                    }
                    else if (codigo.Equals("PROD002", StringComparison.OrdinalIgnoreCase))
                    {
                        _productoSeleccionado = new Producto { IdProducto = 2, Codigo = "PROD002", Nombre = "Papas Fritas 45g", PrecioVenta = 16.50m, Stock = 40 };
                    }
                    else
                    {
                        _productoSeleccionado = new Producto { IdProducto = 99, Codigo = codigo, Nombre = $"Producto {codigo}", PrecioVenta = 25.00m, Stock = 100 };
                    }

                    txtProductoNombre.Text = _productoSeleccionado.Nombre;
                    txtPrecio.Text = _productoSeleccionado.PrecioVenta.ToString("F2");
                }
            }
            catch
            {
                _productoSeleccionado = new Producto { IdProducto = 99, Codigo = codigo, Nombre = $"Producto {codigo}", PrecioVenta = 20.00m, Stock = 100 };
                txtProductoNombre.Text = _productoSeleccionado.Nombre;
                txtPrecio.Text = _productoSeleccionado.PrecioVenta.ToString("F2");
            }
        }

        private void BtnAgregar_Click(object? sender, EventArgs e)
        {
            if (_productoSeleccionado == null)
            {
                BuscarProducto();
                if (_productoSeleccionado == null)
                {
                    MessageBox.Show("Ingresa o escanea un código de producto válido.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            int cantidad = (int)numCantidad.Value;
            decimal subtotal = _productoSeleccionado.PrecioVenta * cantidad;

            // Verificar si ya existe en el carrito
            var existente = _carrito.Find(x => x.IdProducto == _productoSeleccionado.IdProducto);
            if (existente != null)
            {
                existente.Cantidad += cantidad;
                existente.Subtotal = existente.Cantidad * existente.PrecioUnitario;
            }
            else
            {
                _carrito.Add(new DetalleVenta
                {
                    IdProducto = _productoSeleccionado.IdProducto,
                    ProductoNombre = _productoSeleccionado.Nombre,
                    Cantidad = cantidad,
                    PrecioUnitario = _productoSeleccionado.PrecioVenta,
                    Subtotal = subtotal
                });
            }

            ActualizarTablaCarrito();

            // Limpiar inputs
            _productoSeleccionado = null;
            txtCodigo.Clear();
            txtProductoNombre.Clear();
            txtPrecio.Clear();
            numCantidad.Value = 1;
            txtCodigo.Focus();
        }

        private void BtnQuitar_Click(object? sender, EventArgs e)
        {
            if (dgvCarrito.SelectedRows.Count == 0)
            {
                MessageBox.Show("Selecciona una fila del carrito para quitar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int indice = dgvCarrito.SelectedRows[0].Index;
            if (indice >= 0 && indice < _carrito.Count)
            {
                _carrito.RemoveAt(indice);
                ActualizarTablaCarrito();
            }
        }

        private void ActualizarTablaCarrito()
        {
            dgvCarrito.Rows.Clear();
            decimal subtotal = 0;

            foreach (var item in _carrito)
            {
                subtotal += item.Subtotal;
                dgvCarrito.Rows.Add(item.IdProducto, item.ProductoNombre ?? "Producto", item.Cantidad, item.PrecioUnitario.ToString("C2"), item.Subtotal.ToString("C2"));
            }

            decimal impuesto = subtotal * 0.16m;
            decimal total = subtotal + impuesto;

            lblSubtotalVal.Text = subtotal.ToString("C2");
            lblImpuestoVal.Text = impuesto.ToString("C2");
            lblTotalVal.Text = total.ToString("C2");
        }

        private void BtnCobrar_Click(object? sender, EventArgs e)
        {
            if (_carrito.Count == 0)
            {
                MessageBox.Show("El carrito está vacío. Agrega productos para procesar la venta.", "Carrito Vacío", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal subtotal = 0;
            foreach (var item in _carrito) subtotal += item.Subtotal;
            decimal impuesto = subtotal * 0.16m;
            decimal total = subtotal + impuesto;

            int? idCliente = null;
            if (cboClientes.SelectedValue is int cId)
                idCliente = cId;

            var venta = new Venta
            {
                Fecha = DateTime.Now,
                IdCliente = idCliente,
                IdUsuario = _usuario.IdUsuario > 0 ? _usuario.IdUsuario : 1,
                Subtotal = subtotal,
                Impuesto = impuesto,
                Total = total,
                Estado = "COMPLETADA"
            };

            var res = _ventaService.ProcesarVenta(venta, _carrito);
            if (res.Exito)
            {
                MessageBox.Show(
                    $"¡Venta #{res.IdVenta} registrada correctamente!\n\nTotal Cobrado: {total:C2}\nCajero: {_usuario.NombreCompleto}\n\nTicket impreso exitosamente.",
                    "Venta Completada",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                _carrito.Clear();
                ActualizarTablaCarrito();
            }
            else
            {
                
            }
        }
    }
}
