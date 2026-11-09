using PuntoDeVenta.Datos;
using PuntoDeVenta.Entidades;
using PuntoDeVenta.Negocio;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PuntoDeVenta.Presentacion
{
    public class frmVentas : Form
    {
        private readonly Usuario _usuario;
        private readonly ProductoService _productoService = new();
        private readonly VentaService _ventaService = new();
        private readonly ClienteRepository _clienteRepo = new();
        private readonly CajaService _cajaService = new();

        // Controles de cabecera y estado
        private Label lblEstadoCaja = null!;
        private Button btnGestionCaja = null!;
        private ComboBox cboClientes = null!;
        private Caja? _cajaActiva;

        // Controles de entrada de productos
        private TextBox txtCodigo = null!;
        private NumericUpDown numCantidad = null!;
        private Button btnAgregar = null!;
        private Label lblUltimoAgregado = null!;

        // Carrito
        private DataGridView dgvCarrito = null!;
        private Button btnSumar = null!;
        private Button btnRestar = null!;
        private Button btnQuitar = null!;
        private Button btnVaciar = null!;

        // Panel de Cobro
        private Label lblSubtotalVal = null!;
        private Label lblImpuestoVal = null!;
        private Label lblTotalVal = null!;
        private TextBox txtPagaCon = null!;
        private Label lblCambioVal = null!;
        private Button btnCobrar = null!;

        private class ItemCarrito
        {
            public int IdProducto { get; set; }
            public string Codigo { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public int StockDisponible { get; set; }
            public int Cantidad { get; set; }
            public decimal PrecioUnitario { get; set; }
            public decimal Subtotal => Cantidad * PrecioUnitario;
        }

        private readonly List<ItemCarrito> _carrito = new();

        public frmVentas(Usuario usuario)
        {
            _usuario = usuario;
            InicializarComponentes();
            CargarClientes();
            VerificarEstadoCaja();
        }

        private void InicializarComponentes()
        {
            this.Text = "Módulo de Ventas";
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 9.5f);

            // 1. Barra Superior de Estado (Caja y Cliente)
            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(15, 10, 15, 10),
                BorderStyle = BorderStyle.FixedSingle
            };

            lblEstadoCaja = new Label
            {
                Text = "Verificando estado de caja...",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Location = new Point(15, 18),
                AutoSize = true
            };

            btnGestionCaja = new Button
            {
                Text = " Control de Caja",
                Location = new Point(320, 14),
                Size = new Size(130, 30),
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(30, 41, 59),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnGestionCaja.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnGestionCaja.Click += (s, e) => AbrirControlCaja();

            var lblCli = new Label
            {
                Text = "Cliente:",
                Location = new Point(470, 18),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            cboClientes = new ComboBox
            {
                Location = new Point(530, 15),
                Size = new Size(220, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };

            pnlTop.Controls.Add(lblEstadoCaja);
            pnlTop.Controls.Add(btnGestionCaja);
            pnlTop.Controls.Add(lblCli);
            pnlTop.Controls.Add(cboClientes);

            // 2. Panel Derecho de Cobro y Totales
            var pnlCobro = new Panel
            {
                Dock = DockStyle.Right,
                Width = 270,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(15)
            };

            var lblCobroTitulo = new Label
            {
                Text = "Resumen de Cuenta",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(15, 15),
                AutoSize = true
            };

            var lblSub = new Label { Text = "Subtotal:", Location = new Point(15, 60), AutoSize = true, Font = new Font("Segoe UI", 9.5f) };
            lblSubtotalVal = new Label { Text = "$0.00", Location = new Point(130, 60), AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), TextAlign = ContentAlignment.MiddleRight };

            var lblImp = new Label { Text = "IVA (16%):", Location = new Point(15, 90), AutoSize = true, Font = new Font("Segoe UI", 9.5f) };
            lblImpuestoVal = new Label { Text = "$0.00", Location = new Point(130, 90), AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), TextAlign = ContentAlignment.MiddleRight };

            var sep1 = new Label { BorderStyle = BorderStyle.Fixed3D, Location = new Point(15, 125), Size = new Size(235, 2) };

            var lblTot = new Label { Text = "TOTAL A PAGAR", Location = new Point(15, 135), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
            lblTotalVal = new Label
            {
                Text = "$0.00",
                Location = new Point(15, 160),
                AutoSize = true,
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.FromArgb(22, 163, 74)
            };

            var sep2 = new Label { BorderStyle = BorderStyle.Fixed3D, Location = new Point(15, 215), Size = new Size(235, 2) };

            var lblPaga = new Label { Text = "Paga con ($):", Location = new Point(15, 230), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            txtPagaCon = new TextBox { Location = new Point(15, 252), Size = new Size(235, 26), Font = new Font("Segoe UI", 11, FontStyle.Bold) };
            txtPagaCon.TextChanged += (s, e) => CalcularCambio();

            var lblCambio = new Label { Text = "Cambio:", Location = new Point(15, 290), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            lblCambioVal = new Label { Text = "$0.00", Location = new Point(80, 288), AutoSize = true, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235) };

            btnCobrar = new Button
            {
                Text = " COBRAR",
                Location = new Point(15, 335),
                Size = new Size(235, 55),
                BackColor = Color.FromArgb(22, 163, 74),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCobrar.FlatAppearance.BorderSize = 0;
            btnCobrar.Click += BtnCobrar_Click;

            pnlCobro.Controls.Add(lblCobroTitulo);
            pnlCobro.Controls.Add(lblSub);
            pnlCobro.Controls.Add(lblSubtotalVal);
            pnlCobro.Controls.Add(lblImp);
            pnlCobro.Controls.Add(lblImpuestoVal);
            pnlCobro.Controls.Add(sep1);
            pnlCobro.Controls.Add(lblTot);
            pnlCobro.Controls.Add(lblTotalVal);
            pnlCobro.Controls.Add(sep2);
            pnlCobro.Controls.Add(lblPaga);
            pnlCobro.Controls.Add(txtPagaCon);
            pnlCobro.Controls.Add(lblCambio);
            pnlCobro.Controls.Add(lblCambioVal);
            pnlCobro.Controls.Add(btnCobrar);

            // 3. Panel Central (Escaneo de Producto y Carrito)
            var pnlCentro = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15)
            };

            
            var pnlEscaneo = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(12)
            };

            var lblCod = new Label { Text = "Código de Barras:", Location = new Point(12, 10), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            txtCodigo = new TextBox
            {
                Location = new Point(12, 32),
                Size = new Size(240, 27),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                PlaceholderText = "Escanea o escribe código..."
            };
            txtCodigo.KeyDown += TxtCodigo_KeyDown;

            var lblCant = new Label { Text = "Cantidad:", Location = new Point(265, 10), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            numCantidad = new NumericUpDown
            {
                Location = new Point(265, 32),
                Size = new Size(70, 27),
                Font = new Font("Segoe UI", 10),
                Minimum = 1,
                Maximum = 9999,
                Value = 1
            };

            btnAgregar = new Button
            {
                Text = "➕ Agregar al Carrito",
                Location = new Point(345, 30),
                Size = new Size(150, 31),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAgregar.FlatAppearance.BorderSize = 0;
            btnAgregar.Click += (s, e) => AgregarProductoAlCarrito();

            lblUltimoAgregado = new Label
            {
                Text = "Listo para escanear productos.",
                Location = new Point(505, 37),
                AutoSize = true,
                ForeColor = Color.FromArgb(100, 116, 139),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic)
            };

            pnlEscaneo.Controls.Add(lblCod);
            pnlEscaneo.Controls.Add(txtCodigo);
            pnlEscaneo.Controls.Add(lblCant);
            pnlEscaneo.Controls.Add(numCantidad);
            pnlEscaneo.Controls.Add(btnAgregar);
            pnlEscaneo.Controls.Add(lblUltimoAgregado);

            // Carrito DataGridView
            dgvCarrito = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                AllowUserToAddRows = false
            };
            ConfigurarColumnasCarrito();

            // Barra inferior del Carrito 
            var pnlBotonesCarrito = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 45,
                BackColor = Color.FromArgb(241, 245, 249),
                Padding = new Padding(5)
            };

            btnSumar = new Button
            {
                Text = "➕ Sumar (+1)",
                Location = new Point(10, 7),
                Size = new Size(110, 30),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSumar.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnSumar.Click += (s, e) => ModificarCantidadSeleccionada(1);

            btnRestar = new Button
            {
                Text = "➖ Restar (-1)",
                Location = new Point(130, 7),
                Size = new Size(110, 30),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRestar.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnRestar.Click += (s, e) => ModificarCantidadSeleccionada(-1);

            btnQuitar = new Button
            {
                Text = "Quitar Ítem",
                Location = new Point(250, 7),
                Size = new Size(110, 30),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(185, 28, 28),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnQuitar.FlatAppearance.BorderColor = Color.FromArgb(252, 165, 165);
            btnQuitar.Click += (s, e) => QuitarSeleccionado();

            btnVaciar = new Button
            {
                Text = " Vaciar Carrito",
                Location = new Point(370, 7),
                Size = new Size(120, 30),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(100, 116, 139),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f),
                Cursor = Cursors.Hand
            };
            btnVaciar.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnVaciar.Click += (s, e) => VaciarCarrito();

            pnlBotonesCarrito.Controls.Add(btnSumar);
            pnlBotonesCarrito.Controls.Add(btnRestar);
            pnlBotonesCarrito.Controls.Add(btnQuitar);
            pnlBotonesCarrito.Controls.Add(btnVaciar);

            var pnlCarritoContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 10, 0, 0) };
            pnlCarritoContainer.Controls.Add(dgvCarrito);
            pnlCarritoContainer.Controls.Add(pnlBotonesCarrito);

            pnlCentro.Controls.Add(pnlCarritoContainer);
            pnlCentro.Controls.Add(pnlEscaneo);

            this.Controls.Add(pnlCentro);
            this.Controls.Add(pnlCobro);
            this.Controls.Add(pnlTop);
        }

        private void ConfigurarColumnasCarrito()
        {
            dgvCarrito.Columns.Clear();
            dgvCarrito.Columns.Add("IdProducto", "Id");
            dgvCarrito.Columns.Add("Codigo", "Código");
            dgvCarrito.Columns.Add("Nombre", "Producto");
            dgvCarrito.Columns.Add("Stock", "Stock Disp.");
            dgvCarrito.Columns.Add("Cantidad", "Cant.");
            dgvCarrito.Columns.Add("PrecioUnitario", "Precio Unit.");
            dgvCarrito.Columns.Add("Subtotal", "Subtotal");

            dgvCarrito.Columns["IdProducto"]!.Visible = false;
            dgvCarrito.Columns["Codigo"]!.Width = 100;
            dgvCarrito.Columns["Nombre"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvCarrito.Columns["Stock"]!.Width = 90;
            dgvCarrito.Columns["Cantidad"]!.Width = 70;
            dgvCarrito.Columns["PrecioUnitario"]!.Width = 95;
            dgvCarrito.Columns["Subtotal"]!.Width = 100;

            dgvCarrito.Columns["PrecioUnitario"]!.DefaultCellStyle.Format = "C2";
            dgvCarrito.Columns["Subtotal"]!.DefaultCellStyle.Format = "C2";
        }

        public void VerificarEstadoCaja()
        {
            try
            {
                _cajaActiva = _cajaService.ObtenerCajaAbierta(_usuario.IdUsuario > 0 ? _usuario.IdUsuario : 1);
                if (_cajaActiva != null)
                {
                    lblEstadoCaja.Text = $" Caja Abierta #{_cajaActiva.IdCaja} | Monto Inicial: {_cajaActiva.MontoInicial:C2} ({_cajaActiva.FechaApertura:HH:mm})";
                    lblEstadoCaja.ForeColor = Color.FromArgb(22, 101, 52);
                }
                else
                {
                    lblEstadoCaja.Text = "🔴 CAJA CERRADA (Se requiere apertura para registrar ventas)";
                    lblEstadoCaja.ForeColor = Color.FromArgb(185, 28, 28);
                }
            }
            catch (Exception)
            {
                lblEstadoCaja.Text = "⚠️ Error de conexión a Base de Datos";
                lblEstadoCaja.ForeColor = Color.FromArgb(185, 28, 28);
            }
        }

        private void AbrirControlCaja()
        {
            var frm = new frmCaja(_usuario);
            frm.ShowDialog();
            VerificarEstadoCaja();
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
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar clientes desde la base de datos:\n{ex.Message}", "Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboClientes.Items.Add("Consumidor Final");
                cboClientes.SelectedIndex = 0;
            }
        }

        private void TxtCodigo_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                AgregarProductoAlCarrito();
            }
        }

        private void AgregarProductoAlCarrito()
        {
            string codigo = txtCodigo.Text.Trim();
            if (string.IsNullOrEmpty(codigo)) return;

            int cantidadAAgregar = (int)numCantidad.Value;
            if (cantidadAAgregar <= 0) cantidadAAgregar = 1;

            Producto? prod = null;
            try
            {
                prod = _productoService.BuscarPorCodigo(codigo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al consultar el producto en la base de datos:\n{ex.Message}", "Error de Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (prod == null)
            {
                MessageBox.Show($"No se encontró ningún producto con el código '{codigo}' en la base de datos.", "Producto No Encontrado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCodigo.SelectAll();
                txtCodigo.Focus();
                return;
            }

            // Buscar si ya existe en el carrito
            var itemExistente = _carrito.Find(x => x.IdProducto == prod.IdProducto);
            int cantidadActualEnCarrito = itemExistente?.Cantidad ?? 0;
            int cantidadFinal = cantidadActualEnCarrito + cantidadAAgregar;

            // Validación de stock: no permitir acumular más del stock real
            if (cantidadFinal > prod.Stock)
            {
                MessageBox.Show(
                    $"Stock insuficiente para '{prod.Nombre}'.\n\nStock Disponible: {prod.Stock}\nEn carrito: {cantidadActualEnCarrito}\nIntentando agregar: {cantidadAAgregar}",
                    "Stock Superado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                txtCodigo.SelectAll();
                txtCodigo.Focus();
                return;
            }

            if (itemExistente != null)
            {
                // ACUMULAR CANTIDAD EN EL MISMO ÍTEM
                itemExistente.Cantidad = cantidadFinal;
                lblUltimoAgregado.Text = $"Acumulado: {prod.Nombre} (Total: {itemExistente.Cantidad})";
            }
            else
            {
                // AGREGAR NUEVO ÍTEM
                _carrito.Add(new ItemCarrito
                {
                    IdProducto = prod.IdProducto,
                    Codigo = prod.Codigo,
                    Nombre = prod.Nombre,
                    StockDisponible = prod.Stock,
                    Cantidad = cantidadAAgregar,
                    PrecioUnitario = prod.PrecioVenta
                });
                lblUltimoAgregado.Text = $"Agregado: {prod.Nombre} x{cantidadAAgregar}";
            }

            ActualizarTablaCarrito();

            // Limpiar código y restablecer cantidad a 1 para el siguiente escaneo
            txtCodigo.Clear();
            numCantidad.Value = 1;
            txtCodigo.Focus();
        }

        private void ModificarCantidadSeleccionada(int delta)
        {
            if (dgvCarrito.SelectedRows.Count == 0)
            {
                MessageBox.Show("Selecciona un producto del carrito.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int index = dgvCarrito.SelectedRows[0].Index;
            if (index < 0 || index >= _carrito.Count) return;

            var item = _carrito[index];
            int nuevaCantidad = item.Cantidad + delta;

            if (nuevaCantidad <= 0)
            {
                _carrito.RemoveAt(index);
            }
            else
            {
                if (nuevaCantidad > item.StockDisponible)
                {
                    MessageBox.Show($"No puedes superar el stock disponible ({item.StockDisponible}) de este producto.", "Stock Máximo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                item.Cantidad = nuevaCantidad;
            }

            ActualizarTablaCarrito();
            if (index < dgvCarrito.Rows.Count)
                dgvCarrito.Rows[index].Selected = true;
        }

        private void QuitarSeleccionado()
        {
            if (dgvCarrito.SelectedRows.Count == 0) return;
            int index = dgvCarrito.SelectedRows[0].Index;
            if (index >= 0 && index < _carrito.Count)
            {
                _carrito.RemoveAt(index);
                ActualizarTablaCarrito();
            }
        }

        private void VaciarCarrito()
        {
            if (_carrito.Count == 0) return;
            var resp = MessageBox.Show("¿Seguro que deseas vaciar todos los productos del carrito actual?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (resp == DialogResult.Yes)
            {
                _carrito.Clear();
                ActualizarTablaCarrito();
                txtCodigo.Focus();
            }
        }

        private void ActualizarTablaCarrito()
        {
            dgvCarrito.Rows.Clear();
            decimal subtotal = 0;

            foreach (var item in _carrito)
            {
                subtotal += item.Subtotal;
                dgvCarrito.Rows.Add(
                    item.IdProducto,
                    item.Codigo,
                    item.Nombre,
                    item.StockDisponible,
                    item.Cantidad,
                    item.PrecioUnitario,
                    item.Subtotal
                );
            }

            decimal impuesto = subtotal * 0.16m;
            decimal total = subtotal + impuesto;

            lblSubtotalVal.Text = subtotal.ToString("C2");
            lblImpuestoVal.Text = impuesto.ToString("C2");
            lblTotalVal.Text = total.ToString("C2");

            CalcularCambio();
        }

        private decimal ObtenerTotalActual()
        {
            decimal subtotal = _carrito.Sum(x => x.Subtotal);
            return subtotal + (subtotal * 0.16m);
        }

        private void CalcularCambio()
        {
            decimal total = ObtenerTotalActual();
            if (decimal.TryParse(txtPagaCon.Text, out decimal pagaCon) && pagaCon >= total)
            {
                decimal cambio = pagaCon - total;
                lblCambioVal.Text = cambio.ToString("C2");
                lblCambioVal.ForeColor = Color.FromArgb(22, 101, 52);
            }
            else
            {
                lblCambioVal.Text = "$0.00";
                lblCambioVal.ForeColor = Color.FromArgb(100, 116, 139);
            }
        }

        private void BtnCobrar_Click(object? sender, EventArgs e)
        {
            if (_carrito.Count == 0)
            {
                MessageBox.Show("El carrito de compras está vacío. Agrega al menos un producto para cobrar.", "Carrito Vacío", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCodigo.Focus();
                return;
            }

            // DETALLE 4: Comprobar si la caja está abierta
            VerificarEstadoCaja();
            if (_cajaActiva == null)
            {
                var resp = MessageBox.Show(
                    "Actualmente no hay una caja abierta para este turno.\n\n¿Deseas abrir la caja ahora para poder registrar la venta?",
                    "Apertura de Caja Requerida",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (resp == DialogResult.Yes)
                {
                    AbrirControlCaja();
                    if (_cajaActiva == null)
                    {
                        MessageBox.Show("Debes realizar la apertura de caja antes de cobrar la venta.", "Caja Cerrada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                else
                {
                    return;
                }
            }

            decimal subtotal = _carrito.Sum(x => x.Subtotal);
            decimal impuesto = subtotal * 0.16m;
            decimal total = subtotal + impuesto;

            decimal pagaCon = 0;
            if (!string.IsNullOrWhiteSpace(txtPagaCon.Text))
            {
                if (!decimal.TryParse(txtPagaCon.Text, out pagaCon) || pagaCon < total)
                {
                    MessageBox.Show($"El monto ingresado en 'Paga con' (${pagaCon:F2}) es menor al total de la cuenta (${total:F2}).", "Monto Insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPagaCon.Focus();
                    return;
                }
            }
            else
            {
                pagaCon = total;
            }

            decimal cambio = pagaCon - total;

            int? idCliente = null;
            string clienteNombre = "Consumidor Final";
            if (cboClientes.SelectedItem is Cliente c)
            {
                idCliente = c.IdCliente;
                clienteNombre = c.Nombre;
            }

            var venta = new Venta
            {
                Fecha = DateTime.Now,
                IdCliente = idCliente,
                IdUsuario = _usuario.IdUsuario > 0 ? _usuario.IdUsuario : 1,
                IdCaja = _cajaActiva?.IdCaja,
                Subtotal = subtotal,
                Impuesto = impuesto,
                Total = total,
                Estado = "COMPLETADA"
            };

            var detalles = _carrito.Select(c => new DetalleVenta
            {
                IdProducto = c.IdProducto,
                ProductoNombre = c.Nombre,
                Cantidad = c.Cantidad,
                PrecioUnitario = c.PrecioUnitario,
                Subtotal = c.Subtotal
            }).ToList();

            var res = _ventaService.ProcesarVenta(venta, detalles);
            if (res.Exito)
            {
                string mensajeTicket = 
                    "=========================================\n" +
                    "           TICKET DE VENTA               \n" +
                    "=========================================\n" +
                    $" Folio de Venta : #{res.IdVenta}\n" +
                    $" Fecha / Hora   : {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n" +
                    $" Cajero         : {_usuario.NombreCompleto}\n" +
                    $" Cliente        : {clienteNombre}\n" +
                    $" Caja / Turno   : #{_cajaActiva?.IdCaja ?? 1}\n" +
                    "-----------------------------------------\n" +
                    string.Join("\n", _carrito.Select(i => $" {i.Cantidad}x {i.Nombre} (${i.PrecioUnitario:F2}) = {i.Subtotal:C2}")) + "\n" +
                    "-----------------------------------------\n" +
                    $" Subtotal : {subtotal:C2}\n" +
                    $" IVA (16%): {impuesto:C2}\n" +
                    $" TOTAL    : {total:C2}\n" +
                    $" Paga con : {pagaCon:C2}\n" +
                    $" Cambio   : {cambio:C2}\n" +
                    "=========================================\n" +
                    "      ¡Gracias por su compra!            \n" +
                    "=========================================";

                MessageBox.Show(
                    mensajeTicket,
                    $"¡Venta #{res.IdVenta} Exitosa!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                _carrito.Clear();
                txtPagaCon.Clear();
                lblCambioVal.Text = "$0.00";
                lblUltimoAgregado.Text = "Venta anterior cobrada con éxito.";
                ActualizarTablaCarrito();
                txtCodigo.Focus();
                VerificarEstadoCaja();
            }
            else
            {
                MessageBox.Show(
                    $"No se pudo procesar la venta:\n\n{res.Mensaje}",
                    "Error al Cobrar",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
