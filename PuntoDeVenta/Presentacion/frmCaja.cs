using PuntoDeVenta.Entidades;
using PuntoDeVenta.Negocio;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PuntoDeVenta.Presentacion
{
    public class frmCaja : Form
    {
        private readonly Usuario _usuario;
        private readonly CajaService _cajaService = new();

        private Caja? _cajaActual;

        // Paneles dinámicos
        private Panel pnlApertura = null!;
        private Panel pnlCierre = null!;
        private Label lblEstadoTurno = null!;

        // Controles Apertura
        private TextBox txtMontoInicial = null!;
        private TextBox txtObsApertura = null!;
        private Button btnAbrir = null!;

        // Controles Cierre
        private Label lblMontoInicialVal = null!;
        private Label lblVentasTurnoVal = null!;
        private Label lblTotalEsperadoVal = null!;
        private TextBox txtMontoCierre = null!;
        private Label lblDiferenciaVal = null!;
        private TextBox txtObsCierre = null!;
        private Button btnCerrar = null!;

        // Historial
        private DataGridView dgvHistorial = null!;

        public frmCaja(Usuario usuario)
        {
            _usuario = usuario;
            InicializarComponentes();
            RefrescarEstado();
            CargarHistorial();
        }

        private void InicializarComponentes()
        {
            this.Text = "Control de Apertura y Cierre de Caja";
            this.Size = new Size(860, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 9.5f);

            // Encabezado
            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(20, 12, 20, 12),
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblTitulo = new Label
            {
                Text = " Control de Turnos y Caja Registradora",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(20, 16),
                AutoSize = true
            };

            lblEstadoTurno = new Label
            {
                Text = "Estado: ...",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(540, 18),
                AutoSize = true
            };

            pnlTop.Controls.Add(lblTitulo);
            pnlTop.Controls.Add(lblEstadoTurno);

            // Contenedor principal dividido
            var pnlCentro = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15)
            };

            // Panel Apertura
            pnlApertura = new Panel
            {
                Location = new Point(15, 10),
                Size = new Size(815, 175),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false
            };

            var lblApTitulo = new Label { Text = "Apertura de Turno / Caja", Font = new Font("Segoe UI", 11, FontStyle.Bold), Location = new Point(15, 12), AutoSize = true, ForeColor = Color.FromArgb(15, 23, 42) };
            var lblApSub = new Label { Text = "Ingresa el fondo de cambio inicial en efectivo para comenzar a registrar ventas:", Font = new Font("Segoe UI", 8.5f), Location = new Point(15, 36), AutoSize = true, ForeColor = Color.FromArgb(100, 116, 139) };

            var lblIni = new Label { Text = "Fondo Inicial en Efectivo ($):", Location = new Point(15, 68), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            txtMontoInicial = new TextBox { Location = new Point(15, 90), Size = new Size(180, 26), Font = new Font("Segoe UI", 10, FontStyle.Bold), Text = "500.00" };

            var lblObsAp = new Label { Text = "Observaciones (Opcional):", Location = new Point(215, 68), AutoSize = true, Font = new Font("Segoe UI", 9) };
            txtObsApertura = new TextBox { Location = new Point(215, 90), Size = new Size(380, 26), Font = new Font("Segoe UI", 9.5f) };

            btnAbrir = new Button
            {
                Text = "ABRIR CAJA",
                Location = new Point(615, 80),
                Size = new Size(175, 42),
                BackColor = Color.FromArgb(22, 163, 74),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAbrir.FlatAppearance.BorderSize = 0;
            btnAbrir.Click += BtnAbrir_Click;

            pnlApertura.Controls.Add(lblApTitulo);
            pnlApertura.Controls.Add(lblApSub);
            pnlApertura.Controls.Add(lblIni);
            pnlApertura.Controls.Add(txtMontoInicial);
            pnlApertura.Controls.Add(lblObsAp);
            pnlApertura.Controls.Add(txtObsApertura);
            pnlApertura.Controls.Add(btnAbrir);

            // Panel Cierre
            pnlCierre = new Panel
            {
                Location = new Point(15, 10),
                Size = new Size(815, 175),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false
            };

            var lblCiTitulo = new Label { Text = "Cierre y Arqueo de Caja", Font = new Font("Segoe UI", 11, FontStyle.Bold), Location = new Point(15, 10), AutoSize = true, ForeColor = Color.FromArgb(15, 23, 42) };

            var lblIniText = new Label { Text = "Fondo Inicial:", Location = new Point(15, 38), AutoSize = true, Font = new Font("Segoe UI", 9) };
            lblMontoInicialVal = new Label { Text = "$0.00", Location = new Point(110, 38), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };

            var lblVenText = new Label { Text = "Ventas Turno:", Location = new Point(200, 38), AutoSize = true, Font = new Font("Segoe UI", 9) };
            lblVentasTurnoVal = new Label { Text = "$0.00", Location = new Point(295, 38), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(22, 163, 74) };

            var lblEspText = new Label { Text = "Total Esperado:", Location = new Point(400, 38), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            lblTotalEsperadoVal = new Label { Text = "$0.00", Location = new Point(505, 38), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235) };

            var lblContado = new Label { Text = "Efectivo Físico Contado ($):", Location = new Point(15, 75), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            txtMontoCierre = new TextBox { Location = new Point(15, 98), Size = new Size(180, 26), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            txtMontoCierre.TextChanged += (s, e) => CalcularDiferencia();

            var lblDif = new Label { Text = "Diferencia:", Location = new Point(205, 75), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            lblDiferenciaVal = new Label { Text = "$0.00 (Cuadre Exacto)", Location = new Point(205, 100), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139) };

            var lblObsCi = new Label { Text = "Observaciones de Cierre:", Location = new Point(390, 75), AutoSize = true, Font = new Font("Segoe UI", 9) };
            txtObsCierre = new TextBox { Location = new Point(390, 98), Size = new Size(220, 26), Font = new Font("Segoe UI", 9.5f) };

            btnCerrar = new Button
            {
                Text = " REALIZAR CIERRE",
                Location = new Point(625, 85),
                Size = new Size(175, 45),
                BackColor = Color.FromArgb(220, 38, 38),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.Click += BtnCerrar_Click;

            pnlCierre.Controls.Add(lblCiTitulo);
            pnlCierre.Controls.Add(lblIniText);
            pnlCierre.Controls.Add(lblMontoInicialVal);
            pnlCierre.Controls.Add(lblVenText);
            pnlCierre.Controls.Add(lblVentasTurnoVal);
            pnlCierre.Controls.Add(lblEspText);
            pnlCierre.Controls.Add(lblTotalEsperadoVal);
            pnlCierre.Controls.Add(lblContado);
            pnlCierre.Controls.Add(txtMontoCierre);
            pnlCierre.Controls.Add(lblDif);
            pnlCierre.Controls.Add(lblDiferenciaVal);
            pnlCierre.Controls.Add(lblObsCi);
            pnlCierre.Controls.Add(txtObsCierre);
            pnlCierre.Controls.Add(btnCerrar);

            // Historial de Cajas
            var lblHistTitulo = new Label
            {
                Text = " Historial de Turnos y Arqueos de Caja",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(15, 195),
                AutoSize = true,
                ForeColor = Color.FromArgb(30, 41, 59)
            };

            var btnRefrescar = new Button
            {
                Text = " Actualizar",
                Location = new Point(715, 190),
                Size = new Size(115, 26),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f),
                Cursor = Cursors.Hand
            };
            btnRefrescar.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnRefrescar.Click += (s, e) => { RefrescarEstado(); CargarHistorial(); };

            dgvHistorial = new DataGridView
            {
                Location = new Point(15, 225),
                Size = new Size(815, 290),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                AllowUserToAddRows = false
            };
            ConfigurarColumnasHistorial();

            pnlCentro.Controls.Add(pnlApertura);
            pnlCentro.Controls.Add(pnlCierre);
            pnlCentro.Controls.Add(lblHistTitulo);
            pnlCentro.Controls.Add(btnRefrescar);
            pnlCentro.Controls.Add(dgvHistorial);

            this.Controls.Add(pnlCentro);
            this.Controls.Add(pnlTop);
        }

        private void ConfigurarColumnasHistorial()
        {
            dgvHistorial.Columns.Clear();
            dgvHistorial.Columns.Add("IdCaja", "Nº Turno");
            dgvHistorial.Columns.Add("Usuario", "Cajero");
            dgvHistorial.Columns.Add("Apertura", "F. Apertura");
            dgvHistorial.Columns.Add("MontoInicial", "Fondo Inicial");
            dgvHistorial.Columns.Add("Ventas", "Ventas Turno");
            dgvHistorial.Columns.Add("Cierre", "F. Cierre");
            dgvHistorial.Columns.Add("MontoCierre", "Contado");
            dgvHistorial.Columns.Add("Diferencia", "Diferencia");
            dgvHistorial.Columns.Add("Estado", "Estado");

            dgvHistorial.Columns["IdCaja"]!.Width = 65;
            dgvHistorial.Columns["MontoInicial"]!.DefaultCellStyle.Format = "C2";
            dgvHistorial.Columns["Ventas"]!.DefaultCellStyle.Format = "C2";
            dgvHistorial.Columns["MontoCierre"]!.DefaultCellStyle.Format = "C2";
            dgvHistorial.Columns["Diferencia"]!.DefaultCellStyle.Format = "C2";
        }

        public void RefrescarEstado()
        {
            try
            {
                _cajaActual = _cajaService.ObtenerCajaAbierta(_usuario.IdUsuario > 0 ? _usuario.IdUsuario : 1);
            }
            catch
            {
                _cajaActual = null;
            }

            if (_cajaActual == null)
            {
                lblEstadoTurno.Text = " CAJA CERRADA";
                lblEstadoTurno.ForeColor = Color.FromArgb(185, 28, 28);
                pnlApertura.Visible = true;
                pnlCierre.Visible = false;
                txtMontoInicial.Focus();
            }
            else
            {
                lblEstadoTurno.Text = $" CAJA ABIERTA #{_cajaActual.IdCaja} ({_cajaActual.FechaApertura:HH:mm})";
                lblEstadoTurno.ForeColor = Color.FromArgb(22, 101, 52);
                pnlApertura.Visible = false;
                pnlCierre.Visible = true;

                lblMontoInicialVal.Text = _cajaActual.MontoInicial.ToString("C2");
                lblVentasTurnoVal.Text = _cajaActual.MontoVentas.ToString("C2");
                decimal esperado = _cajaActual.TotalEsperado;
                lblTotalEsperadoVal.Text = esperado.ToString("C2");
                txtMontoCierre.Text = esperado.ToString("F2");
                CalcularDiferencia();
            }
        }

        private void CalcularDiferencia()
        {
            if (_cajaActual == null) return;
            decimal esperado = _cajaActual.TotalEsperado;

            if (decimal.TryParse(txtMontoCierre.Text, out decimal contado))
            {
                decimal diff = contado - esperado;
                if (diff == 0)
                {
                    lblDiferenciaVal.Text = "$0.00 (Exacto)";
                    lblDiferenciaVal.ForeColor = Color.FromArgb(22, 101, 52);
                }
                else if (diff > 0)
                {
                    lblDiferenciaVal.Text = $"+{diff:C2} (Sobrante)";
                    lblDiferenciaVal.ForeColor = Color.FromArgb(37, 99, 235);
                }
                else
                {
                    lblDiferenciaVal.Text = $"{diff:C2} (Faltante)";
                    lblDiferenciaVal.ForeColor = Color.FromArgb(220, 38, 38);
                }
            }
            else
            {
                lblDiferenciaVal.Text = "Monto inválido";
                lblDiferenciaVal.ForeColor = Color.DarkGray;
            }
        }

        private void CargarHistorial()
        {
            dgvHistorial.Rows.Clear();
            try
            {
                var historial = _cajaService.ListarHistorialCajas();
                foreach (var c in historial)
                {
                    dgvHistorial.Rows.Add(
                        c.IdCaja,
                        c.UsuarioNombre ?? "Cajero",
                        c.FechaApertura.ToString("dd/MM/yyyy HH:mm"),
                        c.MontoInicial,
                        c.MontoVentas,
                        c.FechaCierre.HasValue ? c.FechaCierre.Value.ToString("dd/MM/yyyy HH:mm") : "En curso",
                        c.MontoCierre.HasValue ? c.MontoCierre.Value : (object)"-",
                        c.Diferencia.HasValue ? c.Diferencia.Value : (object)"-",
                        c.Estado
                    );
                }
            }
            catch { }
        }

        private void BtnAbrir_Click(object? sender, EventArgs e)
        {
            if (!decimal.TryParse(txtMontoInicial.Text, out decimal montoInicial) || montoInicial < 0)
            {
                MessageBox.Show("Ingresa un monto inicial válido mayor o igual a 0.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMontoInicial.Focus();
                return;
            }

            int idUsr = _usuario.IdUsuario > 0 ? _usuario.IdUsuario : 1;
            var res = _cajaService.AbrirCaja(idUsr, montoInicial, txtObsApertura.Text.Trim());

            MessageBox.Show(res.Mensaje, res.Exito ? "Caja Abierta" : "Aviso", MessageBoxButtons.OK, res.Exito ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

            if (res.Exito)
            {
                RefrescarEstado();
                CargarHistorial();
            }
        }

        private void BtnCerrar_Click(object? sender, EventArgs e)
        {
            if (_cajaActual == null) return;

            if (!decimal.TryParse(txtMontoCierre.Text, out decimal montoCierre) || montoCierre < 0)
            {
                MessageBox.Show("Ingresa el monto físico contado en caja.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMontoCierre.Focus();
                return;
            }

            decimal esperado = _cajaActual.TotalEsperado;
            decimal diff = montoCierre - esperado;

            string resumen = 
                "¿Deseas confirmar el cierre de turno con los siguientes datos?\n\n" +
                $"• Fondo Inicial  : {_cajaActual.MontoInicial:C2}\n" +
                $"• Ventas Turno   : {_cajaActual.MontoVentas:C2}\n" +
                $"• Total Esperado : {esperado:C2}\n" +
                $"• Total Contado  : {montoCierre:C2}\n" +
                $"• Diferencia     : {diff:C2}\n";

            var confirm = MessageBox.Show(resumen, "Confirmar Cierre de Caja", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                var res = _cajaService.CerrarCaja(_cajaActual.IdCaja, montoCierre, txtObsCierre.Text.Trim());
                MessageBox.Show(res.Mensaje, res.Exito ? "Éxito" : "Aviso", MessageBoxButtons.OK, res.Exito ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                if (res.Exito)
                {
                    RefrescarEstado();
                    CargarHistorial();
                }
            }
        }
    }
}
