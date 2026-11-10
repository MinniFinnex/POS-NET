using PuntoDeVenta.Entidades;
using PuntoDeVenta.Negocio;
using PuntoDeVenta.Utilidades;
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

        private Panel pnlApertura = null!;
        private Panel pnlCierre = null!;
        private Label lblEstadoTurno = null!;

        // Apertura
        private TextBox txtMontoInicial = null!;
        private TextBox txtObsApertura = null!;
        private Button btnAbrir = null!;

        // Cierre
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
            this.Text = "Caja";
            this.Size = new Size(880, 620);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = TemaUI.FondoClaro;
            this.Font = new Font("Segoe UI", 9.5f);

            // Barra superior
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
                Text = "Control de Caja",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = TemaUI.TextoOscuro,
                Location = new Point(20, 14),
                AutoSize = true
            };

            lblEstadoTurno = new Label
            {
                Text = "Estado: ...",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(560, 16),
                AutoSize = true
            };

            pnlTop.Controls.Add(lblTitulo);
            pnlTop.Controls.Add(lblEstadoTurno);

            var pnlCentro = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15)
            };

            // Panel Apertura
            pnlApertura = new Panel
            {
                Location = new Point(15, 10),
                Size = new Size(830, 140),
                BackColor = TemaUI.Blanco,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false
            };

            var lblApTitulo = new Label
            {
                Text = "Apertura de Turno",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                Location = new Point(15, 12),
                AutoSize = true,
                ForeColor = TemaUI.TextoOscuro
            };

            var lblIni = new Label
            {
                Text = "Fondo Inicial ($):",
                Location = new Point(15, 45),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = TemaUI.TextoOscuro
            };
            txtMontoInicial = new TextBox
            {
                Location = new Point(15, 68),
                Size = new Size(160, 26),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Text = "500.00"
            };

            var lblObsAp = new Label
            {
                Text = "Observaciones (Opcional):",
                Location = new Point(195, 45),
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                ForeColor = TemaUI.TextoOscuro
            };
            txtObsApertura = new TextBox
            {
                Location = new Point(195, 68),
                Size = new Size(410, 26),
                Font = new Font("Segoe UI", 9.5f)
            };

            btnAbrir = new Button
            {
                Text = "Abrir Caja",
                Location = new Point(625, 62),
                Size = new Size(180, 36)
            };
            TemaUI.AplicarBotonAzul(btnAbrir, new Font("Segoe UI", 9.5f, FontStyle.Bold));
            btnAbrir.Click += BtnAbrir_Click;

            pnlApertura.Controls.Add(lblApTitulo);
            pnlApertura.Controls.Add(lblIni);
            pnlApertura.Controls.Add(txtMontoInicial);
            pnlApertura.Controls.Add(lblObsAp);
            pnlApertura.Controls.Add(txtObsApertura);
            pnlApertura.Controls.Add(btnAbrir);

            // Panel Cierre
            pnlCierre = new Panel
            {
                Location = new Point(15, 10),
                Size = new Size(830, 155),
                BackColor = TemaUI.Blanco,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false
            };

            var lblCiTitulo = new Label
            {
                Text = "Cierre y Arqueo de Caja",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                Location = new Point(15, 10),
                AutoSize = true,
                ForeColor = TemaUI.TextoOscuro
            };

            var lblIniText = new Label
            {
                Text = "Fondo Inicial:",
                Location = new Point(15, 36),
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                ForeColor = TemaUI.TextoSutil
            };
            lblMontoInicialVal = new Label
            {
                Text = "$0.00",
                Location = new Point(100, 36),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = TemaUI.TextoOscuro
            };

            var lblVenText = new Label
            {
                Text = "Ventas:",
                Location = new Point(190, 36),
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                ForeColor = TemaUI.TextoSutil
            };
            lblVentasTurnoVal = new Label
            {
                Text = "$0.00",
                Location = new Point(245, 36),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = TemaUI.AzulMarino
            };

            var lblEspText = new Label
            {
                Text = "Esperado:",
                Location = new Point(340, 36),
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                ForeColor = TemaUI.TextoSutil
            };
            lblTotalEsperadoVal = new Label
            {
                Text = "$0.00",
                Location = new Point(410, 36),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = TemaUI.AzulMarino
            };

            var lblContado = new Label
            {
                Text = "Efectivo Contado ($):",
                Location = new Point(15, 68),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = TemaUI.TextoOscuro
            };
            txtMontoCierre = new TextBox
            {
                Location = new Point(15, 90),
                Size = new Size(160, 26),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            txtMontoCierre.TextChanged += (s, e) => CalcularDiferencia();

            var lblDif = new Label
            {
                Text = "Diferencia:",
                Location = new Point(195, 68),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = TemaUI.TextoOscuro
            };
            lblDiferenciaVal = new Label
            {
                Text = "$0.00 (Exacto)",
                Location = new Point(195, 93),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = TemaUI.TextoSutil
            };

            var lblObsCi = new Label
            {
                Text = "Observaciones:",
                Location = new Point(370, 68),
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                ForeColor = TemaUI.TextoOscuro
            };
            txtObsCierre = new TextBox
            {
                Location = new Point(370, 90),
                Size = new Size(240, 26),
                Font = new Font("Segoe UI", 9.5f)
            };

            btnCerrar = new Button
            {
                Text = "Cerrar Caja",
                Location = new Point(625, 80),
                Size = new Size(180, 40)
            };
            TemaUI.AplicarBotonAzul(btnCerrar, new Font("Segoe UI", 9.5f, FontStyle.Bold));
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

            // Historial
            var lblHistTitulo = new Label
            {
                Text = "Historial de Turnos",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(15, 175),
                AutoSize = true,
                ForeColor = TemaUI.TextoOscuro
            };

            var btnRefrescar = new Button
            {
                Text = "Actualizar",
                Location = new Point(735, 170),
                Size = new Size(110, 28)
            };
            TemaUI.AplicarBotonSecundario(btnRefrescar, new Font("Segoe UI", 8.5f, FontStyle.Bold));
            btnRefrescar.Click += (s, e) => { RefrescarEstado(); CargarHistorial(); };

            dgvHistorial = new DataGridView
            {
                Location = new Point(15, 205),
                Size = new Size(830, 335),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            TemaUI.AplicarEstiloGrid(dgvHistorial);
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
            dgvHistorial.Columns.Add("Apertura", "Apertura");
            dgvHistorial.Columns.Add("MontoInicial", "Fondo Inicial");
            dgvHistorial.Columns.Add("Ventas", "Ventas");
            dgvHistorial.Columns.Add("Cierre", "Cierre");
            dgvHistorial.Columns.Add("MontoCierre", "Contado");
            dgvHistorial.Columns.Add("Diferencia", "Diferencia");
            dgvHistorial.Columns.Add("Estado", "Estado");

            dgvHistorial.Columns["IdCaja"]!.Width = 70;
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
                lblEstadoTurno.Text = "Caja Cerrada";
                lblEstadoTurno.ForeColor = TemaUI.TextoSutil;
                pnlApertura.Visible = true;
                pnlCierre.Visible = false;
                txtMontoInicial.Focus();
            }
            else
            {
                lblEstadoTurno.Text = $"Caja #{_cajaActual.IdCaja} (Abierta {_cajaActual.FechaApertura:HH:mm})";
                lblEstadoTurno.ForeColor = TemaUI.AzulMarino;
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
                    lblDiferenciaVal.ForeColor = TemaUI.AzulMarino;
                }
                else if (diff > 0)
                {
                    lblDiferenciaVal.Text = $"+{diff:C2} (Sobrante)";
                    lblDiferenciaVal.ForeColor = TemaUI.AzulMarino;
                }
                else
                {
                    lblDiferenciaVal.Text = $"{diff:C2} (Faltante)";
                    lblDiferenciaVal.ForeColor = TemaUI.AzulMarino;
                }
            }
            else
            {
                lblDiferenciaVal.Text = "Monto inválido";
                lblDiferenciaVal.ForeColor = TemaUI.TextoSutil;
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
                "¿Confirmas el cierre de turno con los siguientes datos?\n\n" +
                $"Fondo Inicial: {_cajaActual.MontoInicial:C2}\n" +
                $"Ventas Turno: {_cajaActual.MontoVentas:C2}\n" +
                $"Total Esperado: {esperado:C2}\n" +
                $"Total Contado: {montoCierre:C2}\n" +
                $"Diferencia: {diff:C2}\n";

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
