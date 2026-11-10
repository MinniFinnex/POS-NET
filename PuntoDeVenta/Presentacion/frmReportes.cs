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

        // Tarjetas
        private Label lblKpiTotalVentas = null!;
        private Label lblKpiTransacciones = null!;
        private Label lblKpiTicketPromedio = null!;
        private Label lblKpiArticulos = null!;

        // Grids
        private DataGridView dgvVentas = null!;
        private DataGridView dgvDetalle = null!;

        private List<Venta> _ventasActuales = new();

        public frmReportes(Usuario usuario)
        {
            _usuario = usuario;
            InicializarComponentes();
            EstablecerFiltroHoy();
        }

        private void InicializarComponentes()
        {
            this.Text = "Reportes";
            this.BackColor = TemaUI.FondoClaro;
            this.Font = new Font("Segoe UI", 9.5f);

            // Encabezado
            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = TemaUI.Blanco,
                Padding = new Padding(20, 10, 20, 10)
            };

            var pnlTopBorder = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = TemaUI.Borde
            };
            pnlTop.Controls.Add(pnlTopBorder);

            var lblTitulo = new Label
            {
                Text = "Reportes y Ventas",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = TemaUI.TextoOscuro,
                Location = new Point(20, 14),
                AutoSize = true
            };
            pnlTop.Controls.Add(lblTitulo);

            // Filtros
            var pnlFiltros = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = TemaUI.Blanco,
                Padding = new Padding(15, 8, 15, 8)
            };

            var pnlFiltrosBorder = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = TemaUI.Borde
            };
            pnlFiltros.Controls.Add(pnlFiltrosBorder);

            btnHoy = new Button { Text = "Hoy", Location = new Point(15, 11), Size = new Size(60, 28) };
            TemaUI.AplicarBotonSecundario(btnHoy, new Font("Segoe UI", 8.5f, FontStyle.Bold));
            btnHoy.Click += (s, e) => EstablecerFiltroHoy();

            btnSemana = new Button { Text = "7 Días", Location = new Point(80, 11), Size = new Size(65, 28) };
            TemaUI.AplicarBotonSecundario(btnSemana, new Font("Segoe UI", 8.5f, FontStyle.Bold));
            btnSemana.Click += (s, e) => EstablecerFiltroSemana();

            btnMes = new Button { Text = "Mes", Location = new Point(150, 11), Size = new Size(60, 28) };
            TemaUI.AplicarBotonSecundario(btnMes, new Font("Segoe UI", 8.5f, FontStyle.Bold));
            btnMes.Click += (s, e) => EstablecerFiltroMes();

            var lblDesde = new Label { Text = "Desde:", Location = new Point(225, 16), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = TemaUI.TextoOscuro };
            dtpInicio = new DateTimePicker { Location = new Point(275, 13), Size = new Size(115, 25), Format = DateTimePickerFormat.Short };

            var lblHasta = new Label { Text = "Hasta:", Location = new Point(400, 16), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = TemaUI.TextoOscuro };
            dtpFin = new DateTimePicker { Location = new Point(450, 13), Size = new Size(115, 25), Format = DateTimePickerFormat.Short };

            btnFiltrar = new Button
            {
                Text = "Consultar",
                Location = new Point(580, 11),
                Size = new Size(95, 28)
            };
            TemaUI.AplicarBotonAzul(btnFiltrar, new Font("Segoe UI", 9, FontStyle.Bold));
            btnFiltrar.Click += (s, e) => CargarDatos();

            btnVerTicket = new Button
            {
                Text = "Ver Ticket",
                Location = new Point(685, 11),
                Size = new Size(95, 28)
            };
            TemaUI.AplicarBotonAzul(btnVerTicket, new Font("Segoe UI", 9, FontStyle.Bold));
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

            // Tarjetas KPI
            var pnlKpis = new Panel
            {
                Dock = DockStyle.Top,
                Height = 78,
                Padding = new Padding(15, 8, 15, 6)
            };

            Panel CrearTarjetaKpi(string titulo, out Label lblValor, int left)
            {
                var card = new Panel
                {
                    Location = new Point(left, 4),
                    Size = new Size(190, 64),
                    BackColor = TemaUI.Blanco,
                    BorderStyle = BorderStyle.FixedSingle
                };

                var lblT = new Label
                {
                    Text = titulo,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    ForeColor = TemaUI.TextoSutil,
                    Location = new Point(10, 8),
                    AutoSize = true
                };

                lblValor = new Label
                {
                    Text = "$0.00",
                    Font = new Font("Segoe UI", 13, FontStyle.Bold),
                    ForeColor = TemaUI.AzulMarino,
                    Location = new Point(10, 28),
                    AutoSize = true
                };

                card.Controls.Add(lblT);
                card.Controls.Add(lblValor);
                return card;
            }

            pnlKpis.Controls.Add(CrearTarjetaKpi("TOTAL VENTAS", out lblKpiTotalVentas, 15));
            pnlKpis.Controls.Add(CrearTarjetaKpi("Nº VENTAS", out lblKpiTransacciones, 215));
            pnlKpis.Controls.Add(CrearTarjetaKpi("TICKET PROMEDIO", out lblKpiTicketPromedio, 415));
            pnlKpis.Controls.Add(CrearTarjetaKpi("ARTÍCULOS VENDIDOS", out lblKpiArticulos, 615));

            // Split de Ventas y Detalle
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 500,
                BackColor = TemaUI.Borde
            };

            dgvVentas = new DataGridView
            {
                Dock = DockStyle.Fill
            };
            TemaUI.AplicarEstiloGrid(dgvVentas);
            ConfigurarColumnasVentas();
            dgvVentas.SelectionChanged += DgvVentas_SelectionChanged;

            dgvDetalle = new DataGridView
            {
                Dock = DockStyle.Fill
            };
            TemaUI.AplicarEstiloGrid(dgvDetalle);
            ConfigurarColumnasDetalle();

            var pnlDetalleWrapper = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = TemaUI.Blanco,
                Padding = new Padding(8)
            };

            var lblDetalleHeader = new Label
            {
                Text = "Detalle del Ticket",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = TemaUI.TextoOscuro,
                Dock = DockStyle.Top,
                Height = 26
            };
            pnlDetalleWrapper.Controls.Add(dgvDetalle);
            pnlDetalleWrapper.Controls.Add(lblDetalleHeader);

            split.Panel1.Controls.Add(dgvVentas);
            split.Panel2.Controls.Add(pnlDetalleWrapper);

            var pnlContenido = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15, 0, 15, 15)
            };
            pnlContenido.Controls.Add(split);

            this.Controls.Add(pnlContenido);
            this.Controls.Add(pnlKpis);
            this.Controls.Add(pnlFiltros);
            this.Controls.Add(pnlTop);
        }

        private void ConfigurarColumnasVentas()
        {
            dgvVentas.Columns.Clear();
            dgvVentas.Columns.Add("IdVenta", "Folio");
            dgvVentas.Columns.Add("Fecha", "Fecha");
            dgvVentas.Columns.Add("Cliente", "Cliente");
            dgvVentas.Columns.Add("Cajero", "Cajero");
            dgvVentas.Columns.Add("Total", "Total");
            dgvVentas.Columns.Add("Estado", "Estado");

            dgvVentas.Columns["IdVenta"]!.Width = 65;
            dgvVentas.Columns["Fecha"]!.Width = 125;
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

            dgvDetalle.Columns["Cantidad"]!.Width = 55;
            dgvDetalle.Columns["PrecioUnitario"]!.Width = 80;
            dgvDetalle.Columns["PrecioUnitario"]!.DefaultCellStyle.Format = "C2";
            dgvDetalle.Columns["Subtotal"]!.Width = 85;
            dgvDetalle.Columns["Subtotal"]!.DefaultCellStyle.Format = "C2";
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

            int totalTransacciones = _ventasActuales.Count;
            decimal ticketPromedio = totalTransacciones > 0 ? totalVendido / totalTransacciones : 0;

            lblKpiTotalVentas.Text = totalVendido.ToString("C2");
            lblKpiTransacciones.Text = $"{totalTransacciones}";
            lblKpiTicketPromedio.Text = ticketPromedio.ToString("C2");

            try
            {
                var resumen = _ventaService.ObtenerResumen(fInicio, fFin);
                lblKpiArticulos.Text = $"{resumen.TotalArticulos}";
            }
            catch
            {
                lblKpiArticulos.Text = "-";
            }

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
                "TICKET DE VENTA\n" +
                "-----------------------------------------\n" +
                $"Folio: #{venta.IdVenta}\n" +
                $"Fecha: {venta.Fecha:dd/MM/yyyy HH:mm}\n" +
                $"Cajero: {venta.UsuarioNombre ?? "Cajero"}\n" +
                $"Cliente: {venta.ClienteNombre ?? "Consumidor Final"}\n" +
                $"Estado: {venta.Estado}\n" +
                "-----------------------------------------\n" +
                string.Join("\n", detalles.Select(d => $"{d.Cantidad}x {d.ProductoNombre} (${d.PrecioUnitario:F2}) = {d.Subtotal:C2}")) + "\n" +
                "-----------------------------------------\n" +
                $"TOTAL: {venta.Total:C2}\n" +
                "-----------------------------------------\n";

            MessageBox.Show(ticket, $"Ticket #{idVenta}", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
