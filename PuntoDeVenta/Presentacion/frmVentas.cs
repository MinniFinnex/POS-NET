using PuntoDeVenta.Datos;
using PuntoDeVenta.Entidades;
using PuntoDeVenta.Negocio;
using PuntoDeVenta.Utilidades;
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

        private Label lblEstadoCaja = null!;
        private Button btnGestionCaja = null!;
        private ComboBox cboClientes = null!;
        private Caja? _cajaActiva;

        private TextBox txtCodigo = null!;
        private NumericUpDown numCantidad = null!;
        private Button btnAgregar = null!;

        private DataGridView dgvCarrito = null!;
        private Button btnSumar = null!;
        private Button btnRestar = null!;
        private Button btnQuitar = null!;
        private Button btnVaciar = null!;

        private Label lblTotalVal = null!;
        private TextBox txtPagaCon = null!;
        private Button btnPagoExacto = null!;
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
            this.KeyPreview = true;
            this.KeyDown += FrmVentas_KeyDown;

            InicializarComponentes();
            CargarClientes();
            VerificarEstadoCaja();
        }

        private void FrmVentas_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F12)
            {
                e.Handled = true;
                if (!txtPagaCon.Focused)
                {
                    txtPagaCon.Focus();
                    txtPagaCon.SelectAll();
                }
                else
                {
                    BtnCobrar_Click(btnCobrar, EventArgs.Empty);
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                txtCodigo.Clear();
                txtCodigo.Focus();
            }
        }

        private void InicializarComponentes()
        {
            this.Text = "Ventas";
            this.BackColor = TemaUI.FondoClaro;
            this.Font = new Font("Segoe UI", 9.5f);

            // Barra superior
            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = TemaUI.Blanco,
                Padding = new Padding(15, 8, 15, 8)
            };

            var pnlTopBorder = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = TemaUI.Borde
            };
            pnlTop.Controls.Add(pnlTopBorder);

            lblEstadoCaja = new Label
            {
                Text = "Caja...",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = TemaUI.TextoOscuro,
                Location = new Point(15, 14),
                AutoSize = true
            };

            btnGestionCaja = new Button
            {
                Text = "Caja",
                Location = new Point(190, 9),
                Size = new Size(75, 28)
            };
            TemaUI.AplicarBotonAzul(btnGestionCaja, new Font("Segoe UI", 8.5f, FontStyle.Bold));
            btnGestionCaja.Click += (s, e) => AbrirControlCaja();

            var pnlCliente = new Panel
            {
                Dock = DockStyle.Right,
                Width = 320,
                Height = 36
            };

            var lblCli = new Label
            {
                Text = "Cliente:",
                Location = new Point(10, 8),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = TemaUI.TextoOscuro
            };

            cboClientes = new ComboBox
            {
                Location = new Point(65, 5),
                Size = new Size(245, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9f)
            };

            pnlCliente.Controls.Add(lblCli);
            pnlCliente.Controls.Add(cboClientes);

            pnlTop.Controls.Add(lblEstadoCaja);
            pnlTop.Controls.Add(btnGestionCaja);
            pnlTop.Controls.Add(pnlCliente);

            // Panel Lateral de Cobro
            var pnlCobro = new Panel
            {
                Dock = DockStyle.Right,
                Width = 280,
                BackColor = TemaUI.Blanco,
                Padding = new Padding(16)
            };

            var pnlCobroBorder = new Panel
            {
                Dock = DockStyle.Left,
                Width = 1,
                BackColor = TemaUI.Borde
            };
            pnlCobro.Controls.Add(pnlCobroBorder);

            var cardTotal = new Panel
            {
                Location = new Point(16, 16),
                Size = new Size(246, 75),
                BackColor = TemaUI.FondoClaro,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10)
            };

            var lblHeaderTotal = new Label
            {
                Text = "TOTAL",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = TemaUI.TextoSutil,
                Location = new Point(10, 8),
                AutoSize = true
            };

            lblTotalVal = new Label
            {
                Text = "$0.00",
                Font = new Font("Segoe UI", 22f, FontStyle.Bold),
                ForeColor = TemaUI.AzulMarino,
                Location = new Point(6, 26),
                AutoSize = true
            };

            cardTotal.Controls.Add(lblHeaderTotal);
            cardTotal.Controls.Add(lblTotalVal);

            var lblPaga = new Label
            {
                Text = "Paga con:",
                Location = new Point(16, 105),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = TemaUI.TextoOscuro
            };

            txtPagaCon = new TextBox
            {
                Location = new Point(16, 128),
                Size = new Size(140, 30),
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                PlaceholderText = "0.00"
            };
            txtPagaCon.TextChanged += (s, e) => CalcularCambio();
            txtPagaCon.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    BtnCobrar_Click(btnCobrar, EventArgs.Empty);
                }
            };

            btnPagoExacto = new Button
            {
                Text = "Exacto",
                Location = new Point(164, 127),
                Size = new Size(98, 32)
            };
            TemaUI.AplicarBotonAzul(btnPagoExacto, new Font("Segoe UI", 8.5f, FontStyle.Bold));
            btnPagoExacto.Click += (s, e) =>
            {
                decimal total = ObtenerTotalActual();
                txtPagaCon.Text = total.ToString("F2");
                txtPagaCon.Focus();
                txtPagaCon.SelectAll();
            };

            var cardCambio = new Panel
            {
                Location = new Point(16, 172),
                Size = new Size(246, 68),
                BackColor = TemaUI.FondoClaro,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10)
            };

            var lblHeaderCambio = new Label
            {
                Text = "CAMBIO",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = TemaUI.TextoSutil,
                Location = new Point(10, 8),
                AutoSize = true
            };

            lblCambioVal = new Label
            {
                Text = "$0.00",
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = TemaUI.AzulMarino,
                Location = new Point(8, 26),
                AutoSize = true
            };

            cardCambio.Controls.Add(lblHeaderCambio);
            cardCambio.Controls.Add(lblCambioVal);

            btnCobrar = new Button
            {
                Text = "Cobrar",
                Location = new Point(16, 255),
                Size = new Size(246, 48)
            };
            TemaUI.AplicarBotonAzul(btnCobrar, new Font("Segoe UI", 11.5f, FontStyle.Bold));
            btnCobrar.Click += BtnCobrar_Click;

            pnlCobro.Controls.Add(cardTotal);
            pnlCobro.Controls.Add(lblPaga);
            pnlCobro.Controls.Add(txtPagaCon);
            pnlCobro.Controls.Add(btnPagoExacto);
            pnlCobro.Controls.Add(cardCambio);
            pnlCobro.Controls.Add(btnCobrar);

            // Panel Central
            var pnlCentro = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15)
            };

            // Barra de entrada de producto
            var pnlEscaneo = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = TemaUI.Blanco,
                Padding = new Padding(12, 8, 12, 8)
            };

            var pnlEscaneoBorder = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = TemaUI.Borde
            };
            pnlEscaneo.Controls.Add(pnlEscaneoBorder);

            var lblCod = new Label
            {
                Text = "Código:",
                Location = new Point(12, 6),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = TemaUI.TextoOscuro
            };

            txtCodigo = new TextBox
            {
                Location = new Point(12, 24),
                Size = new Size(250, 26),
                Font = new Font("Segoe UI", 10f),
                PlaceholderText = "Escribir o escanear código..."
            };
            txtCodigo.KeyDown += TxtCodigo_KeyDown;

            var lblCant = new Label
            {
                Text = "Cant:",
                Location = new Point(272, 6),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = TemaUI.TextoOscuro
            };

            numCantidad = new NumericUpDown
            {
                Location = new Point(272, 24),
                Size = new Size(60, 26),
                Font = new Font("Segoe UI", 10f),
                Minimum = 1,
                Maximum = 9999,
                Value = 1
            };

            btnAgregar = new Button
            {
                Text = "Agregar",
                Location = new Point(342, 22),
                Size = new Size(90, 30)
            };
            TemaUI.AplicarBotonAzul(btnAgregar, new Font("Segoe UI", 9f, FontStyle.Bold));
            btnAgregar.Click += (s, e) => AgregarProductoAlCarrito();

            pnlEscaneo.Controls.Add(lblCod);
            pnlEscaneo.Controls.Add(txtCodigo);
            pnlEscaneo.Controls.Add(lblCant);
            pnlEscaneo.Controls.Add(numCantidad);
            pnlEscaneo.Controls.Add(btnAgregar);

            // DataGridView del Carrito
            dgvCarrito = new DataGridView
            {
                Dock = DockStyle.Fill
            };
            TemaUI.AplicarEstiloGrid(dgvCarrito);
            ConfigurarColumnasCarrito();

            // Barra inferior de botones del Carrito
            var pnlBotonesCarrito = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 42,
                BackColor = TemaUI.Blanco,
                Padding = new Padding(10, 6, 10, 6)
            };

            var pnlBotonesBorder = new Panel
            {
                Dock = DockStyle.Top,
                Height = 1,
                BackColor = TemaUI.Borde
            };
            pnlBotonesCarrito.Controls.Add(pnlBotonesBorder);

            btnSumar = new Button
            {
                Text = "+1",
                Location = new Point(10, 6),
                Size = new Size(55, 28)
            };
            TemaUI.AplicarBotonAzul(btnSumar, new Font("Segoe UI", 8.5f, FontStyle.Bold));
            btnSumar.Click += (s, e) => ModificarCantidadSeleccionada(1);

            btnRestar = new Button
            {
                Text = "-1",
                Location = new Point(72, 6),
                Size = new Size(55, 28)
            };
            TemaUI.AplicarBotonAzul(btnRestar, new Font("Segoe UI", 8.5f, FontStyle.Bold));
            btnRestar.Click += (s, e) => ModificarCantidadSeleccionada(-1);

            btnQuitar = new Button
            {
                Text = "Quitar",
                Location = new Point(135, 6),
                Size = new Size(70, 28)
            };
            TemaUI.AplicarBotonAzul(btnQuitar, new Font("Segoe UI", 8.5f, FontStyle.Bold));
            btnQuitar.Click += (s, e) => QuitarSeleccionado();

            btnVaciar = new Button
            {
                Text = "Vaciar",
                Location = new Point(212, 6),
                Size = new Size(70, 28)
            };
            TemaUI.AplicarBotonAzul(btnVaciar, new Font("Segoe UI", 8.5f, FontStyle.Bold));
            btnVaciar.Click += (s, e) => VaciarCarrito();

            pnlBotonesCarrito.Controls.Add(btnSumar);
            pnlBotonesCarrito.Controls.Add(btnRestar);
            pnlBotonesCarrito.Controls.Add(btnQuitar);
            pnlBotonesCarrito.Controls.Add(btnVaciar);

            var pnlCarritoContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 10, 0, 0)
            };
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
            dgvCarrito.Columns.Add("Stock", "Stock");
            dgvCarrito.Columns.Add("Cantidad", "Cant.");
            dgvCarrito.Columns.Add("PrecioUnitario", "Precio");
            dgvCarrito.Columns.Add("Subtotal", "Total");

            dgvCarrito.Columns["IdProducto"]!.Visible = false;
            dgvCarrito.Columns["Codigo"]!.Width = 110;
            dgvCarrito.Columns["Nombre"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvCarrito.Columns["Stock"]!.Width = 70;
            dgvCarrito.Columns["Cantidad"]!.Width = 65;
            dgvCarrito.Columns["PrecioUnitario"]!.Width = 90;
            dgvCarrito.Columns["Subtotal"]!.Width = 100;

            dgvCarrito.Columns["Stock"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCarrito.Columns["Cantidad"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCarrito.Columns["PrecioUnitario"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvCarrito.Columns["Subtotal"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            dgvCarrito.Columns["PrecioUnitario"]!.DefaultCellStyle.Format = "C2";
            dgvCarrito.Columns["Subtotal"]!.DefaultCellStyle.Format = "C2";
            dgvCarrito.Columns["Subtotal"]!.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        }

        public void VerificarEstadoCaja()
        {
            try
            {
                _cajaActiva = _cajaService.ObtenerCajaAbierta(_usuario.IdUsuario > 0 ? _usuario.IdUsuario : 1);
                if (_cajaActiva != null)
                {
                    lblEstadoCaja.Text = $"Caja #{_cajaActiva.IdCaja} (Abierta)";
                    lblEstadoCaja.ForeColor = TemaUI.TextoOscuro;
                    btnGestionCaja.Text = "Caja";
                }
                else
                {
                    lblEstadoCaja.Text = "Caja Cerrada";
                    lblEstadoCaja.ForeColor = TemaUI.TextoOscuro;
                    btnGestionCaja.Text = "Abrir";
                }
            }
            catch
            {
                lblEstadoCaja.Text = "Sin conexión a caja";
                lblEstadoCaja.ForeColor = TemaUI.TextoSutil;
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
                MessageBox.Show($"Error al consultar el producto:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (prod == null)
            {
                MessageBox.Show($"No se encontró producto con código '{codigo}'.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCodigo.SelectAll();
                txtCodigo.Focus();
                return;
            }

            var itemExistente = _carrito.Find(x => x.IdProducto == prod.IdProducto);
            int cantidadActualEnCarrito = itemExistente?.Cantidad ?? 0;
            int cantidadFinal = cantidadActualEnCarrito + cantidadAAgregar;

            if (cantidadFinal > prod.Stock)
            {
                MessageBox.Show(
                    $"Stock insuficiente para '{prod.Nombre}'.\nDisponible: {prod.Stock}\nEn carrito: {cantidadActualEnCarrito}",
                    "Stock",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                txtCodigo.SelectAll();
                txtCodigo.Focus();
                return;
            }

            if (itemExistente != null)
            {
                itemExistente.Cantidad = cantidadFinal;
            }
            else
            {
                _carrito.Add(new ItemCarrito
                {
                    IdProducto = prod.IdProducto,
                    Codigo = prod.Codigo,
                    Nombre = prod.Nombre,
                    StockDisponible = prod.Stock,
                    Cantidad = cantidadAAgregar,
                    PrecioUnitario = prod.PrecioVenta
                });
            }

            ActualizarTablaCarrito();

            txtCodigo.Clear();
            numCantidad.Value = 1;
            txtCodigo.Focus();
        }

        private void ModificarCantidadSeleccionada(int delta)
        {
            if (dgvCarrito.SelectedRows.Count == 0) return;

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
                    MessageBox.Show($"Stock máximo disponible: {item.StockDisponible}.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            if (MessageBox.Show("¿Vaciar la lista actual?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _carrito.Clear();
                ActualizarTablaCarrito();
                txtCodigo.Focus();
            }
        }

        private void ActualizarTablaCarrito()
        {
            dgvCarrito.Rows.Clear();
            decimal total = 0;

            foreach (var item in _carrito)
            {
                total += item.Subtotal;
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

            lblTotalVal.Text = total.ToString("C2");
            CalcularCambio();
        }

        private decimal ObtenerTotalActual()
        {
            return _carrito.Sum(x => x.Subtotal);
        }

        private void CalcularCambio()
        {
            decimal total = ObtenerTotalActual();
            if (decimal.TryParse(txtPagaCon.Text, out decimal pagaCon) && pagaCon >= total)
            {
                decimal cambio = pagaCon - total;
                lblCambioVal.Text = cambio.ToString("C2");
            }
            else
            {
                lblCambioVal.Text = "$0.00";
            }
        }

        private void BtnCobrar_Click(object? sender, EventArgs e)
        {
            if (_carrito.Count == 0)
            {
                MessageBox.Show("No hay productos en la lista.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCodigo.Focus();
                return;
            }

            VerificarEstadoCaja();
            if (_cajaActiva == null)
            {
                var resp = MessageBox.Show(
                    "No hay una caja abierta.\n¿Deseas abrir la caja ahora?",
                    "Caja Requerida",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (resp == DialogResult.Yes)
                {
                    AbrirControlCaja();
                    if (_cajaActiva == null)
                    {
                        MessageBox.Show("Se requiere abrir la caja antes de cobrar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                else
                {
                    return;
                }
            }

            decimal total = ObtenerTotalActual();
            decimal pagaCon = 0;

            if (!string.IsNullOrWhiteSpace(txtPagaCon.Text))
            {
                if (!decimal.TryParse(txtPagaCon.Text, out pagaCon) || pagaCon < total)
                {
                    MessageBox.Show($"El monto (${pagaCon:F2}) es menor al total (${total:F2}).", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                Subtotal = total,
                Impuesto = 0m,
                Total = total,
                Estado = "COMPLETADA"
            };

            var detalles = _carrito.Select(item => new DetalleVenta
            {
                IdProducto = item.IdProducto,
                ProductoNombre = item.Nombre,
                Cantidad = item.Cantidad,
                PrecioUnitario = item.PrecioUnitario,
                Subtotal = item.Subtotal
            }).ToList();

            var res = _ventaService.ProcesarVenta(venta, detalles);
            if (res.Exito)
            {
                string mensajeTicket =
                    "TICKET DE VENTA\n" +
                    "-----------------------------------------\n" +
                    $"Folio: #{res.IdVenta}\n" +
                    $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}\n" +
                    $"Cajero: {_usuario.NombreCompleto}\n" +
                    $"Cliente: {clienteNombre}\n" +
                    "-----------------------------------------\n" +
                    string.Join("\n", _carrito.Select(i => $"{i.Cantidad}x {i.Nombre} = {i.Subtotal:C2}")) + "\n" +
                    "-----------------------------------------\n" +
                    $"TOTAL: {total:C2}\n" +
                    $"PAGA CON: {pagaCon:C2}\n" +
                    $"CAMBIO: {cambio:C2}\n" +
                    "-----------------------------------------\n" +
                    "Gracias por su compra.";

                MessageBox.Show(
                    mensajeTicket,
                    $"Venta #{res.IdVenta} Registrada",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                _carrito.Clear();
                txtPagaCon.Clear();
                lblCambioVal.Text = "$0.00";

                ActualizarTablaCarrito();
                txtCodigo.Focus();
                VerificarEstadoCaja();
            }
            else
            {
                MessageBox.Show(
                    $"No se pudo procesar la venta:\n{res.Mensaje}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
