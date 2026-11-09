using PuntoDeVenta.Entidades;
using PuntoDeVenta.Negocio;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PuntoDeVenta.Presentacion
{
    public class frmReportes : Form
    {
        private readonly Usuario _usuario;
        private readonly VentaService _ventaService = new();

        private DateTimePicker dtpInicio = null!;
        private DateTimePicker dtpFin = null!;
        private Button btnHoy = null!;
        private Button btnSemana = null!;
        private Button btnMes = null!;
        private Button btnFiltrar = null!;
        private Button btnVerTicket = null!;

        // Tarjetas KPI
        private Label lblKpiTotalVentas = null!;
        private Label lblKpiTransacciones = null!;
        private Label lblKpiTicketPromedio = null!;
        private Label lblKpiArticulos = null!;

        // Pestañas
        private TabControl tabControl = null!;
        private DataGridView dgvVentas = null!;
        private DataGridView dgvDetalle = null!;
        private DataGridView dgvTopProductos = null!;

        private List<Venta> _ventasActuales = new();

        public frmReportes(Usuario usuario)
        {
            _usuario = usuario;
            InicializarComponentes();
            EstablecerFiltroHoy();
        }

        private void InicializarComponentes()
        {
            this.Text = "Historial y Reportes de Ventas";
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 9.5f);

            // 1. Encabezado
            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.White,
                Padding = new Padding(20, 10, 20, 10),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblTitulo = new Label
            {
                Text = " Historial y Reportes de Ventas",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(20, 15),
                AutoSize = true
            };
            pnlTop.Controls.Add(lblTitulo);

            // 2. Barra de Filtros
            var pnlFiltros = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(15, 10, 15, 10)
            };

            btnHoy = CrearBotonFiltro("Hoy", (s, e) => EstablecerFiltroHoy());
            btnHoy.Location = new Point(15, 16);

            btnSemana = CrearBotonFiltro("Últimos 7 Días", (s, e) => EstablecerFiltroSemana());
            btnSemana.Location = new Point(75, 16);

            btnMes = CrearBotonFiltro("Este Mes", (s, e) => EstablecerFiltroMes());
            btnMes.Location = new Point(185, 16);

            var lblDesde = new Label { Text = "Desde:", Location = new Point(285, 20), AutoSize = true, Font = new Font("Segoe UI", 9) };
            dtpInicio = new DateTimePicker { Location = new Point(335, 18), Size = new Size(115, 25), Format = DateTimePickerFormat.Short };

            var lblHasta = new Label { Text = "Hasta:", Location = new Point(460, 20), AutoSize = true, Font = new Font("Segoe UI", 9) };
            dtpFin = new DateTimePicker { Location = new Point(510, 18), Size = new Size(115, 25), Format = DateTimePickerFormat.Short };

            btnFiltrar = new Button
            {
                Text = " Consultar",
                Location = new Point(635, 15),
                Size = new Size(110, 31),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnFiltrar.FlatAppearance.BorderSize = 0;
            btnFiltrar.Click += (s, e) => CargarDatos();

            btnVerTicket = new Button
            {
                Text = " Ver Ticket",
                Location = new Point(755, 15),
                Size = new Size(110, 31),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnVerTicket.FlatAppearance.BorderSize = 0;
            btnVerTicket.Click += BtnVerTicket_Click;

            pnlFiltros.Controls.Add(btnHoy);
            pnlFiltros.Controls.Add(btnSemana);
            pnlFiltros.Controls.Add(btnMes);
            pnlFiltros.Controls.Add(lblDesde);
            pnlFiltros.Controls.Add(dtpInicio);
            pnlFiltros.Controls.Add(lblHasta);
            pnlFiltros.Controls.Add(dtpFin);
            pnlFiltros.Controls.Add(btnFiltrar);
            pnlFiltros.Controls.Add(btnVerTicket);

            // 3. Tarjetas 
            var pnlKpis = new Panel
            {
                Dock = DockStyle.Top,
                Height = 85,
                Padding = new Padding(15, 10, 15, 5)
            };

            Panel CrearTarjetaKpi(string titulo, out Label lblValor, Color colorAcento, int left)
            {
                var card = new Panel
                {
                    Location = new Point(left, 5),
                    Size = new Size(200, 68),
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle
                };

                var lblT = new Label
                {
                    Text = titulo,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(100, 116, 139),
                    Location = new Point(12, 10),
                    AutoSize = true
                };

                lblValor = new Label
                {
                    Text = "$0.00",
                    Font = new Font("Segoe UI", 13, FontStyle.Bold),
                    ForeColor = colorAcento,
                    Location = new Point(12, 32),
                    AutoSize = true
                };

                card.Controls.Add(lblT);
                card.Controls.Add(lblValor);
                return card;
            }

            pnlKpis.Controls.Add(CrearTarjetaKpi("TOTAL VENTAS", out lblKpiTotalVentas, Color.FromArgb(22, 163, 74), 15));
            pnlKpis.Controls.Add(CrearTarjetaKpi("Nº VENTAS", out lblKpiTransacciones, Color.FromArgb(37, 99, 235), 230));
            pnlKpis.Controls.Add(CrearTarjetaKpi("TICKET PROMEDIO", out lblKpiTicketPromedio, Color.FromArgb(124, 58, 237), 445));
            pnlKpis.Controls.Add(CrearTarjetaKpi("ARTÍCULOS VENDIDOS", out lblKpiArticulos, Color.FromArgb(217, 119, 6), 660));

            // 4. Pestañas de Reporte
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5f)
            };

            var tabHistorial = new TabPage("🧾 Historial de Ventas y Tickets");
            tabHistorial.BackColor = Color.FromArgb(248, 250, 252);

            // Split de Ventas y Detalle
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 530,
                BackColor = Color.FromArgb(226, 232, 240)
            };

            dgvVentas = new DataGridView
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
            ConfigurarColumnasVentas();
            dgvVentas.SelectionChanged += DgvVentas_SelectionChanged;

            dgvDetalle = new DataGridView
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
            ConfigurarColumnasDetalle();

            var pnlDetalleWrapper = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };
            var lblDetalleHeader = new Label { Text = "Productos del Ticket Seleccionado:", Font = new Font("Segoe UI", 9, FontStyle.Bold), Dock = DockStyle.Top, Height = 25 };
            pnlDetalleWrapper.Controls.Add(dgvDetalle);
            pnlDetalleWrapper.Controls.Add(lblDetalleHeader);

            split.Panel1.Controls.Add(dgvVentas);
            split.Panel2.Controls.Add(pnlDetalleWrapper);
            tabHistorial.Controls.Add(split);

            // Pestaña Top Productos
            var tabTop = new TabPage(" Top Productos Más Vendidos");
            tabTop.BackColor = Color.FromArgb(248, 250, 252);
            tabTop.Padding = new Padding(15);

            dgvTopProductos = new DataGridView
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
            ConfigurarColumnasTop();
            tabTop.Controls.Add(dgvTopProductos);

            tabControl.TabPages.Add(tabHistorial);
            tabControl.TabPages.Add(tabTop);

            var pnlContenido = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15, 0, 15, 15) };
            pnlContenido.Controls.Add(tabControl);

            this.Controls.Add(pnlContenido);
            this.Controls.Add(pnlKpis);
            this.Controls.Add(pnlFiltros);
            this.Controls.Add(pnlTop);
        }

        private Button CrearBotonFiltro(string texto, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = texto,
                Size = new Size(textsWidth(texto), 28),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btn.Click += onClick;
            return btn;
        }

        private static int textsWidth(string text) => text.Length * 8 + 30;

        private void ConfigurarColumnasVentas()
        {
            dgvVentas.Columns.Clear();
            dgvVentas.Columns.Add("IdVenta", "Folio");
            dgvVentas.Columns.Add("Fecha", "Fecha y Hora");
            dgvVentas.Columns.Add("Cliente", "Cliente");
            dgvVentas.Columns.Add("Cajero", "Cajero");
            dgvVentas.Columns.Add("Total", "Total");
            dgvVentas.Columns.Add("Estado", "Estado");

            dgvVentas.Columns["IdVenta"]!.Width = 60;
            dgvVentas.Columns["Fecha"]!.Width = 135;
            dgvVentas.Columns["Total"]!.Width = 85;
            dgvVentas.Columns["Total"]!.DefaultCellStyle.Format = "C2";
            dgvVentas.Columns["Estado"]!.Width = 85;
        }

        private void ConfigurarColumnasDetalle()
        {
            dgvDetalle.Columns.Clear();
            dgvDetalle.Columns.Add("Producto", "Producto");
            dgvDetalle.Columns.Add("Cantidad", "Cant.");
            dgvDetalle.Columns.Add("PrecioUnitario", "Precio");
            dgvDetalle.Columns.Add("Subtotal", "Subtotal");

            dgvDetalle.Columns["Cantidad"]!.Width = 50;
            dgvDetalle.Columns["PrecioUnitario"]!.Width = 75;
            dgvDetalle.Columns["PrecioUnitario"]!.DefaultCellStyle.Format = "C2";
            dgvDetalle.Columns["Subtotal"]!.Width = 75;
            dgvDetalle.Columns["Subtotal"]!.DefaultCellStyle.Format = "C2";
        }

        private void ConfigurarColumnasTop()
        {
            dgvTopProductos.Columns.Clear();
            dgvTopProductos.Columns.Add("Codigo", "Código");
            dgvTopProductos.Columns.Add("Nombre", "Nombre del Producto");
            dgvTopProductos.Columns.Add("CantidadVendida", "Unidades Vendidas");
            dgvTopProductos.Columns.Add("TotalRecaudado", "Ingresos Totales");

            dgvTopProductos.Columns["Codigo"]!.Width = 120;
            dgvTopProductos.Columns["CantidadVendida"]!.Width = 150;
            dgvTopProductos.Columns["TotalRecaudado"]!.Width = 150;
            dgvTopProductos.Columns["TotalRecaudado"]!.DefaultCellStyle.Format = "C2";
        }

        private void EstablecerFiltroHoy()
        {
            dtpInicio.Value = DateTime.Today;
            dtpFin.Value = DateTime.Today.AddDays(1).AddSeconds(-1);
            CargarDatos();
        }

        private void EstablecerFiltroSemana()
        {
            dtpInicio.Value = DateTime.Today.AddDays(-7);
            dtpFin.Value = DateTime.Today.AddDays(1).AddSeconds(-1);
            CargarDatos();
        }

        private void EstablecerFiltroMes()
        {
            dtpInicio.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            dtpFin.Value = DateTime.Today.AddDays(1).AddSeconds(-1);
            CargarDatos();
        }

        private void CargarDatos()
        {
            DateTime fInicio = dtpInicio.Value.Date;
            DateTime fFin = dtpFin.Value.Date.AddDays(1).AddSeconds(-1);

            try
            {
                _ventasActuales = _ventaService.HistorialVentasFiltrado(fInicio, fFin);
            }
            catch
            {
                _ventasActuales = new List<Venta>();
            }

            dgvVentas.Rows.Clear();
            dgvDetalle.Rows.Clear();

            decimal totalVendido = 0;
            foreach (var v in _ventasActuales)
            {
                totalVendido += v.Total;
                dgvVentas.Rows.Add(
                    v.IdVenta,
                    v.Fecha.ToString("dd/MM/yyyy HH:mm"),
                    v.ClienteNombre ?? "Consumidor Final",
                    v.UsuarioNombre ?? "Cajero",
                    v.Total,
                    v.Estado
                );
            }

            // Calcular 
            int totalTransacciones = _ventasActuales.Count;
            decimal ticketPromedio = totalTransacciones > 0 ? totalVendido / totalTransacciones : 0;

            lblKpiTotalVentas.Text = totalVendido.ToString("C2");
            lblKpiTransacciones.Text = $"{totalTransacciones} ventas";
            lblKpiTicketPromedio.Text = ticketPromedio.ToString("C2");

            try
            {
                var resumen = _ventaService.ObtenerResumen(fInicio, fFin);
                lblKpiArticulos.Text = $"{resumen.TotalArticulos} unid.";
            }
            catch
            {
                lblKpiArticulos.Text = "-";
            }

            // Cargar info Top Productos
            dgvTopProductos.Rows.Clear();
            try
            {
                var topProds = _ventaService.ObtenerTopProductos(fInicio, fFin, 10);
                foreach (var tp in topProds)
                {
                    dgvTopProductos.Rows.Add(tp.Codigo, tp.Nombre, tp.CantidadVendida, tp.TotalRecaudado);
                }
            }
            catch { }

            if (dgvVentas.Rows.Count > 0)
                dgvVentas.Rows[0].Selected = true;
        }

        private void DgvVentas_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvVentas.SelectedRows.Count == 0)
            {
                dgvDetalle.Rows.Clear();
                return;
            }

            int idVenta = Convert.ToInt32(dgvVentas.SelectedRows[0].Cells["IdVenta"].Value);
            CargarDetallesDeVenta(idVenta);
        }

        private void CargarDetallesDeVenta(int idVenta)
        {
            dgvDetalle.Rows.Clear();
            try
            {
                var detalles = _ventaService.ObtenerDetallesPorVenta(idVenta);
                foreach (var d in detalles)
                {
                    dgvDetalle.Rows.Add(
                        d.ProductoNombre ?? "Producto",
                        d.Cantidad,
                        d.PrecioUnitario,
                        d.Subtotal
                    );
                }
            }
            catch { }
        }

        private void BtnVerTicket_Click(object? sender, EventArgs e)
        {
            if (dgvVentas.SelectedRows.Count == 0)
            {
                MessageBox.Show("Selecciona una venta de la lista para ver su ticket.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int idVenta = Convert.ToInt32(dgvVentas.SelectedRows[0].Cells["IdVenta"].Value);
            var venta = _ventasActuales.FirstOrDefault(x => x.IdVenta == idVenta);
            if (venta == null) return;

            var detalles = _ventaService.ObtenerDetallesPorVenta(idVenta);

            string ticket = 
                "=========================================\n" +
                "       COPIA DE TICKET DE VENTA          \n" +
                "=========================================\n" +
                $" Folio de Venta : #{venta.IdVenta}\n" +
                $" Fecha / Hora   : {venta.Fecha:dd/MM/yyyy HH:mm:ss}\n" +
                $" Cajero         : {venta.UsuarioNombre ?? "Cajero"}\n" +
                $" Cliente        : {venta.ClienteNombre ?? "Consumidor Final"}\n" +
                $" Estado         : {venta.Estado}\n" +
                "-----------------------------------------\n" +
                string.Join("\n", detalles.Select(d => $" {d.Cantidad}x {d.ProductoNombre} (${d.PrecioUnitario:F2}) = {d.Subtotal:C2}")) + "\n" +
                "-----------------------------------------\n" +
                $" Subtotal : {venta.Subtotal:C2}\n" +
                $" IVA (16%): {venta.Impuesto:C2}\n" +
                $" TOTAL    : {venta.Total:C2}\n" +
                "=========================================\n";

            MessageBox.Show(ticket, $"Ticket Venta #{idVenta}", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
